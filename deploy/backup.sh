#!/usr/bin/env bash
# =============================================================================
# backup.sh — full deployment backup: SQLite database + uploads + configuration.
#
# Produces a single timestamped tarball in $BACKUP_ROOT. Safe to run while the
# app is live: the SQLite database is snapshotted with the online-backup API
# (sqlite3 ".backup"), so it is consistent even with an active WAL.
#
#   Usage: sudo ./backup.sh
#
# Overridable via environment: BASE_DIR, BACKUP_ROOT, KEEP_LAST.
# Schedule from cron/systemd-timer (see docs/91_Production_Deployment_Guide.md).
# =============================================================================
set -euo pipefail

# ---- Configuration ----------------------------------------------------------
BASE_DIR="${BASE_DIR:-/var/www/profalycms}"
SHARED_DIR="$BASE_DIR/shared"
DATA_DIR="$SHARED_DIR/App_Data"
DB_FILE="$DATA_DIR/app.db"
UPLOADS_DIR="$DATA_DIR/uploads"
CURRENT_DIR="$BASE_DIR/current"
ENV_FILE="/etc/profalycms/profalycms.env"
BACKUP_ROOT="${BACKUP_ROOT:-/var/backups/profalycms}"
KEEP_LAST="${KEEP_LAST:-30}"   # number of full backups to retain (0 = keep all)

STAMP="$(date -u +%Y%m%d-%H%M%S)"
ARCHIVE="$BACKUP_ROOT/profalycms-backup-$STAMP.tar.gz"

STAGE="$(mktemp -d)"
cleanup() { rm -rf "$STAGE"; }
trap cleanup EXIT

echo "==> profalycms backup $STAMP"
mkdir -p "$BACKUP_ROOT"
mkdir -p "$STAGE/App_Data" "$STAGE/config"

# ---- 1) SQLite (consistent snapshot) ---------------------------------------
if [ -f "$DB_FILE" ]; then
    if command -v sqlite3 >/dev/null 2>&1; then
        echo "  - SQLite: online .backup snapshot"
        sqlite3 "$DB_FILE" ".backup '$STAGE/App_Data/app.db'"
    else
        echo "  - SQLite: sqlite3 not found; copying db + WAL/SHM sidecars"
        cp -a "$DB_FILE" "$STAGE/App_Data/app.db"
        [ -f "$DB_FILE-wal" ] && cp -a "$DB_FILE-wal" "$STAGE/App_Data/app.db-wal" || true
        [ -f "$DB_FILE-shm" ] && cp -a "$DB_FILE-shm" "$STAGE/App_Data/app.db-shm" || true
    fi
else
    echo "  - SQLite: WARNING no database at $DB_FILE (skipping)"
fi

# ---- 2) Uploads -------------------------------------------------------------
if [ -d "$UPLOADS_DIR" ]; then
    echo "  - Uploads: copying media tree"
    cp -a "$UPLOADS_DIR" "$STAGE/App_Data/uploads"
else
    echo "  - Uploads: none at $UPLOADS_DIR (skipping)"
fi

# ---- 3) Configuration -------------------------------------------------------
echo "  - Config: appsettings.Production.json + service env"
[ -f "$CURRENT_DIR/appsettings.Production.json" ] && \
    cp -a "$CURRENT_DIR/appsettings.Production.json" "$STAGE/config/" || \
    echo "    (no appsettings.Production.json in $CURRENT_DIR)"
[ -f "$ENV_FILE" ] && cp -a "$ENV_FILE" "$STAGE/config/profalycms.env" || \
    echo "    (no env file at $ENV_FILE)"

# ---- Manifest ---------------------------------------------------------------
DB_SIZE="$( [ -f "$STAGE/App_Data/app.db" ] && stat -c%s "$STAGE/App_Data/app.db" || echo 0 )"
cat > "$STAGE/manifest.txt" <<EOF
backup_utc=$STAMP
host=$(hostname)
db_bytes=$DB_SIZE
includes=sqlite,uploads,config
source=$BASE_DIR
EOF

# ---- Pack + verify ----------------------------------------------------------
echo "==> Packing $ARCHIVE"
tar --force-local -czf "$ARCHIVE" -C "$STAGE" .
# Verify the archive is readable (catches truncation/disk-full).
tar --force-local -tzf "$ARCHIVE" >/dev/null
chmod 600 "$ARCHIVE"

echo "==> Backup complete: $ARCHIVE ($(du -h "$ARCHIVE" | cut -f1))"

# ---- Prune old backups ------------------------------------------------------
if [ "$KEEP_LAST" -gt 0 ]; then
    mapfile -t OLD < <(ls -1t "$BACKUP_ROOT"/profalycms-backup-*.tar.gz 2>/dev/null | tail -n +"$((KEEP_LAST + 1))")
    if [ "${#OLD[@]}" -gt 0 ]; then
        echo "==> Pruning ${#OLD[@]} old backup(s) (keeping $KEEP_LAST)"
        for f in "${OLD[@]}"; do rm -f "$f"; echo "    removed $(basename "$f")"; done
    fi
fi
