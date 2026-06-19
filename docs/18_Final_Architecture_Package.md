# 18 — Final Architecture Package (SINGLE SOURCE OF TRUTH)

**Project:** Dr. Aly Hussein — Academic Portfolio & Content Management Platform
**Stack:** ASP.NET Core Razor Pages (.NET 8 LTS) · EF Core · SQLite (WAL) · Bootstrap 5 (RTL) · ASP.NET Core Identity
**Status:** **FINAL — approved for implementation (v2.0).** This package supersedes the v1.0 baseline where they differ. It folds in the static-site migration analysis (`16`), the pre-implementation review (`17`), and the owner's final decisions. **No application code exists yet.**

> This document is **deliverable A** (Final Architecture Package). Deliverables **B–G are the finalized numbered docs** listed in §3. Everything here is consistent with them; if any later edit diverges, **doc 18 + the finalized docs win over docs 16/17**, which are now historical analysis.

---

## 1. Owner's Final Decisions (the contract)

| # | Decision | Effect on the model |
|---|---|---|
| 1 | **Languages:** AR + EN + **FR** in V1. French content initially **empty**. DB/translations/admin/validation/routing support FR from day one. Missing FR → **fallback render**. | All `*Translation` tables already culture-keyed; admin shows AR/EN/FR tabs; `hreflang` emits FR only where present (doc 11 §5). |
| 2 | **New entities** (fully admin-managed): `Qualification`, `Skill`, `Membership`, `StatItem`, `Credibility`, `Activity`. | Added (+ `ActivityGroup`) with translation tables (docs 03–05). **All site content is dynamic.** |
| 3 | **Profile extensions:** DateOfBirth, Nationality, MaritalStatus, Location, Languages, Positioning, ShortName, CvFile. | `DateOfBirth` on `Profile`; the rest on `ProfileTranslation`; **per-culture** `CvFileId`. |
| 4 | **Book extensions:** Publisher, Role, IsFeatured. | `IsFeatured` on `ContentItem`; `Publisher` + `AuthorshipRole` on `ContentItemTranslation` (translatable). |
| 5 | **Thesis extensions:** ResearcherName, RelationshipType (Supervised/Examined/Ongoing). | `RelationshipType` on `ContentItem`; `ResearcherName` on `ContentItemTranslation`. **`Supervisor` removed.** |
| 6 | **SEO:** Redirect / UrlAlias support. | New `Redirect` table + resolution middleware + legacy `*.html` map. |
| 7 | **UI/Theme:** keep Dark Mode; **static site design is the source of truth.** | `SiteSettings.DefaultTheme`; canonical tokens/fonts pinned in doc 07 §3; ported in task F-12. |

---

## 2. What Changed from v1.0 → v2.0 (delta summary)

**New entities (8 tables incl. translations not counted):** `Qualification`, `Skill`, `Membership`, `StatItem`, `Credibility`, `ActivityGroup`, `Activity`, `Redirect`.

**Extended entities:**
- `Profile` (+ `DateOfBirth`); `ProfileTranslation` (+ ShortName, Positioning, Nationality, MaritalStatus, Location, Languages, CvFileId).
- `ContentItem` (+ `IsFeatured`, `RelationshipType`; − `Supervisor`).
- `ContentItemTranslation` (+ relocated/added translatable fields: Journal, Authors, Publisher, AuthorshipRole, ResearcherName).
- `Course` (+ `Level`); `ExperienceEntry`/translation (+ `PeriodLabel`, dates optional).
- `SiteSettings` (+ `DefaultTheme`).

**Cross-cutting:** French first-class with fallback; Western-digit house style; partial-culture `hreflang`; legacy redirect map; design-system port; first-class content-migration phase.

**Unchanged (validated, no rework):** stack, hosting, layering, TPH content model, translation-table strategy, file/storage abstraction, backup/restore, statistics model, single-admin auth.

---

## 3. Deliverable Map (A–G) → finalized documents

| Deliverable | Document | Status |
|---|---|---|
| **A. Final Architecture Package** | **`18_Final_Architecture_Package.md`** (this doc) + `02_System_Architecture.md` | ✅ Final |
| **B. Final Database Design** | `03_Database_Design.md` | ✅ Final (v2.0) |
| **C. Final ERD** | `04_ERD.md` | ✅ Final (v2.0) |
| **D. Final Entity List** | `05_Entities.md` | ✅ Final (v2.0) |
| **E. Final Admin Dashboard Structure** | `06_Admin_Dashboard_Structure.md` | ✅ Final (v2.0) |
| **F. Final Public Website Structure** | `07_Public_Website_Structure.md` | ✅ Final (v2.0) |
| **G. Final Execution Plan** | `15_Claude_Code_Execution_Plan.md` | ✅ Final (v2.0) |

**Also updated to v2.0:** `10_MultiLanguage_Strategy.md`, `11_SEO_Strategy.md`, `14_Task_Breakdown.md`.
**Historical (do not edit):** `16_Static_To_Dynamic_Migration_Plan.md`, `17_Final_PreImplementation_Review.md`.
**Unchanged from v1.0:** `01`, `02`, `08`, `09`, `12`, `13` (still valid; `08` content-strategy and `02` architecture are consistent with v2.0).

---

## 4. Canonical Entity Inventory (quick reference)

**TPH collection content** — `ContentItem` (+ `ContentItemTranslation`), discriminator `ContentType` ∈
`Book · Publication · ResearchPaper · Thesis · Project · Resource · EnrichmentItem · Video`.

**Singletons:** `Profile` (+Translation), `SiteSettings` (+Translation).

**Profile-page / list entities** (each + its `*Translation`):
`Qualification · Skill · Membership · StatItem · Credibility · ActivityGroup · Activity · ExperienceEntry · Course · PageSection`.

**Taxonomy:** `Category` (+Translation), `ContentItemCategory` (join).

**Media:** `MediaFile`. **Comms:** `ContactMessage`. **SEO:** `PageSeo`, `Redirect`. **Stats:** `ContentEvent`, `PageView`. **Identity:** ASP.NET Core Identity tables.

**Enums:** `RelationshipType{Supervised,Examined,Ongoing}` · `DegreeLevel{Master,PhD}` · `Course.Level{Undergraduate,Graduate}` · `Membership.Kind{Society,Board}` · `ProjectStatus{Ongoing,Completed}` · `MediaKind{Image,Pdf,Thumbnail}` · `DefaultTheme{Light,Dark}` · `Culture{ar,en,fr}`.

---

## 5. Content Seed Targets (from the legacy site)

| Dataset | Count | Target |
|---|---|---|
| Profile + bio | 1 | Profile/Translation + FullBio |
| Publications | 9 | ContentItem(Publication) |
| Books | 14 (3 featured) | ContentItem(Book) + Publisher/AuthorshipRole/IsFeatured |
| Theses | 57 | ContentItem(Thesis) + ResearcherName/RelationshipType/DegreeLevel |
| Courses | 16 | Course (Level UG/Grad) |
| Experience | 8 | ExperienceEntry (+PeriodLabel) |
| Qualifications | 4 | Qualification |
| Skills | 5 | Skill |
| Memberships | 10 | Membership (Society/Board) |
| Stats | 5 | StatItem |
| Credibility | 5 | Credibility |
| Activities | ~28 in 5 groups | ActivityGroup + Activity |
| Images | 3 (+favicon) | MediaFile (favicon = static asset) |

Languages seeded: **AR + EN**; **FR left empty** (fallback). Book covers and full-text PDFs do **not** exist in the legacy site — popup/preview launch empty (CSS book-cover fallback), filled later by the admin.

---

## 6. Public Routes (culture-prefixed `/{c}/…`)

`/` (home) · `about` · `experience` · `activities` · `research` · `publications` · `books` · `theses` · `projects` · `teaching` · `videos` · `enrichment` · `resources` · `contact` · `search` · `category/{slug}` · detail `…/{slug}` where applicable · `sitemap.xml` · `robots.txt`.

**Frozen naming map:** legacy `publications.html`→**Publications**, `books.html`→**Books**, `research.html`→**Activities**. Legacy `*.html` URLs 301-redirect to the AR equivalents (doc 11 §9).

---

## 7. Architecture Invariants (do not violate during build)

- Server-rendered Razor Pages monolith; **no SPA, no separate API, no microservices, no container orchestration.**
- SQLite (WAL) via EF Core; **schema changes only through named EF migrations.**
- Translatable text **only** on `*Translation` tables; non-translatable data on base rows.
- Files on disk behind `IFileStorage`; **only metadata in DB**; **video is never stored** (YouTube embeds only).
- Single Super Admin; `/admin` gated by `RequireSuperAdmin`.
- Server-side validation + rich-text sanitization are mandatory.
- The static site's tokens/fonts/components are the **visual contract** (doc 07 §3).
- French is first-class but may be empty; **never block publish on French**; fall back at render.

---

## 8. Readiness Statement

All ten gate conditions from `17_Final_PreImplementation_Review.md` §6 are now **closed**:

- ✅ Owner decisions recorded (§1).
- ✅ Entity set widened (Qualification, Skill, Membership, StatItem, Credibility, Activity/ActivityGroup) — docs 03–05.
- ✅ Profile / Book / Thesis / Course / Experience extensions applied.
- ✅ `Redirect` table + middleware in the model and plan.
- ✅ Section terminology map frozen (§6, doc 07 §1).
- ✅ French strategy + partial-culture `hreflang` defined (docs 10–11).
- ✅ Design tokens/fonts + dark mode pinned (doc 07 §3, task F-12).
- ✅ Content-migration promoted to a first-class phase (doc 14 Phase 3b, doc 15 Stage 6b).
- ✅ Digit house style decided (doc 10 §8b).
- ✅ Planning docs 03/04/05/06/07/10/11/14/15 updated to v2.0.

**Verdict: READY FOR IMPLEMENTATION.** Begin at `15_Claude_Code_Execution_Plan.md` Stage 0. The data model can be built **once** (Stages 1–2) against this package, satisfying the "design once, avoid refactoring" principle.

**Still required before deployment only (non-blocking for the build):** production domain, hosting target (OCI vs Hetzner), Super Admin credentials, and the optional decision to commission French translations.

---

## 9. Document Index (final package)

| # | File | Role |
|---|---|---|
| 00 | `00_README.md` | Package index + recommendations |
| 01 | `01_Project_Vision.md` | Goals / scope |
| 02 | `02_System_Architecture.md` | Architecture (A) |
| 03 | `03_Database_Design.md` | **B — Final DB Design** |
| 04 | `04_ERD.md` | **C — Final ERD** |
| 05 | `05_Entities.md` | **D — Final Entity List** |
| 06 | `06_Admin_Dashboard_Structure.md` | **E — Final Admin Structure** |
| 07 | `07_Public_Website_Structure.md` | **F — Final Public Structure** |
| 08 | `08_Content_Management_Strategy.md` | Per-type content handling |
| 09 | `09_File_Management_Strategy.md` | Files/storage |
| 10 | `10_MultiLanguage_Strategy.md` | Localization (v2.0) |
| 11 | `11_SEO_Strategy.md` | SEO + redirects (v2.0) |
| 12 | `12_Backup_And_Restore_Strategy.md` | Backup/restore |
| 13 | `13_Project_Roadmap.md` | Phases |
| 14 | `14_Task_Breakdown.md` | Tasks (v2.0) |
| 15 | `15_Claude_Code_Execution_Plan.md` | **G — Final Execution Plan** |
| 16 | `16_Static_To_Dynamic_Migration_Plan.md` | Migration analysis (historical) |
| 17 | `17_Final_PreImplementation_Review.md` | Readiness review (historical) |
| 18 | `18_Final_Architecture_Package.md` | **A — Final Architecture Package (this doc, SSOT)** |
