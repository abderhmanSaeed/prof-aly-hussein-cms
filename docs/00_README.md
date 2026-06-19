# 00 — Planning Package Index & Final Recommendations

**Project:** Dr. Aly Hussein — Academic Portfolio & Content Management Platform
**Stack:** ASP.NET Core Razor Pages (.NET 8 LTS) · EF Core · SQLite · Bootstrap 5
**Model:** Single Super Admin · public site (no login) · trilingual (AR/EN/FR) · dynamic CMS · image/PDF uploads · YouTube video integration
**Status:** Planning & architecture phase — **no application code yet.**

---

## Document Index

| # | File | Purpose |
|---|---|---|
| 01 | `01_Project_Vision.md` | Goals, objectives, scope, out of scope |
| 02 | `02_System_Architecture.md` | High-level + Razor Pages + layers + deployment + hosting |
| 03 | `03_Database_Design.md` | Tables, relationships, constraints, indexes, pragmas, FTS |
| 04 | `04_ERD.md` | Full Mermaid ERD + cardinality |
| 05 | `05_Entities.md` | Entity catalogue, properties, validation rules |
| 06 | `06_Admin_Dashboard_Structure.md` | Side menu, admin pages, CRUD pattern, permissions |
| 07 | `07_Public_Website_Structure.md` | Public pages, navigation, user flows |
| 08 | `08_Content_Management_Strategy.md` | Per-type content handling + categories |
| 09 | `09_File_Management_Strategy.md` | Images, PDFs, storage structure, naming, security |
| 10 | `10_MultiLanguage_Strategy.md` | AR/EN/FR localization (UI + content), RTL |
| 11 | `11_SEO_Strategy.md` | Metadata, slugs, sitemap, hreflang, structured data |
| 12 | `12_Backup_And_Restore_Strategy.md` | Backup, restore, export, disaster recovery |
| 13 | `13_Project_Roadmap.md` | Seven implementation phases with exit criteria |
| 14 | `14_Task_Breakdown.md` | Detailed tasks (ID, priority, dependencies, complexity) |
| 15 | `15_Claude_Code_Execution_Plan.md` | Ordered build sequence for Claude Code |

**Pending inputs (do not block architecture):** the previous static website project and existing content/structure — needed for the content inventory, data seeding, and legacy→new 301 redirect map.

---

## Final Recommendations

### 1. Final Technology Stack
- **ASP.NET Core Razor Pages on .NET 8 (LTS)** — server-rendered monolith; native SEO; one deployable artifact; the simplest auth model for a single admin.
- **EF Core** with the SQLite provider; **TPH** content model + **translation tables** for trilingual content (the two decisions that prevent future refactoring).
- **Bootstrap 5** (with RTL) for responsive UI.
- **ASP.NET Core Identity** scoped to one Super Admin.
- **SQLite FTS5** for search; **SMTP** for the contact form; **Caddy** for automatic HTTPS in production.
- No SPA, no separate API, no container orchestration — deliberately.

### 2. Database Choice
- **SQLite in WAL mode.** Single writer + read-mostly traffic makes it ideal: zero administration, file-based backup, full EF Core support. **Upgrade path preserved** — moving to PostgreSQL later is a provider swap plus regenerated migrations, so this is not a dead end.

### 3. Hosting Choice
- **A single small Linux VM.** Either **Oracle Cloud Always Free** (Ampere A1; note the free allowance was reduced to 2 OCPU / 12 GB on 15 Jun 2026 but remains ample, with capacity availability the main caveat) for $0, or **Hetzner ARM (~€4/month)** for predictable "set and forget" reliability. Kestrel behind Caddy (auto-HTTPS), app as a `systemd` service. The same app can run on Windows/IIS if that environment is already available.

### 4. Storage Strategy
- **Local filesystem** for images and PDFs, date-partitioned with GUID stored-names; **only metadata in the database** (`MediaFile`). Behind an `IFileStorage` abstraction so it can move to **Cloudflare R2 / Backblaze B2** later without code changes. **Video is never stored** — YouTube embeds only. Backups capture `app.db` + the `uploads/` tree together, pushed nightly off-site.

### 5. Long-Term Maintenance Strategy
- **One process, one database file, one uploads folder, one backup set** — a single maintainer can own it for years.
- **LTS runtime** (.NET 8) reduces churn; plan a deliberate jump to the next LTS when current support nears end.
- **Automated nightly off-site backups** with retention and a **proven restore runbook** (pre-restore snapshot, schema-version check).
- **Migrations only** for schema change; **content lives in the database**, so day-to-day updates need no deploys.
- **Abstractions at the two likely change points** (storage and database provider) keep the cheap-now choices from becoming expensive-later traps.
- Keep dependencies minimal and free; revisit the out-of-scope list only when a real need appears.
