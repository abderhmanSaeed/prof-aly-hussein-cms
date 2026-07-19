#!/usr/bin/env bash
# =============================================================================
# healthcheck.sh — verify the running app reports Healthy at /health.
#
#   Usage: ./healthcheck.sh [URL] [RETRIES] [SLEEP_SECONDS]
#   Defaults: URL=http://127.0.0.1:5000/health  RETRIES=10  SLEEP=3
#
# Exit 0 when the endpoint returns HTTP 200 and JSON status "Healthy".
# Used on its own for monitoring and by update.sh to gate a release.
# =============================================================================
set -euo pipefail

URL="${1:-${HEALTH_URL:-http://127.0.0.1:5000/health}}"
RETRIES="${2:-10}"
SLEEP="${3:-3}"

echo "Health check: $URL (up to $RETRIES attempts)"

for attempt in $(seq 1 "$RETRIES"); do
    # -s silent, -S show error, -m timeout; capture body + HTTP code.
    if body="$(curl -sS -m 10 -w '\n%{http_code}' "$URL" 2>/dev/null)"; then
        code="$(printf '%s' "$body" | tail -n1)"
        payload="$(printf '%s' "$body" | sed '$d')"
        if [ "$code" = "200" ] && printf '%s' "$payload" | grep -q '"status":"Healthy"'; then
            echo "  attempt $attempt: OK (HTTP $code)"
            echo "$payload"
            exit 0
        fi
        echo "  attempt $attempt: not ready (HTTP ${code:-none})"
    else
        echo "  attempt $attempt: no response"
    fi
    sleep "$SLEEP"
done

echo "Health check FAILED after $RETRIES attempts." >&2
exit 1
