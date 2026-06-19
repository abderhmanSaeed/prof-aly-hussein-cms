# 52 — Database Backup Design

**Date:** 2026-06-19
**Component:** `IDatabaseBackupService` / `SqliteDatabaseBackupService` (Database Safety Layer).

---

## 1. Goals

- Produce **consistent** point-in-time copies of the SQLite database, safe to take while the DB is open and has an active WAL.
- Run **automatically** before any risky operation (initialization, import).
- **Never** delete the live database; backups are additive by default.
- Be cheap, dependency-free, and easy to restore by hand.

## 2. Where it lives

| Artifact | Path |
|---|---|
| Abstraction | `src/ProfAly.CMS.Application/Abstractions/IDatabaseBackupService.cs` |
| Implementation | `src/ProfAly.CMS.Infrastructure/Persistence/Backup/SqliteDatabaseBackupService.cs` |
| DI registration | `DependencyInjection.AddPersistence` → `AddScoped<IDatabaseBackupService, SqliteDatabaseBackupService>()` |
| Backups output | `App_Data/Backups/` (sibling of `app.db`) |

## 3. Backup mechanism

Uses the **SQLite online-backup API** (`Microsoft.Data.Sqlite` `SqliteConnection.BackupDatabase`), not a raw file copy:

```csharp
await using var source = new SqliteConnection($"Data Source={dbPath}");
await using var target = new SqliteConnection($"Data Source={destination}");
await source.OpenAsync(ct);
await target.OpenAsync(ct);
source.BackupDatabase(target);   // consistent snapshot; merges committed WAL pages
```

Why the backup API and not `File.Copy`:
- It produces a **transactionally consistent** single-file copy even when other connections are open and a `-wal`/`-shm` exist (a plain copy of `app.db` alone would miss data still in the WAL).
- The destination is a clean, self-contained `.db` (no separate WAL needed).

## 4. File naming & location

```
App_Data/Backups/app-{yyyyMMdd-HHmmss}-{reason}.db
```
- UTC timestamp → lexical sort = chronological order.
- `{reason}` is a sanitized tag (`[a-z0-9-]`): `startup`, `pre-import`, `test`, or `manual`.
- The DB path is resolved from the EF connection string (`SqliteConnectionStringBuilder.DataSource`); `:memory:`/unset → no-op.

## 5. Triggers

| Caller | Reason tag | When |
|---|---|---|
| `DatabaseInitializer.RunAsync` | `startup` | Before migrations & seeders, on every startup where a DB file exists |
| `StaticContentImporter.SeedAsync` | `pre-import` | Immediately before content is written (only when the import actually proceeds) |
| (manual) `IDatabaseBackupService.CreateBackupAsync(reason)` | caller-supplied | On demand from any future admin action |

First run (no DB file yet) → `CreateBackupAsync` returns `null` (nothing to back up); no error.

## 6. Configuration & retention

| Key | Default | Meaning |
|---|---|---|
| `Backup:Enabled` | `true` | Master switch |
| `Backup:KeepLast` | `0` | **0 = keep every backup (no deletion)**. If `> 0`, prune to the N most recent (best-effort, logged). |

Retention defaults to **keep-all** so the safety layer never deletes data. Pruning is opt-in.

## 7. Restore procedure (manual)

1. **Stop the application** (release the SQLite file lock).
2. Choose a backup from `App_Data/Backups/` (newest = last `app-*.db` by name).
3. Replace the live database:
   - copy the chosen `app-*.db` → `App_Data/app.db` (overwrite), and
   - delete any leftover `App_Data/app.db-wal` and `App_Data/app.db-shm` (they belong to the old DB; the restored file is self-contained).
4. **Start the application.** Initialization will take a fresh `startup` backup of the restored state and apply any pending migrations.

Alternative (no overwrite): point `ConnectionStrings:DefaultConnection` at the backup file to inspect it first.

## 8. Failure handling

- A backup failure is **logged at Error** and **does not throw** — startup proceeds. This is safe because the only subsequent operations are **additive migrations** (no data loss) and the **guarded importer** (won't wipe). The error is surfaced in logs for follow-up.
- Backup creation is wrapped so a partial/failed destination does not block the app.

## 9. `AddSystemSettings` migration (supporting change)

The once-only marker needs persistent storage. Migration `20260619185627_AddSystemSettings` is **purely additive**:

```
Up:   CREATE TABLE SystemSetting (Key TEXT PK [128], Value TEXT [1024] NULL, UpdatedUtc TEXT NOT NULL)
Down: DROP TABLE SystemSetting
```

No existing table is touched. It is applied automatically on the next startup **after** the `startup` backup runs — so applying it cannot lose data.

## 10. Test coverage (temp DB, never App_Data)

`tests/ProfAly.CMS.Tests/DatabaseSafetyTests.cs`:
- `Backup_CreatesTimestampedCopy_InBackupsFolder` — asserts a non-empty file appears in `<dbdir>/Backups` with the `-test` tag.
- `Backup_ReturnsNull_WhenDatabaseFileMissing` — first-run safety.
- `Import_RunsExactlyOnce_AndSetsMarker` — first import populates content + sets `StaticContentImported=true`; second import is a no-op (counts unchanged); 7 activity groups / 1 profile confirmed.

All pass (suite: **17/17**). Tests run entirely in `%TEMP%`; `App_Data` is never used or modified.
