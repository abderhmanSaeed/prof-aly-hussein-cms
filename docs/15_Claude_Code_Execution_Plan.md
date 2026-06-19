# 15 — Claude Code Execution Plan (FINAL)

**Status:** **FINAL — single source of truth (v2.0).** Aligned with the v2.0 data model (docs 03–05), admin/public structure (docs 06–07), localization/SEO finals (docs 10–11), and the v2.0 task breakdown (doc 14). The legacy static site has now been supplied and analysed (docs 16/17); the content-migration step is first-class.

A step-by-step implementation sequence to hand to **Claude Code** later. It is written as an ordered build script of *instructions* (not code), each a self-contained unit ending in a verifiable checkpoint. Follow the order; do not skip ahead, because later steps assume earlier checkpoints pass.

> **Operating rules for the agent**
> - Build in small steps; after each step, ensure the solution **compiles** and the app **runs**.
> - Apply EF Core migrations explicitly; never auto-mutate the schema outside a migration.
> - Reference docs 03–05 as the source of truth for the data model; if reality diverges, update the docs, not just the code.
> - Server-side validation and HTML sanitization are mandatory wherever user/admin input is rendered.
> - Commit at every checkpoint with a clear message.
> - Do not introduce new architecture (no SPA, no extra services, no container orchestration). Stay within docs 01–14.

---

## Stage 0 — Repository & Conventions
1. Initialize the git repository and `.gitignore` for .NET.
2. Create the solution and the four projects (Web, Application, Domain, Infrastructure) per doc 02.
3. Add a `README` pointing to this planning package and the decisions in docs 01–14.
4. Decide and record conventions: nullable enabled, analyzers on, central package versions.
**Checkpoint:** empty solution builds.

## Stage 1 — Domain Model
5. Implement Domain entities and enums exactly as docs 04–05 (v2.0):
   - TPH base `ContentItem` + subtypes; all `*Translation` tables (with the relocated translatable fields `Journal`, `Authors`, `Publisher`, `AuthorshipRole`, `ResearcherName`).
   - `Category`, `MediaFile`, `Profile` (+ extended fields & per-culture `CvFileId`), `SiteSettings` (+ `DefaultTheme`), `PageSection`, `ExperienceEntry` (+ `PeriodLabel`), `Course` (+ `Level`), `ContactMessage`, `ContentEvent`, `PageView`, `PageSeo`.
   - **New entities:** `Qualification`, `Skill`, `Membership` (+ `Kind`), `StatItem`, `Credibility`, `ActivityGroup` + `Activity`, `Redirect`.
6. Encode invariants/enums (culture set ar/en/fr, `RelationshipType`, `DegreeLevel`, `Course.Level`, `Membership.Kind`, `ProjectStatus`, `MediaKind`, `DefaultTheme`, cross-field rules from doc 05). Thesis uses `ResearcherName` + `RelationshipType` (no `Supervisor`).
**Checkpoint:** Domain compiles; entity shapes match doc 05 (v2.0).

## Stage 2 — Persistence
7. Add EF Core + SQLite provider in Infrastructure; implement `AppDbContext`.
8. Configure TPH discriminator, translation table keys/uniqueness, FK delete behaviors, and all indexes from doc 03 (including the new entities, `Redirect.FromPath` unique, and the theses-facet / featured indexes).
9. Add culture `CHECK` constraints and default values.
10. Wire startup pragmas (WAL, foreign_keys, busy_timeout, synchronous).
11. Create the **initial migration**; generate the database.
**Checkpoint:** database created from migration; constraints/indexes present (inspect schema).

## Stage 3 — Cross-Cutting Foundations
12. Configure request localization (ar/en/fr; URL-segment provider; default from settings) and central culture constants.
13. Add Bootstrap 5 (LTR + RTL by culture) and **port the static design system** (doc 07 §3): token palette, per-locale fonts (Amiri/IBM Plex Sans Arabic · Cormorant/Inter), **dark-mode tokens + theme toggle**, RTL logical properties, brand "ع" glyph fallback; base `_Layout`, header/footer placeholders, AR/EN/FR language switcher stub.
14. Implement `IFileStorage` + filesystem provider (date-partitioned paths, GUID names, validation hook) per doc 09.
15. Add logging, global error handling, health check.
16. Implement seed routine (SiteSettings, Profile, sample categories).
**Checkpoint:** app runs; `/{culture}/` switches culture + direction; a file round-trips through `IFileStorage`.

## Stage 4 — Authentication
17. Integrate ASP.NET Core Identity into `AppDbContext`; migration for Identity tables.
18. Seed the single Super Admin user + `SuperAdmin` role (credentials via secure config, not hard-coded).
19. Build localized login/logout; password policy + lockout.
20. Create the `/admin` Area; apply `RequireSuperAdmin` at area level; redirect anonymous users.
21. Add Account & Security (change password).
**Checkpoint:** admin logs in and reaches an empty dashboard; anonymous blocked from `/admin`.

## Stage 5 — Application Services
22. Implement the content service (CRUD, publish, reorder, translation create/update, fallback resolution).
23. Implement the media service (validate by extension + content sniffing, store, thumbnail, usage lookup, safe delete).
24. Implement localization/content-fallback helper, SEO model builder, statistics recorder, and the backup/restore service interfaces.
**Checkpoint:** services unit-testable; a content item can be created/translated/published via a test.

## Stage 6 — Admin Content Management
25. Shared content **list** screen (filters, status, reorder, counts) bound to `ContentType`.
26. Shared content **create/edit** screen with AR/EN/FR tabs, shared fields, validation, draft/publish; missing-translation indicators.
27. Slug auto-generation (Arabic-aware) + live uniqueness validation.
28. Type-specific fields (Video YouTube ID; Publication/Research journal/authors/DOI; Thesis degree/supervisor; Project status/role; Resource type).
29. Media Library UI + media pickers wired into content edit.
30. Categories CRUD (trilingual) + assignment.
31. Profile/About + biography (extended fields + **per-culture CV upload**); Experience; Teaching (Level); Page Sections; Header; Footer; Profile & Contact Info; **Appearance/Theme**; SEO (PageSeo); **Redirects**.
31a. **List-editor screens** for the new small entities: Qualifications, Skills, Memberships (by Kind), StatItems, Credibility, **Activities** (groups + items).
31b. **Home/Profile builder**: hero, stats/credibility selection + order, **featured-Book** selection.
31c. Type specifics: Thesis (`ResearcherName`/`RelationshipType`/`DegreeLevel` + facet filters); Book (`Publisher`/`AuthorshipRole`/`IsFeatured`).
32. Rich-text editor + server-side sanitization.
33. FTS5 index + sync on content changes (incl. ResearcherName/Journal/Authors/Publisher).
34. Contact Messages inbox; statistics capture; dashboard overview.
**Checkpoint:** every content type, profile-page entity, and setting is fully manageable in AR/EN/FR; uploads validated; search index updates; all admin pages from doc 06 (v2.0) exist and persist.

## Stage 7 — Public Website
35. Public layout from settings; AR/EN/FR switcher with slug mapping; RTL correctness; theme toggle.
36. Content card + list/pagination components; section list pages; detail/slug pages.
37. **Home composition**: hero, Credibility chips, animated StatItem counters, about snapshot, featured Books, CTA band.
38. Videos grid + deferred `youtube-nocookie` embeds.
39. Book/Resource popup (cover, summary, PDF preview, Read/Download) with metric increments; **CSS book-cover fallback** when no cover image.
40. Search results (grouped); category pages; **About** (qualifications, skills, personal details, CV download), **Experience** (timeline + Memberships), **Activities** (grouped accordion), **Theses** (filter-table: relationship/degree/year + search), **Teaching** (UG/Graduate).
41. Contact form (validation, honeypot, rate limit, email + inbox).
42. Culture fallback rendering (empty French → default culture); accessibility + RTL QA.
**Checkpoint:** all published content reachable and correct in ar/en/fr (French falling back where empty); popups, embeds, theses filters, search, contact, counters work end-to-end.

## Stage 8 — SEO
43. SEO head component (title/desc/keywords, canonical, OG/Twitter, lang/dir).
44. `hreflang` alternates + `x-default` — **emit `fr` only where a French translation exists** (partial-culture rule, doc 11 §5).
45. Dynamic `sitemap.xml` (published cultures, alternates, lastmod); `robots.txt` (disallow /admin).
46. JSON-LD per page type (Person/ScholarlyArticle/Book/VideoObject/Breadcrumb/WebSite+SearchAction).
47. **Redirect middleware** (resolve `Redirect` → 301/302) + **seed the legacy `*.html` map** (doc 11 §9); slug-change 301 auto-creation.
**Checkpoint:** structured data validates; sitemap/robots correct; canonical + hreflang verified across cultures; legacy `*.html` URLs 301 to `/{culture}/…`.

## Stage 9 — Testing
48. Unit tests (services), integration tests (CRUD + authorization), E2E smoke (key flows).
49. Security tests (upload validation, XSS, path traversal, auth boundaries, rate limit).
50. Backup→restore dry run with pre-restore snapshot + schema-version check.
**Checkpoint:** tests green; security checklist clear; backup/restore cycle proven on a copy.

## Stage 10 — Deployment Prep
51. Production configuration (DB path, uploads path, secrets, request size limits, pragmas).
52. Backup service: admin export/download + nightly cron to R2/B2 + retention; restore flow.
53. Publish profile/artifact; `systemd` unit; reverse proxy config (Caddy auto-HTTPS).
54. Runbook: deploy, backup, restore, rollback.
**Checkpoint:** artifact deploys to a staging VM; HTTPS works; a backup is produced and restored successfully.

---

## How to Drive Claude Code Through This
- **One stage per session focus.** Give Claude Code the relevant doc(s) + the stage's steps; ask it to implement to the next checkpoint, then stop and report.
- **Always verify the checkpoint** before moving on (build, run, migrate, test).
- **Feed it the data model docs verbatim** (03–05) so entity/field names stay consistent across stages.
- **Guard the boundaries:** if a step seems to require new infrastructure or a new pattern, pause and reconcile with docs 01–02 before proceeding.
- **Migrations are commits too:** every schema change is a named EF migration, reviewed before apply.
- **Content migration** is now a **first-class step after Stage 6** (doc 14 Phase 3b, tasks M-01…M-07). Source inventory: `16_Static_To_Dynamic_Migration_Plan.md` Part A. Seed AR/EN; leave French empty (fallback applies).

## Stage 6b — Content Migration / Seed (after Stage 6)
55. Seed Profile + SiteSettings + bio; import Publications(9), Books(14, with Publisher/AuthorshipRole/IsFeatured), Theses(57, ResearcherName/RelationshipType/DegreeLevel), Courses(16, Level), Experience(8, PeriodLabel), Qualifications(4), Skills(5), Memberships(10), StatItems(5), Credibility(5), Activities(~28 in groups).
56. Import the 3 photos into the Media Library; wire Profile photo; ship favicon as a fixed app static asset (SVG is rejected by the upload pipeline).
57. Seed the legacy `*.html` → `/{culture}/…` `Redirect` rows; apply Western-digit house style.
**Checkpoint:** the public site renders the real CV content in AR/EN (French falling back); the homepage featured/stats/credibility reflect seeded data.

## Pre-Build Inputs (status)
- ✅ The **previous static website project** and **existing content/structure** — **supplied and analysed** (docs 16/17); inventory drives Stage 6b.
- ⏳ Final **domain name** and chosen **hosting target** (OCI vs Hetzner) for Stage 10.
- ⏳ The **Super Admin credentials** (set via secure config at deploy time).
- ⏳ Decision (future, not blocking): whether to **commission French translations** or keep French empty with fallback at launch.
