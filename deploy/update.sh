#!/usr/bin/env bash
# =============================================================================
# update.sh — deploy a published artifact with atomic release swap + rollback.
#
#   Usage: sudo ./update.sh <artifact.tar.gz>
#
# Flow: unpack -> new release dir -> link shared data -> safety backup ->
#       atomically repoint "current" -> restart -> health check.
#       If the health check fails, "current" is rolled back to the previous
#       release and the service is restarted automatically.
#
# Layout under $BASE_DIR:
#   releases/<stamp>/   published binaries (App_Data & Logs are symlinks to shared/)
#   current  -> releases/<stamp>   (symlink the systemd unit runs)
#   shared/App_Data/{app.db,uploads,Backups}   persistent data (never replaced)
#   shared/Logs/
# =============================================================================
set -euo pipefail

# ---- Configuration ----------------------------------------------------------
BASE_DIR="${BASE_DIR:-/var/www/profalycms}"
RELEASES_DIR="$BASE_DIR/releases"
CURRENT_DIR="$BASE_DIR/current"
SHARED_DIR="$BASE_DIR/shared"
DATA_DIR="$SHARED_DIR/App_Data"
LOG_DIR="$SHARED_DIR/Logs"
SERVICE="${SERVICE:-profalycms.service}"
APP_USER="${APP_USER:-profalycms}"
APP_GROUP="${APP_GROUP:-profalycms}"
HEALTH_URL="${HEALTH_URL:-http://127.0.0.1:5000/health}"
KEEP_RELEASES="${KEEP_RELEASES:-5}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# ---- Args / preconditions ---------------------------------------------------
[ "$#" -ge 1 ] || { echo "Usage: sudo $0 <artifact.tar.gz>" >&2; exit 2; }
[ "$(id -u)" -eq 0 ] || { echo "ERROR: run as root (sudo)." >&2; exit 1; }
ARTIFACT="$1"
[ -f "$ARTIFACT" ] || { echo "ERROR: artifact not found: $ARTIFACT" >&2; exit 1; }
tar --force-local -tzf "$ARTIFACT" >/dev/null || { echo "ERROR: not a readable tar.gz" >&2; exit 1; }

STAMP="$(date -u +%Y%m%d-%H%M%S)"
NEW_RELEASE="$RELEASES_DIR/$STAMP"

# Remember what "current" points at now, for rollback.
PREVIOUS=""
if [ -L "$CURRENT_DIR" ]; then
    PREVIOUS="$(readlink -f "$CURRENT_DIR")"
fi

echo "==> Deploying release $STAMP"
mkdir -p "$RELEASES_DIR" "$DATA_DIR/uploads" "$DATA_DIR/Backups" "$LOG_DIR"

# ---- Unpack the new release -------------------------------------------------
mkdir -p "$NEW_RELEASE"
tar --force-local -xzf "$ARTIFACT" -C "$NEW_RELEASE"
[ -f "$NEW_RELEASE/ProfAly.CMS.Web.dll" ] || {
    echo "ERROR: artifact missing ProfAly.CMS.Web.dll" >&2
    rm -rf "$NEW_RELEASE"; exit 1
}

# ---- Link persistent data into the release ---------------------------------
# The app resolves App_Data/ and Logs/ relative to its content root; point them
# at the shared directories so data survives every release swap.
rm -rf "$NEW_RELEASE/App_Data" "$NEW_RELEASE/Logs"
ln -s "$DATA_DIR" "$NEW_RELEASE/App_Data"
ln -s "$LOG_DIR"  "$NEW_RELEASE/Logs"

chown -R "$APP_USER:$APP_GROUP" "$NEW_RELEASE"
chown -R "$APP_USER:$APP_GROUP" "$SHARED_DIR"

# ---- Safety backup before switching ----------------------------------------
echo "==> Safety backup before swap"
BASE_DIR="$BASE_DIR" bash "$SCRIPT_DIR/backup.sh" || \
    echo "WARNING: pre-deploy backup failed; continuing."

# ---- Atomically repoint "current" ------------------------------------------
echo "==> Switching current -> $STAMP"
ln -sfn "$NEW_RELEASE" "$CURRENT_DIR.tmp"
mv -Tf "$CURRENT_DIR.tmp" "$CURRENT_DIR"

# ---- Restart + health gate --------------------------------------------------
echo "==> Restarting $SERVICE"
systemctl restart "$SERVICE"

if bash "$SCRIPT_DIR/healthcheck.sh" "$HEALTH_URL" 15 3; then
    echo "==> Release $STAMP is healthy."
else
    echo "ERROR: new release unhealthy — ROLLING BACK." >&2
    if [ -n "$PREVIOUS" ] && [ -d "$PREVIOUS" ]; then
        ln -sfn "$PREVIOUS" "$CURRENT_DIR.tmp"
        mv -Tf "$CURRENT_DIR.tmp" "$CURRENT_DIR"
        systemctl restart "$SERVICE"
        if bash "$SCRIPT_DIR/healthcheck.sh" "$HEALTH_URL" 15 3; then
            echo "==> Rolled back to $(basename "$PREVIOUS") successfully." >&2
        else
            echo "CRITICAL: rollback also unhealthy. Inspect: journalctl -u $SERVICE -n 100" >&2
        fi
    else
        echo "CRITICAL: no previous release to roll back to." >&2
    fi
    exit 1
fi

# ---- Prune old releases -----------------------------------------------------
CURRENT_REAL="$(readlink -f "$CURRENT_DIR")"
if [ "$KEEP_RELEASES" -gt 0 ]; then
    mapfile -t ALL < <(ls -1dt "$RELEASES_DIR"/*/ 2>/dev/null | sed 's:/*$::')
    if [ "${#ALL[@]}" -gt "$KEEP_RELEASES" ]; then
        for rel in "${ALL[@]:$KEEP_RELEASES}"; do
            real="$(readlink -f "$rel")"
            # Never delete what current (or the previous rollback target) points to.
            if [ "$real" != "$CURRENT_REAL" ] && [ "$real" != "$PREVIOUS" ]; then
                rm -rf "$rel"; echo "    pruned old release $(basename "$rel")"
            fi
        done
    fi
fi

echo "==> Update complete: $STAMP"
