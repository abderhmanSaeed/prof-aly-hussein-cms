# 91 — Production Deployment Guide

**Application:** Prof. Aly Hussein CMS (ASP.NET Core 8 Razor Pages + SQLite)
**Target stack:** Ubuntu Linux · .NET 8 runtime · Kestrel · Nginx (reverse proxy + TLS) · Let's Encrypt
**Companion artifacts:** everything referenced here lives in [`/deploy`](../deploy).

> This guide only *prepares* deployment. Nothing is deployed by following the audit steps
> until you deliberately run the commands on your server.

Deploy artifacts:

| File | Purpose |
|------|---------|
| `deploy/profalycms.service` | systemd unit (Kestrel on `127.0.0.1:5000`) |
| `deploy/profalycms.conf` | Nginx site (TLS, reverse proxy, compression, caching, security headers, large uploads) |
| `deploy/publish.sh` | build a production artifact (tarball) |
| `deploy/update.sh` | deploy an artifact with atomic release swap + auto-rollback |
| `deploy/backup.sh` | full backup: SQLite + uploads + configuration |
| `deploy/restore.sh` | restore from a backup archive (safety backup first) |
| `deploy/healthcheck.sh` | verify `/health` returns `Healthy` |

---

## 1. Server requirements

| Resource | Minimum | Recommended |
|----------|---------|-------------|
| OS | Ubuntu 22.04 LTS | Ubuntu 24.04 LTS |
| CPU | 1 vCPU | 2 vCPU |
| RAM | 1 GB | 2 GB |
| Disk | 10 GB | 20 GB+ (uploads + backups grow) |
| Network | Public IPv4, DNS A/AAAA → server | + IPv6 |

Single-node deployment. SQLite means **no separate database server** — the whole
application state is the `app.db` file plus the `uploads/` folder under `App_Data`.

---

## 2. Ubuntu packages

```bash
sudo apt update && sudo apt upgrade -y
sudo apt install -y nginx sqlite3 curl ca-certificates ufw \
                    certbot python3-certbot-nginx gnupg
```

- `nginx` — reverse proxy + TLS termination
- `sqlite3` — consistent online backups (`.backup`) and maintenance (`VACUUM`, `integrity_check`)
- `certbot` + `python3-certbot-nginx` — Let's Encrypt certificates
- `ufw` — firewall
- `curl` — health checks

---

## 3. .NET 8 runtime installation

Install the **ASP.NET Core 8 runtime** (not the SDK — the app is published elsewhere).

```bash
# Microsoft package feed
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O /tmp/pm.deb
sudo dpkg -i /tmp/pm.deb
sudo apt update
sudo apt install -y aspnetcore-runtime-8.0

# Verify
dotnet --list-runtimes    # expect Microsoft.AspNetCore.App 8.0.x and Microsoft.NETCore.App 8.0.x
which dotnet               # expect /usr/bin/dotnet (matches the systemd unit)
```

> If you intend to build **on the server**, install `dotnet-sdk-8.0` instead and run
> `deploy/publish.sh` there. Otherwise build on a workstation/CI and copy the artifact.

---

## 4. Service account & folder structure

Create a dedicated, non-login service user and the directory layout the scripts expect.

```bash
sudo useradd --system --home /var/www/profalycms --shell /usr/sbin/nologin profalycms

sudo mkdir -p /var/www/profalycms/{releases,shared/App_Data/uploads,shared/App_Data/Backups,shared/Logs}
sudo mkdir -p /var/backups/profalycms
sudo mkdir -p /etc/profalycms
sudo mkdir -p /var/www/certbot           # Let's Encrypt webroot challenge
```

### Layout

```
/var/www/profalycms/
├── current -> releases/<stamp>          # symlink the systemd unit runs
├── releases/
│   └── <stamp>/                         # published binaries (App_Data & Logs are symlinks → shared)
├── shared/                              # PERSISTENT — never replaced by a deploy
│   ├── App_Data/
│   │   ├── app.db                       # SQLite database (+ -wal/-shm at runtime)
│   │   ├── uploads/                     # media (images/PDF/docs)
│   │   └── Backups/                     # in-app DB snapshots (Stage 90 backup service)
│   └── Logs/                            # rolling app logs (yyyy-MM.log)
├── deploy/                              # copy of the /deploy scripts (optional but handy)
/var/backups/profalycms/                 # FULL deployment backups (db+uploads+config tarballs)
/etc/profalycms/profalycms.env           # secrets (chmod 600) — loaded by systemd
```

Each release links persistent data into place, so `app.db`, uploads, backups and logs
survive every deploy and rollback:

```
releases/<stamp>/App_Data -> /var/www/profalycms/shared/App_Data
releases/<stamp>/Logs      -> /var/www/profalycms/shared/Logs
```

(`update.sh` creates these symlinks automatically.)

---

## 5. Permissions

```bash
sudo chown -R profalycms:profalycms /var/www/profalycms
sudo chown -R profalycms:profalycms /var/backups/profalycms
sudo find /var/www/profalycms/shared -type d -exec chmod 750 {} \;
sudo find /var/www/profalycms/shared -type f -exec chmod 640 {} \;

# Secrets file — root-owned, readable by the service group, never world-readable.
sudo tee /etc/profalycms/profalycms.env >/dev/null <<'EOF'
# Secrets for profalycms.service (loaded by systemd EnvironmentFile).
#
# OPTIONAL: override the first-run Super Admin password. If this line is left
# commented out, the app seeds the admin using the default in
# appsettings.Production.json (see docs/92_Admin_Account_Verification.md) so a
# clean deploy always has a working login. If you set it here, THIS value becomes
# the seeded password instead (it overrides appsettings.Production).
# It only ever affects an EMPTY database on first run — never an existing account.
#AdminAccount__Password=CHANGE_ME_TO_A_STRONG_PASSWORD
EOF
sudo chown root:profalycms /etc/profalycms/profalycms.env
sudo chmod 640 /etc/profalycms/profalycms.env
```

> The admin **email** is set in `appsettings.json` (`AdminAccount:Email`). The first-run
> **password** comes from `appsettings.Production.json` by default, and can be overridden by
> the `AdminAccount__Password` environment variable above. In either case it is only used to
> **seed** the account on the first startup of an empty database — an existing admin is never
> modified. Rotate the password from the admin UI after the first login.

---

## 6. Firewall & ports

| Port | Service | Notes |
|------|---------|-------|
| 22 | SSH | administration (restrict to your IP if possible) |
| 80 | HTTP | ACME challenge + redirect to HTTPS |
| 443 | HTTPS | public site |
| 5000 | Kestrel | **loopback only** — never exposed publicly |

```bash
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
sudo ufw status verbose
```

Kestrel binds to `127.0.0.1:5000` (set in the systemd unit), so it is unreachable from the
network — only Nginx talks to it.

---

## 7. Nginx configuration

```bash
sudo cp /var/www/profalycms/deploy/profalycms.conf /etc/nginx/sites-available/profalycms.conf
# Edit and replace every "example.com" with your domain:
sudo sed -i 's/example.com/yourdomain.com/g' /etc/nginx/sites-available/profalycms.conf
sudo ln -s /etc/nginx/sites-available/profalycms.conf /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t && sudo systemctl reload nginx
```

The site config provides: HTTPS (TLS 1.2/1.3), reverse proxy to Kestrel, gzip compression,
static caching, `client_max_body_size 60m` for large uploads, HSTS at the edge, and a quiet
`/health` location.

> **Security headers:** the application already emits CSP, `X-Frame-Options`,
> `X-Content-Type-Options`, `Referrer-Policy` and `Permissions-Policy` (Stage 90). Nginx adds
> **HSTS** only, to avoid duplicate headers. A commented block in `profalycms.conf` lets you
> move the others to the edge if you prefer.

---

## 8. Let's Encrypt (TLS)

DNS must already point at the server. Issue a certificate with the webroot method (matches the
`/.well-known/acme-challenge/` location in the Nginx config):

```bash
sudo certbot certonly --webroot -w /var/www/certbot \
     -d yourdomain.com -d www.yourdomain.com \
     --email you@example.com --agree-tos --no-eff-email
sudo systemctl reload nginx
```

Auto-renewal is installed by the certbot package (`systemctl status certbot.timer`). Test it:

```bash
sudo certbot renew --dry-run
```

Add a reload hook so Nginx picks up renewed certs:

```bash
echo -e '#!/bin/sh\nsystemctl reload nginx' | sudo tee /etc/letsencrypt/renewal-hooks/deploy/reload-nginx.sh
sudo chmod +x /etc/letsencrypt/renewal-hooks/deploy/reload-nginx.sh
```

---

## 9. Install the systemd service

```bash
sudo cp /var/www/profalycms/deploy/profalycms.service /etc/systemd/system/profalycms.service
sudo systemctl daemon-reload
sudo systemctl enable profalycms
# (Do not start yet — start happens on the first deploy, once "current" exists.)
```

---

## 10. Publish command

On a machine with the **.NET 8 SDK** (workstation, CI, or the server if it has the SDK):

```bash
# From the repository root:
./deploy/publish.sh
# → produces deploy/artifacts/profalycms-<stamp>.tar.gz  (framework-dependent, linux-x64)

# Optional: faster cold-start (only on a linux-x64 build host):
R2R=true ./deploy/publish.sh
```

Copy the artifact to the server:

```bash
scp deploy/artifacts/profalycms-<stamp>.tar.gz  user@server:/tmp/
```

---

## 11. Updating the application (deploy)

Run on the server as root. `update.sh` unpacks the artifact into a new release, links the
shared data, takes a safety backup, atomically repoints `current`, restarts the service, and
**verifies `/health`** — rolling back automatically if the new release is unhealthy.

```bash
sudo /var/www/profalycms/deploy/update.sh /tmp/profalycms-<stamp>.tar.gz
```

First deploy also becomes the first `current`, so the service starts cleanly. Confirm:

```bash
systemctl status profalycms --no-pager
curl -s https://yourdomain.com/health | jq .    # status: Healthy
```

---

## 12. Rollback procedure

`update.sh` rolls back automatically on a failed health check. To roll back **manually** to a
previous release:

```bash
ls -1dt /var/www/profalycms/releases/*/          # list releases, newest first
sudo ln -sfn /var/www/profalycms/releases/<older-stamp> /var/www/profalycms/current.tmp
sudo mv -Tf /var/www/profalycms/current.tmp /var/www/profalycms/current
sudo systemctl restart profalycms
sudo /var/www/profalycms/deploy/healthcheck.sh
```

Because `App_Data` is shared, rolling back the **binaries** does not roll back **data**. If a
release also changed the database schema, restore a matching data backup as well (§14). The
last `KEEP_RELEASES` (default 5) releases are retained for exactly this reason.

---

## 13. Backup strategy

Two independent layers:

1. **In-app snapshots (Stage 90):** the app auto-backs-up `app.db` to `shared/App_Data/Backups`
   at startup, before content imports, and on demand from **Admin → System → Backup & Restore**.
   These are DB-only and verified with `PRAGMA integrity_check`.
2. **Full deployment backups (`backup.sh`):** SQLite **+ uploads + configuration** into a single
   tarball under `/var/backups/profalycms`. Safe to run live (online `.backup` snapshot).

```bash
sudo /var/www/profalycms/deploy/backup.sh
# → /var/backups/profalycms/profalycms-backup-<stamp>.tar.gz  (keeps last 30)
```

Schedule a nightly full backup with cron:

```bash
sudo crontab -e
# Every day at 02:30 UTC:
30 2 * * *  /var/www/profalycms/deploy/backup.sh >> /var/log/profalycms-backup.log 2>&1
```

**Off-site:** copy `/var/backups/profalycms/*.tar.gz` to object storage / another host regularly
— the on-box backups do not survive a disk loss.

---

## 14. Restore strategy

```bash
# From a specific archive:
sudo /var/www/profalycms/deploy/restore.sh /var/backups/profalycms/profalycms-backup-<stamp>.tar.gz
# Or the most recent one:
sudo /var/www/profalycms/deploy/restore.sh latest
# Also restore appsettings.Production.json (secrets env is never auto-overwritten):
sudo /var/www/profalycms/deploy/restore.sh latest --with-config
```

`restore.sh` verifies the archive, takes a **pre-restore safety backup**, stops the service,
replaces `app.db` (clearing stale WAL/SHM) and uploads, fixes ownership, restarts, and
health-checks. A failed restore leaves the safety backup for recovery.

---

## 15. SQLite maintenance

SQLite runs in WAL mode with `busy_timeout`, `foreign_keys=ON`, `synchronous=NORMAL` (applied
by the app on every connection). Occasional maintenance:

```bash
DB=/var/www/profalycms/shared/App_Data/app.db

# Integrity check (should print: ok)
sudo -u profalycms sqlite3 "$DB" 'PRAGMA integrity_check;'

# Checkpoint the WAL into the main file (safe anytime)
sudo -u profalycms sqlite3 "$DB" 'PRAGMA wal_checkpoint(TRUNCATE);'

# Reclaim space / defragment — do this while the service is stopped for a clean VACUUM:
sudo systemctl stop profalycms
sudo -u profalycms sqlite3 "$DB" 'VACUUM;'
sudo systemctl start profalycms
```

Always `backup.sh` before manual DB surgery.

---

## 16. Monitoring

```bash
# Service state
systemctl status profalycms --no-pager
systemctl is-active profalycms

# Live logs (journald) and the app's rolling file logs
journalctl -u profalycms -f
tail -f /var/www/profalycms/shared/Logs/$(date -u +%Y-%m).log

# Health (local and public)
/var/www/profalycms/deploy/healthcheck.sh
curl -s https://yourdomain.com/health | jq .
```

`/health` reports the application process, SQLite connectivity, and the upload + backup folder
accessibility. Wire it into an uptime monitor (expect HTTP 200 + `"status":"Healthy"`). A cron
liveness check example:

```bash
*/5 * * * * /var/www/profalycms/deploy/healthcheck.sh https://yourdomain.com/health 1 0 \
            || systemctl restart profalycms
```

---

## 17. Restart / stop / start the service

```bash
sudo systemctl restart profalycms
sudo systemctl stop profalycms
sudo systemctl start profalycms
sudo systemctl reload nginx        # after Nginx config changes
```

---

## 18. Troubleshooting

| Symptom | Check |
|---------|-------|
| 502 Bad Gateway | Kestrel down or wrong port: `systemctl status profalycms`, `journalctl -u profalycms -n 100`. Confirm it listens on `127.0.0.1:5000` (`ss -ltnp | grep 5000`). |
| Service won't start | `journalctl -u profalycms -n 100`. Common: `dotnet` path wrong, missing runtime (`dotnet --list-runtimes`), or `current` symlink missing (deploy first). |
| 500 on every page | App exception — see app logs in `shared/Logs/…log` and journald. Verify `App_Data` symlink resolves and `app.db` is writable by `profalycms`. |
| Admin can't log in | The Super Admin is seeded on the first startup of an empty DB from `appsettings.Production.json` (or the `AdminAccount__Password` override). Check the startup log for "Created Super Admin"; see docs/92_Admin_Account_Verification.md. |
| DB "readonly"/locked | Ownership/permissions: `chown -R profalycms:profalycms shared/App_Data`; ensure the dir (not just the file) is writable (WAL needs to create `-wal`/`-shm`). |
| Uploads rejected / 413 | `client_max_body_size` in Nginx (60m) vs app caps (`FileStorage:Max*Bytes`). Raise both if larger files are needed. |
| TLS errors / cert not found | `certbot certificates`; confirm `ssl_certificate` paths in `profalycms.conf` match `/etc/letsencrypt/live/<domain>/`. `nginx -t`. |
| HTTPS redirect loop | Confirm `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` (in the unit) so the app sees `X-Forwarded-Proto=https`; let Nginx (not the app) do the 80→443 redirect. |
| Health check fails after deploy | `update.sh` auto-rolls-back; inspect `journalctl -u profalycms` and the artifact. Redeploy a known-good artifact or roll back per §12. |
| Wrong client IPs in rate-limit/logs | Ensure forwarded headers are enabled and Nginx sets `X-Forwarded-For`/`X-Real-IP` (it does in the provided config). |

---

## 19. First-time setup checklist

1. Provision Ubuntu, point DNS at it.
2. §2 packages · §3 .NET runtime.
3. §4 service user + folders · §5 permissions + secrets env.
4. §6 firewall.
5. §7 Nginx config (domain substituted) · §8 Let's Encrypt cert.
6. §9 install systemd unit (enable, don't start).
7. §10 publish → copy artifact → §11 `update.sh` (first deploy starts the service).
8. Verify: `https://yourdomain.com/health` = Healthy, site loads, admin login works.
9. §13 schedule backups + off-site copy.

---

## Notes

- The first-run admin password defaults from `appsettings.Production.json` and can be overridden
  by the `AdminAccount__Password` env var; rotate it after first login. It only seeds an empty DB.
- The app never resets or recreates the database; migrations are additive and a startup backup
  precedes them.
- Deployments are atomic (symlink swap) and health-gated with automatic rollback.
