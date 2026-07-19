#!/usr/bin/env bash
# =============================================================================
# publish.sh — build a production artifact of the CMS.
#
# Run this from a machine with the .NET 8 SDK (a CI runner, your workstation,
# or the server if it has the SDK). It produces a self-contained tarball you
# copy to the server and hand to deploy/update.sh.
#
#   Usage: ./publish.sh [OUTPUT_DIR]
#     OUTPUT_DIR  where the tarball is written (default: ./deploy/artifacts)
#
# Environment overrides:
#   CONFIGURATION   Release (default)
#   RUNTIME         linux-x64 (default) — framework-dependent (needs .NET runtime on server)
#   PROJECT         path to the web csproj
# =============================================================================
set -euo pipefail

# Resolve repo root from this script's location so it runs from anywhere.
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

CONFIGURATION="${CONFIGURATION:-Release}"
RUNTIME="${RUNTIME:-linux-x64}"
PROJECT="${PROJECT:-$REPO_ROOT/src/ProfAly.CMS.Web/ProfAly.CMS.Web.csproj}"
OUTPUT_DIR="${1:-$REPO_ROOT/deploy/artifacts}"
# ReadyToRun improves cold-start but needs a matching-RID crossgen on the build
# host — off by default for portability. Set R2R=true on a linux-x64 build host.
R2R="${R2R:-false}"

# A sortable UTC build stamp (no Date.now dependency — plain date on the build host).
STAMP="$(date -u +%Y%m%d-%H%M%S)"
BUILD_DIR="$(mktemp -d)"
ARTIFACT="$OUTPUT_DIR/profalycms-$STAMP.tar.gz"

cleanup() { rm -rf "$BUILD_DIR"; }
trap cleanup EXIT

echo "==> Restoring & publishing ($CONFIGURATION / $RUNTIME framework-dependent)"
dotnet publish "$PROJECT" \
    -c "$CONFIGURATION" \
    -r "$RUNTIME" \
    --self-contained false \
    -p:PublishReadyToRun="$R2R" \
    -o "$BUILD_DIR"

# web.config is only used by IIS; drop it to keep the Linux artifact clean.
rm -f "$BUILD_DIR/web.config"

# Sanity check the entry point shipped.
if [ ! -f "$BUILD_DIR/ProfAly.CMS.Web.dll" ]; then
    echo "ERROR: ProfAly.CMS.Web.dll missing from publish output." >&2
    exit 1
fi

mkdir -p "$OUTPUT_DIR"
echo "==> Packing artifact"
tar --force-local -czf "$ARTIFACT" -C "$BUILD_DIR" .

echo
echo "Artifact ready:"
echo "  $ARTIFACT"
echo
echo "Next: copy it to the server and deploy, e.g."
echo "  scp \"$ARTIFACT\" user@server:/tmp/"
echo "  ssh user@server 'sudo /var/www/profalycms/deploy/update.sh /tmp/$(basename "$ARTIFACT")'"
