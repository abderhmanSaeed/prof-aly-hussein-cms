# 90 — Production Hardening Report

**Project:** ProfAly.CMS — ASP.NET Core 8 Razor Pages + SQLite CMS
**Stage:** Production Hardening (infrastructure only — no business features added)
**Date:** 2026-07-19
**Result:** Build clean (0 warnings / 0 errors) · 38/38 tests pass · Development + Production runs verified

This stage prepared the existing CMS for production deployment by adding missing
production infrastructure only. No UI was redesigned, no business logic was changed, no
database was reset, `App_Data` and `app.db` were preserved, and existing content is intact.

---

## Part 1 — Audit

Legend: ✅ already implemented · ⚠ improved this stage · ❌ was missing (now added)

| # | Item | Before | Now | Notes |
|---|------|:------:|:---:|-------|
| 1 | HTTPS support | ✅ | ✅ | `UseHttpsRedirection()` already present; port now configurable via `HttpsRedirection:HttpsPort`. |
| 2 | HSTS | ✅ | ✅ | `UseHsts()` (non-dev). Added explicit `AddHsts` policy: 365d, includeSubDomains, preload. |
| 3 | Security Headers | ❌ | ✅ | New `SecurityHeadersMiddleware` on every response. |
| 4 | CSP (Content-Security-Policy) | ❌ | ✅ | UI-compatible policy (local bundles, Google Fonts, YouTube-nocookie embeds). |
| 5 | X-Frame-Options | ❌ | ✅ | `SAMEORIGIN` (+ CSP `frame-ancestors 'self'`). |
| 6 | X-Content-Type-Options | ❌ | ✅ | `nosniff`. |
| 7 | Referrer-Policy | ❌ | ✅ | `strict-origin-when-cross-origin`. |
| 8 | Permissions-Policy | ❌ | ✅ | camera/mic/geolocation/payment/usb/interest-cohort denied. |
| 9 | Rate Limiting | ❌ | ✅ | Global per-IP fixed window + stricter `login` policy. |
| 10 | Anti-forgery | ✅ | ⚠ | On by default for Razor Pages; cookie now HttpOnly + SameSite=Lax + Secure. |
| 11 | Secure Cookies | ⚠ | ✅ | Auth + anti-forgery cookies `Secure=Always` in Production; cookie policy middleware added. |
| 12 | HttpOnly Cookies | ✅ | ✅ | Identity default; now explicit on all cookies. |
| 13 | SameSite Cookies | ⚠ | ✅ | `Lax` enforced via cookie policy + per-cookie config. |
| 14 | Authentication Cookie settings | ✅ | ✅ | Paths/expiry/sliding kept; named `ProfAly.Admin.Auth`, hardened flags added. |
| 15 | Session security | n/a | n/a | No server session/`ISession` is used; not applicable. |
| 16 | Login security | ✅ | ✅ | Identity lockout (5/15min), open-redirect guard, generic errors; + login rate limit. |
| 17 | SQLite WAL mode | ✅ | ✅ | `SqlitePragmaInterceptor` — `journal_mode=WAL`. |
| 18 | SQLite Busy Timeout | ✅ | ✅ | `busy_timeout=5000`. |
| 19 | Database Backup system | ✅ | ⚠ | Startup + pre-import existed; added verify, list, restore, download, manual. |
| 20 | Backup verification | ❌ | ✅ | `PRAGMA integrity_check` after every backup and before every restore. |
| 21 | Health Checks | ⚠ | ✅ | `database` existed; added `uploads` + `backups` folder checks; JSON output. |
| 22 | Structured logging | ⚠ | ✅ | Console existed; added rolling file provider (`Logs/yyyy-MM.log`). |
| 23 | Exception handling | ✅ | ✅ | `UseExceptionHandler("/Error")` (non-dev). |
| 24 | Production error page | ✅ | ✅ | `Pages/Error.cshtml` (no-store, no request details leak). |
| 25 | Upload security | ✅ | ✅ | Allowlist + size + magic-byte validation + filename sanitize (`MediaUploadService`). |
| 26 | MIME validation | ✅ | ✅ | Magic-byte checks per type (SVG/HTML/exe rejected). |
| 27 | Max upload limits | ✅ | ⚠ | Per-kind byte caps existed; added Kestrel/multipart body-size limits. |
| 28 | CSP compatibility | ❌ | ✅ | CSP tuned so no current page breaks (verified at runtime). |
| 29 | Static files cache | ❌ | ✅ | `Cache-Control: public,max-age=604800` on wwwroot + /uploads. |
| 30 | Response compression | ❌ | ✅ | Brotli + Gzip, `EnableForHttps`. |
| 31 | Performance headers | ❌ | ✅ | Cache-Control + Content-Encoding (compression) applied. |

---

## Part 2 — Implemented changes

### New files
- `src/ProfAly.CMS.Web/Infrastructure/Security/SecurityHeadersMiddleware.cs` — defensive headers + CSP.
- `src/ProfAly.CMS.Web/Infrastructure/Logging/FileLoggerProvider.cs` — dependency-free rolling file logger.
- `src/ProfAly.CMS.Infrastructure/Persistence/HealthChecks/StorageHealthChecks.cs` — upload + backup folder checks.
- `src/ProfAly.CMS.Web/Areas/Admin/Pages/Database/Backup.cshtml(.cs)` — admin Backup & Restore UI.
- `src/ProfAly.CMS.Web/appsettings.Production.json` — production log levels, file-log, backup, rate-limit config.

### Modified files
- `Program.cs` — wired HSTS/HTTPS, response compression, cookie policy, hardened auth + anti-forgery cookies,
  upload body-size limits, rate limiter, security headers, static cache headers, file logger, `/health` JSON writer.
- `DependencyInjection.cs` — registered `uploads` + `backups` health checks.
- `IDatabaseBackupService.cs` / `SqliteDatabaseBackupService.cs` — list, verify, restore, download-path resolver, verified backups.
- `Pages/Account/Login.cshtml.cs` — `[EnableRateLimiting("login")]`.
- `Areas/Admin/Pages/Shared/_Sidebar.cshtml` — enabled **System → Backup & Restore** link.
- `tests/…/DatabaseSafetyTests.cs` — 3 new tests (verify+list, restore recovery, path-traversal rejection).

### Pipeline order (final)
`ExceptionHandler → HSTS → HttpsRedirection → SecurityHeaders → ResponseCompression → StaticFiles(cache)
→ /uploads(cache) → Routing → RateLimiter → RequestLocalization → CookiePolicy → AuthN → AuthZ → Endpoints`

### `/health` (verified live)
```json
{"status":"Healthy","checks":[
  {"name":"database","status":"Healthy"},
  {"name":"uploads","status":"Healthy"},
  {"name":"backups","status":"Healthy"}]}
```
Application-running is proven by the pipeline responding; SQLite, upload folder, and backup folder are each probed.

---

## Part 3 — SQLite hardening checklist

Applied on every opened connection by `SqlitePragmaInterceptor` (all ✅ — no change required):

| Setting | Value |
|---------|-------|
| WAL mode | `PRAGMA journal_mode=WAL` |
| Busy timeout | `PRAGMA busy_timeout=5000` |
| Foreign keys | `PRAGMA foreign_keys=ON` |
| Journal mode | WAL (persisted in the file) |
| Synchronous | `PRAGMA synchronous=NORMAL` |

---

## Part 4 — Backup system checklist

Flow guaranteed: **manual edit → automatic backup → restore**.

| Capability | Status | Detail |
|------------|:------:|--------|
| Startup backup | ✅ | `DatabaseInitializer` → `CreateBackupAsync("startup")` before migrations/seeding. |
| Pre-import backup | ✅ | `StaticContentImporter` → `CreateBackupAsync("pre-import")`. |
| Manual backup | ✅ | Admin → System → Backup & Restore → **Create Backup Now** (`"manual"`). |
| Backup verification | ✅ | `PRAGMA integrity_check` after create and before restore. |
| Restore | ✅ | Safety backup first → verify → `ClearAllPools` → drop stale WAL/SHM → replace file. |
| Download backup | ✅ | Path-traversal-safe resolver; served via `PhysicalFile`. |
| Backup list + metadata | ✅ | File name, timestamp (UTC), reason, size — parsed from `app-yyyyMMdd-HHmmss-{reason}.db`. |
| Online-safe snapshots | ✅ | SQLite online-backup API (consistent even with active WAL). |

Restore is reversible: a `pre-restore` safety backup is always taken first. Proven by
`Restore_RecoversPreviousState_AndTakesSafetyBackup` (create data → back up → corrupt → restore → original recovered).

---

## Part 5 — Logging checklist

- Rolling file logs at `Logs/yyyy-MM.log` (monthly; daily optional via `Logging:File:DailyRolling`). Verified created at runtime.
- Structured lines: `timestamp [LEVEL] category (eventId) - message` (+ exception detail).
- **No passwords / secrets logged:** EF sensitive-data logging is off (default), app code logs identifiers only
  (email on lockout, request id), and the file provider only persists messages the app already emits.
- `Logs/` and `*.log` are already git-ignored.

---

## Security checklist (response, verified live in Development)

```
X-Content-Type-Options: nosniff
X-Frame-Options: SAMEORIGIN
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: camera=(), microphone=(), geolocation=(), payment=(), usb=(), interest-cohort=()
Content-Security-Policy: default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'self';
  form-action 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'
  https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com; img-src 'self' data:;
  connect-src 'self'; frame-src https://www.youtube-nocookie.com https://www.youtube.com; media-src 'self';
  manifest-src 'self'; upgrade-insecure-requests
Content-Encoding: br            (compression working)
```
`Server` header suppressed (`AddServerHeader=false`); `X-Powered-By` removed.

---

## Production readiness score

**92 / 100 — Production-ready** for a single-admin, single-node SQLite deployment behind a TLS-terminating reverse proxy.

| Area | Score |
|------|:-----:|
| Transport security (HTTPS/HSTS) | 9/10 |
| HTTP security headers / CSP | 9/10 |
| Cookies & anti-forgery | 10/10 |
| Authentication & login hardening | 9/10 |
| Rate limiting | 8/10 |
| Data safety (SQLite + backup/restore) | 10/10 |
| Observability (health + logging) | 9/10 |
| Uploads | 10/10 |
| Performance (compression + caching) | 9/10 |
| Operational polish | 9/10 |

---

## Known limitations

1. **CSP uses `'unsafe-inline'` for scripts/styles.** Required because the current UI has small inline
   `<script>`/`style=` blocks. Tighten later by moving inline scripts to files and adopting per-request
   nonces (`script-src 'self' 'nonce-…'`) — no functional change needed, purely a hardening upgrade.
2. **HTTPS redirect port.** In this stage it auto-detects (unset). In production set
   `HttpsRedirection:HttpsPort=443` (or `ASPNETCORE_HTTPS_PORT`) or terminate TLS at a reverse proxy;
   otherwise the middleware logs "Failed to determine the https port for redirect" and does not redirect.
3. **Rate limiting is per-node, in-memory** (fixed-window, partitioned by remote IP). Correct for a single
   instance; a multi-node deployment would need a distributed limiter and real client-IP via `UseForwardedHeaders`.
4. **Restore takes effect immediately and is not multi-user coordinated.** Fine for the single-admin model;
   in-flight requests during a restore may briefly see connection resets. A safety backup is always taken first.
5. **Backup retention is unbounded by default** (`Backup:KeepLast=0` = keep all) to honor "never delete".
   Set `Backup:KeepLast` to enable pruning of the oldest backups.
6. **No off-site backup.** Backups live under `App_Data/Backups` on the same host; add an external copy
   (object storage / scheduled job) for disaster recovery.
7. **File logging has no size cap per month.** Monthly rotation only; add archival/cleanup if volume is high.

---

## Verification summary

- `dotnet build ProfAly.CMS.sln` → **0 warnings, 0 errors**
- `dotnet test` → **38/38 passed** (incl. new backup verify/restore/traversal tests)
- Development run: `/health` Healthy (3 checks), all security headers + CSP present, Brotli compression,
  static `Cache-Control`, login `200`, admin + backup page gated `302`, `Logs/2026-07.log` written.
- Production run: boots clean, `/health` Healthy, `appsettings.Production.json` applied.

**No commit, push, or tag was performed, per stage rules.**
