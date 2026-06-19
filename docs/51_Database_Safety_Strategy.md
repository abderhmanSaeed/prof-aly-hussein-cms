# 51 — Database Safety Strategy

**Date:** 2026-06-19
**Status:** Implemented (Database Safety Layer). No feature stage started.
**Goal:** guarantee that manual/imported content can never be silently lost again.

---

## 1. Background (what went wrong)

The SQLite database is a git-ignored runtime artifact. During earlier verification, throwaway databases were repeatedly created and **deleted** (`rm -rf App_Data`), and the importer could re-run. There was **no backup** and **no persistent "already imported" marker**, so a deleted or re-initialized database lost whatever state it held (see report 50). This layer removes those failure modes.

## 2. The four safety guarantees

| # | Requirement | How it is enforced |
|---|---|---|
| **1. Never delete the database** | App_Data / app.db / -wal / -shm must never be deleted | (a) **Policy:** no tool/agent workflow deletes `App_Data`; verification now runs against **temp databases** (the new tests use `%TEMP%`, never `App_Data`). (b) **`.gitignore`** keeps the DB out of source control but it is never removed. (c) Even if a file is lost, **automatic backups** (guarantee 2) make it recoverable. |
| **2. Automatic backups** | timestamped SQLite copies under `App_Data/Backups` | `IDatabaseBackupService` → `SqliteDatabaseBackupService` writes `App_Data/Backups/app-{yyyyMMdd-HHmmss}-{reason}.db` using the SQLite **online-backup API** (consistent even with an open WAL). Detailed in report 52. |
| **3. Backup before init / import** | back up if a database exists, before any initialization or import | `DatabaseInitializer.RunAsync` calls `CreateBackupAsync("startup")` **before** migrations and seeders. `StaticContentImporter` calls `CreateBackupAsync("pre-import")` **before** writing content. Both no-op cleanly when no DB file exists yet. |
| **4. Import protection** | import only when empty; only once unless forced | The importer now checks three gates (below) and records a persistent marker. |

## 3. Import protection (the importer's new gates)

`StaticContentImporter.SeedAsync` runs only if **all** hold:

1. **Enabled** — `Seed:ImportStaticContent = true` (or `Seed:ForceImport = true`).
2. **Not already imported** — the `StaticContentImported` system setting is not `"true"` (unless `Seed:ForceImport=true`).
3. **Database empty of content** — no `Profile`, `ContentItem`, or `ActivityGroup` rows (unless forced). If content already exists, it **records the marker and skips** rather than risk touching real data.

On success it writes `StaticContentImported = true` (+ timestamp). Per-table emptiness guards inside the importer remain, so even a forced re-run never duplicates rows.

| Config key | Default | Effect |
|---|---|---|
| `Seed:ImportStaticContent` | `false` | Master switch to allow the one-time import |
| `Seed:ForceImport` | `false` | Bypass the once-only marker (re-attempt import) |
| `Backup:Enabled` | `true` | Toggle automatic backups |
| `Backup:KeepLast` | `0` | Retention; **0 = keep every backup (never delete)** |

## 4. The `StaticContentImported` system setting

A new key/value table **`SystemSetting`** (`Key` PK, `Value`, `UpdatedUtc`) stores operational flags that must survive restarts — distinct from `SiteSettings` (editorial config). The first well-known key is **`StaticContentImported`**. Added via migration **`AddSystemSettings`** (purely additive: creates one table; see report 52 §6).

## 5. How a future restart behaves

1. App starts → `DatabaseInitializer`:
   - ensures `App_Data` exists,
   - **backs up the existing database** (`startup`) — *this protects the DB even before the additive `AddSystemSettings` migration is applied*,
   - applies pending migrations (adds `SystemSetting` on first run after this change),
   - runs seeders.
2. Importer: if enabled and not yet imported and DB empty → **pre-import backup** → import → set marker. Otherwise skips.

Net effect: **every startup of an existing database produces a backup first**, and content is imported at most once.

## 6. Recovery procedure (summary; full steps in report 52 §7)

1. Stop the app.
2. In `App_Data/Backups`, pick the desired `app-*.db`.
3. Copy it over `App_Data/app.db` and delete any stale `app.db-wal` / `app.db-shm`.
4. Start the app (the startup backup will snapshot the restored state again).

## 7. Verification

Implemented and tested against **temp databases** (never `App_Data`):

- `dotnet build` (Infrastructure) → 0/0; `dotnet test` → **17/17** (14 existing + 3 new safety tests).
- **Backup test:** `CreateBackupAsync` writes a non-empty `App_Data/Backups/app-…-test.db`; returns `null` when no DB file exists (first-run safety).
- **Import-once test:** first run imports content (7 activity groups, profile, content items) and sets `StaticContentImported=true`; a **second run is a no-op** (counts unchanged).
- The `AddSystemSettings` migration was inspected: it only **creates** `SystemSetting` (Down drops only it) — no other table is altered.

> Note: the Web host was **not** rebuilt/run this task because your application (PID 38588, started 21:38) is currently running and locking the Web build output; I did not terminate it. The safety layer takes effect the next time you rebuild and restart the app — at which point the startup backup runs first, then the additive migration is applied.

## 8. Operational recommendations

- Keep `Backup:KeepLast = 0` (keep all) unless disk pressure requires pruning.
- Periodically copy `App_Data/Backups` off-machine (covered by the deployment stage later).
- Treat admin edits as authoritative; consider exporting them back to `static-content.json` so they are reproducible.
