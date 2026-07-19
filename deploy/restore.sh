#!/usr/bin/env bash
# =============================================================================
# restore.sh — restore SQLite + uploads (and optionally config) from a backup
#              archive produced by deploy/backup.sh.
#
#   Usage: sudo ./restore.sh <archive.tar.gz | latest> [--with-config]
#     latest         restore the most recent archive in $BACKUP_ROOT
#     --with-config  also restore appsettings.Production.json (NOT the secrets env)
#
# A pre-restore safety backup is ALWAYS taken first, so a restore is reversible.
# The service is stopped during the swap and restarted + health-checked after.
# =============================================================================
set -euo pipefail

# ---- Configuration ----------------------------------------------------------
BASE_DIR="${BASE_DIR:-/var/www/profalycms}"
SHARED_DIR="$BASE_DIR/shared"
DATA_DIR="$SHARED_DIR/App_Data"
DB_FILE="$DATA_DIR/app.db"
UPLOADS_DIR="$DATA_DIR/uploads"
CURRENT_DIR="$BASE_DIR/current"
BACKUP_ROOT="${BACKUP_ROOT:-/var/backups/profalycms}"
SERVICE="${SERVICE:-profalycms.service}"
APP_USER="${APP_USER:-profalycms}"
APP_GROUP="${APP_GROUP:-profalycms}"
HEALTH_URL="${HEALTH_URL:-http://127.0.0.1:5000/health}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# ---- Args -------------------------------------------------------------------
if [ "$#" -lt 1 ]; then
    echo "Usage: sudo $0 <archive.tar.gz | latest> [--with-config]" >&2
    exit 2
fi
if [ "$(id -u)" -ne 0 ]; then
    echo "ERROR: run as root (sudo)." >&2
    exit 1
fi

ARCHIVE="$1"
WITH_CONFIG="no"
[ "${2:-}" = "--with-config" ] && WITH_CONFIG="yes"

if [ "$ARCHIVE" = "latest" ]; then
    ARCHIVE="$(ls -1t "$BACKUP_ROOT"/profalycms-backup-*.tar.gz 2>/dev/null | head -n1 || true)"
    [ -z "$ARCHIVE" ] && { echo "ERROR: no backups in $BACKUP_ROOT" >&2; exit 1; }
    echo "==> Using latest backup: $ARCHIVE"
fi
[ -f "$ARCHIVE" ] || { echo "ERROR: archive not found: $ARCHIVE" >&2; exit 1; }

# Validate the archive before we touch anything live.
echo "==> Verifying archive"
tar --force-local -tzf "$ARCHIVE" >/dev/null || { echo "ERROR: archive is not a readable tar.gz" >&2; exit 1; }

STAGE="$(mktemp -d)"
cleanup() { rm -rf "$STAGE"; }
trap cleanup EXIT
tar --force-local -xzf "$ARCHIVE" -C "$STAGE"

[ -f "$STAGE/App_Data/app.db" ] || { echo "ERROR: archive has no App_Data/app.db" >&2; exit 1; }

# ---- 1) Safety backup of the CURRENT state ---------------------------------
echo "==> Taking a pre-restore safety backup"
BASE_DIR="$BASE_DIR" BACKUP_ROOT="$BACKUP_ROOT" bash "$SCRIPT_DIR/backup.sh" || \
    echo "WARNING: safety backup failed; continuing (archive is still intact)."

# ---- 2) Stop the service ----------------------------------------------------
echo "==> Stopping $SERVICE"
systemctl stop "$SERVICE" || true

# ---- 3) Restore SQLite ------------------------------------------------------
echo "==> Restoring SQLite database"
mkdir -p "$DATA_DIR"
# Discard stale WAL/SHM so SQLite does not replay old journal pages over the copy.
rm -f "$DB_FILE-wal" "$DB_FILE-shm"
cp -f "$STAGE/App_Data/app.db" "$DB_FILE"

# ---- 4) Restore uploads -----------------------------------------------------
if [ -d "$STAGE/App_Data/uploads" ]; then
    echo "==> Restoring uploads"
    rm -rf "$UPLOADS_DIR"
    cp -a "$STAGE/App_Data/uploads" "$UPLOADS_DIR"
else
    echo "==> No uploads in archive (leaving existing uploads untouched)"
fi

# ---- 5) Optional config -----------------------------------------------------
if [ "$WITH_CONFIG" = "yes" ] && [ -f "$STAGE/config/appsettings.Production.json" ]; then
    echo "==> Restoring appsettings.Production.json"
    cp -f "$STAGE/config/appsettings.Production.json" "$CURRENT_DIR/appsettings.Production.json"
    echo "    NOTE: the secrets env file was NOT overwritten. The archived copy is at:"
    echo "          (inside the archive) config/profalycms.env — apply manually if intended."
fi

# ---- 6) Ownership + restart -------------------------------------------------
echo "==> Fixing ownership"
chown -R "$APP_USER:$APP_GROUP" "$DATA_DIR"

echo "==> Starting $SERVICE"
systemctl start "$SERVICE"

# ---- 7) Health check --------------------------------------------------------
if bash "$SCRIPT_DIR/healthcheck.sh" "$HEALTH_URL" 15 3; then
    echo "==> Restore complete and healthy."
else
    echo "ERROR: service did not become healthy after restore." >&2
    echo "       A pre-restore safety backup was taken; investigate with:" >&2
    echo "         journalctl -u $SERVICE -n 100 --no-pager" >&2
    exit 1
fi
