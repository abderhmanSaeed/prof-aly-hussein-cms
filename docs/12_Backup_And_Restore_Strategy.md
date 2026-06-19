# 12 — Backup & Restore Strategy

The whole point of SQLite + filesystem storage is that a complete backup is **a file copy**. The strategy has two complementary parts: automated off-site backups (operations) and an admin-triggered export/restore (the requirement's "Backup & Restore from the dashboard").

---

## 1. What Constitutes a Complete Backup

A consistent backup set is:
1. `app.db` — the SQLite database (all content, settings, metadata, stats, identity).
2. `uploads/` — the entire media tree (images, thumbnails, PDFs).
3. A small **manifest** (timestamp, app version, schema/migration version, checksums) so a restore can validate compatibility and pairing.

Because file metadata lives in the DB and bytes live on disk, the two must be captured together to stay consistent.

## 2. Backup Process

### 2.1 Automated (operational, recommended)
- **Database:** use SQLite's safe online backup (`VACUUM INTO 'backup.db'` or the backup API) so the copy is consistent even while the app runs — never copy a live WAL DB with a naive `cp`.
- **Uploads:** archive the `uploads/` tree (tar/zip).
- **Schedule:** nightly cron on the VM produces `backup-{yyyymmdd-hhmm}.tar` containing the consistent DB copy + uploads archive + manifest.
- **Off-site:** push the archive to object storage with cheap/free egress (**Cloudflare R2** or **Backblaze B2**). Off-box storage is what protects against VM loss.
- **Retention:** e.g. keep 7 daily + 4 weekly + 3 monthly; prune older. Verify the latest archive opens.

### 2.2 Admin-triggered export (the dashboard feature)
- **Backup & Restore** admin page → **"Create Backup"** builds the same archive on demand and offers it as a **download** (so the professor can keep a personal copy).
- Optionally also lists recent automated backups (if surfaced from storage).
- Long operations run as a background task with progress, not a blocking request.

## 3. Restore Process

1. Admin uploads (or selects) a backup archive on the **Restore** page.
2. System **validates the manifest** — checksums and, importantly, the **schema/migration version**. If the backup predates the current schema, apply EF Core migrations after restore (forward-only) or refuse if incompatible.
3. Put the app in a brief maintenance state.
4. Replace `app.db` with the backup's DB copy and restore the `uploads/` tree (transactional swap: restore into a temp location, then atomically switch).
5. Run pending EF migrations if needed.
6. Verify (row counts, sample media resolves) and exit maintenance.

**Safety rules**
- Restore always creates a **pre-restore snapshot** of the current state first, so a bad restore is itself reversible.
- Restore is a destructive, confirmed action (typed confirmation).
- Never restore over a running write transaction; coordinate the swap.

## 4. Export Strategy (data portability)

Beyond full backups, support lighter exports for portability and peace of mind:
- **Content export (JSON):** dump content items + translations + categories to JSON for archival or migration. Useful if the professor ever moves platforms.
- **Media manifest (CSV):** list of files with original names and usage.
- These exports are read-only and don't replace the full backup for disaster recovery.

## 5. Disaster Recovery (rebuild from zero)

With an off-site archive, full recovery on a fresh VM is:
1. Provision VM, install runtime + reverse proxy.
2. Deploy the app artifact.
3. Pull the latest archive from R2/B2.
4. Restore `app.db` + `uploads/`.
5. Point DNS / TLS and go live.

Target: recoverable within an hour from nothing but the deploy artifact and the latest backup.

## 6. Operational Checklist
- [ ] WAL-safe DB backup (no naive copy of a live DB).
- [ ] DB + uploads captured together with a manifest.
- [ ] Nightly automated backup to off-site object storage.
- [ ] Retention/prune policy and periodic restore test.
- [ ] Admin dashboard "Create Backup" (download) + "Restore" (validated, reversible).
- [ ] Pre-restore snapshot on every restore.
- [ ] Schema-version check during restore.

## 7. Why This Is Cheap and Robust
- No database server means no `pg_dump`/SQL Server tooling or DB host to maintain.
- One archive contains everything; restore is deterministic.
- Off-site storage cost is negligible at this data volume, and egress on R2/B2 is free/near-free.
