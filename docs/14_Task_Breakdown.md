# 14 — Task Breakdown (FINAL)

**Status:** **FINAL — single source of truth (v2.0).** Incorporates the v2.0 entity set, profile/book/thesis/course extensions, the `Redirect` table, the design-system port, and a first-class content-migration task.

Tasks are grouped by phase (doc 13). Each has an **ID**, **Description**, **Priority** (P0 critical · P1 high · P2 optional), **Dependencies**, and **Complexity** (XS · S · M · L).

> Complexity reflects effort/risk, not calendar time. Dependencies use task IDs.

---

## Phase 1 — Foundation (F)

| ID | Description | Priority | Dependencies | Complexity |
|---|---|---|---|---|
| F-01 | Create solution + projects (Web/Application/Domain/Infrastructure), .NET 8 LTS | P0 | — | S |
| F-02 | Add Bootstrap 5 (LTR+RTL), base `_Layout`, shared partial placeholders | P0 | F-01 | S |
| F-03 | Define Domain entities + enums from docs 04–05 — **full v2.0 set**: TPH `ContentItem`(+subtypes), all `*Translation` tables, `Category`, `MediaFile`, `Profile`, `SiteSettings`, `PageSection`, `ExperienceEntry`, `Course`, `ContactMessage`, `ContentEvent`, `PageView`, `PageSeo`, **and the new entities `Qualification`, `Skill`, `Membership`, `StatItem`, `Credibility`, `ActivityGroup`+`Activity`, `Redirect`** | P0 | F-01 | L |
| F-03a | Encode v2.0 enums (`RelationshipType`, `DegreeLevel`, `Course.Level`, `Membership.Kind`, `ProjectStatus`, `MediaKind`, `DefaultTheme`) + cross-field rules (doc 05) | P0 | F-03 | S |
| F-04 | Configure `AppDbContext`: TPH, translation tables, keys, indexes, constraints | P0 | F-03 | L |
| F-05 | Create initial EF Core migration; apply on startup (dev) | P0 | F-04 | S |
| F-06 | Wire SQLite pragmas (WAL, FK on, busy_timeout, synchronous) | P0 | F-05 | XS |
| F-07 | Configure request localization (ar/en/fr, URL-segment provider, default) | P0 | F-02 | M |
| F-08 | `IFileStorage` abstraction + filesystem implementation + uploads root config | P0 | F-01 | M |
| F-09 | Seed data (SiteSettings, Profile, categories, role/admin placeholder) | P1 | F-05 | S |
| F-10 | Logging + health-check endpoint + global error handling | P1 | F-01 | S |
| F-11 | Central culture constants/enum shared by routing, resources, DB checks (ar/en/**fr**) | P1 | F-07 | XS |
| F-12 | Port the static design system into the Razor/Bootstrap theme: token palette, per-locale fonts (Amiri/IBM Plex Sans Arabic · Cormorant/Inter), **dark-mode token set + toggle**, RTL logical properties, brand "ع" glyph fallback (doc 07 §3) | P0 | F-02 | M |

## Phase 2 — Authentication (A)

| ID | Description | Priority | Dependencies | Complexity |
|---|---|---|---|---|
| A-01 | Integrate ASP.NET Core Identity (Identity tables in `AppDbContext`) | P0 | F-04 | M |
| A-02 | Seed single Super Admin user + `SuperAdmin` role | P0 | A-01, F-09 | S |
| A-03 | Login/logout pages (localized) with password policy + lockout | P0 | A-01 | M |
| A-04 | `/admin` Area scaffolding + `RequireSuperAdmin` policy at area level | P0 | A-03 | S |
| A-05 | Anonymous→public redirect; admin route protection verified | P0 | A-04 | S |
| A-06 | Account & Security page (change password) | P1 | A-03 | S |

## Phase 3 — Content Management (C)

| ID | Description | Priority | Dependencies | Complexity |
|---|---|---|---|---|
| C-01 | Content application service (CRUD, publish, reorder, translation handling) | P0 | F-04 | L |
| C-02 | Shared admin content list screen (filters, status, reorder, counts) | P0 | A-04, C-01 | M |
| C-03 | Shared admin create/edit screen with AR/EN/FR tabs + validation | P0 | C-02 | L |
| C-04 | Slug auto-generation (Arabic-aware) + uniqueness validation | P0 | C-03 | M |
| C-05 | Type-specific field handling (Video/Publication/Research/Thesis/Project/Resource) | P0 | C-03 | M |
| C-06 | Media Library: upload (validation + sniffing), thumbnail gen, list, delete-with-usage | P0 | F-08 | L |
| C-07 | Media pickers integrated into content edit (cover/PDF) | P0 | C-06, C-03 | M |
| C-08 | Categories CRUD (trilingual) + assignment UI | P0 | C-01 | M |
| C-09 | Profile/About + biography management | P0 | A-04 | M |
| C-10 | Experience entries CRUD | P1 | C-01 | S |
| C-11 | Teaching/Courses CRUD | P1 | C-01 | S |
| C-12 | Page Sections (Home/About/Contact) management | P1 | C-01 | S |
| C-13 | Header management (logo, nav labels, switcher) | P0 | C-06 | M |
| C-14 | Footer management (text + social links) | P0 | A-04 | S |
| C-15 | SEO (PageSeo) management for static pages + defaults | P1 | A-04 | S |
| C-16 | Rich-text editor + server-side HTML sanitization | P0 | C-03 | M |
| C-17 | FTS5 setup + sync on content create/update/delete | P0 | C-01 | M |
| C-18 | Contact Messages inbox (list/read/delete/unread badge) | P1 | A-04 | S |
| C-19 | Statistics capture (events + counters) | P1 | F-04 | M |
| C-20 | Admin dashboard overview (KPIs, recent activity, stats snapshot) | P1 | C-19 | M |
| C-21 | List-editor screens for the new small entities: **Qualifications, Skills, Memberships (by Kind), StatItems, Credibility** (trilingual, drag-reorder) | P0 | C-01 | M |
| C-22 | **Activities** management (ActivityGroups + Activities, ordered, trilingual) | P0 | C-01 | S |
| C-23 | **Home/Profile builder**: hero, stats/credibility selection + order, **featured-Book** selection, extended Profile fields + **per-culture CV upload** | P0 | C-09, C-21 | M |
| C-24 | Thesis screen specifics: `ResearcherName`, `RelationshipType`, `DegreeLevel`; tabular list-editor + facet filters | P0 | C-03 | M |
| C-25 | Book screen specifics: `Publisher`, `AuthorshipRole`, `IsFeatured` | P0 | C-03 | S |
| C-26 | **Appearance/Theme** admin (default theme, colors) + **Redirects** admin (URL aliases) | P1 | A-04 | S |

## Phase 4 — Public Website (P)

| ID | Description | Priority | Dependencies | Complexity |
|---|---|---|---|---|
| P-01 | Public layout: header/footer from settings, switcher (slug mapping), RTL | P0 | C-13, C-14, F-07 | M |
| P-02 | Reusable content card + list/pagination components | P0 | P-01 | M |
| P-03 | Section list pages (Research/Publications/Books/Theses/Projects/Resources/Enrichment) | P0 | P-02, C-01 | M |
| P-04 | Content detail/slug pages with metadata + download | P0 | P-03 | M |
| P-05 | Home page composition (previews + View All) | P0 | P-02 | M |
| P-06 | Videos grid + deferred `youtube-nocookie` embed (modal/detail) | P0 | P-02 | M |
| P-07 | Book/Resource popup: cover, summary, PDF preview, Read/Download | P0 | P-02, C-06 | L |
| P-08 | Search results page over FTS (grouped by type) | P1 | C-17, P-02 | M |
| P-09 | Category pages (cross-type listing) | P1 | C-08, P-02 | S |
| P-10 | Contact form (validation, honeypot, rate limit, email + inbox) | P0 | C-18 | M |
| P-11 | About/Experience/Teaching public pages | P1 | C-09, C-10, C-11 | S |
| P-12 | Metric increments (view/download/play) wired to public actions | P1 | C-19 | S |
| P-13 | Culture fallback rendering for missing translations | P1 | P-03 | S |
| P-14 | Accessibility pass (alt text, keyboard, semantics, RTL QA) | P1 | P-01..P-11 | M |

## Phase 5 — SEO (S)

| ID | Description | Priority | Dependencies | Complexity |
|---|---|---|---|---|
| S-01 | SEO head component (title/desc/keywords, canonical, OG/Twitter, lang/dir) | P0 | P-01 | M |
| S-02 | `hreflang` alternates + `x-default` across all pages | P0 | S-01 | S |
| S-03 | Dynamic `sitemap.xml` (all cultures, alternates, lastmod) | P0 | P-03 | M |
| S-04 | `robots.txt` (disallow /admin, reference sitemap) | P0 | S-03 | XS |
| S-05 | JSON-LD per page type (Person/ScholarlyArticle/Book/VideoObject/Breadcrumb/WebSite) | P1 | S-01 | M |
| S-06 | Slug-change 301 redirect support + legacy URL redirect map (auto-create on slug change) | P1 | C-04 | M |
| S-07 | **Redirect resolution middleware** (resolve `Redirect.FromPath` → 301/302 before routing) + seed the legacy `*.html` map (doc 11 §9) | P0 | F-03, S-03 | S |
| S-08 | Partial-culture `hreflang` (emit `fr` only where a French translation exists) | P0 | S-02 | S |

## Phase 6 — Testing (T)

| ID | Description | Priority | Dependencies | Complexity |
|---|---|---|---|---|
| T-01 | Unit tests: content/media/localization/SEO/backup services | P0 | Phase 3–5 | M |
| T-02 | Integration tests: CRUD + authorization boundaries | P0 | Phase 3 | M |
| T-03 | E2E smoke: browse, popup, video, search, contact | P1 | Phase 4 | M |
| T-04 | Security: upload validation, XSS, path traversal, auth, rate limit | P0 | Phase 3–4 | M |
| T-05 | Backup→restore dry run (snapshot + schema-version check) | P0 | B tasks (D-06) | M |
| T-06 | Accessibility + RTL + performance spot checks | P1 | Phase 4 | S |

## Phase 7 — Deployment (D)

| ID | Description | Priority | Dependencies | Complexity |
|---|---|---|---|---|
| D-01 | Provision VM (OCI Always Free / Hetzner); OS hardening; service account | P0 | — | M |
| D-02 | Install .NET 8 runtime; deploy artifact; `systemd` unit for Kestrel | P0 | D-01 | M |
| D-03 | Reverse proxy + automatic HTTPS (Caddy / Nginx+Certbot); domain/TLS | P0 | D-02 | M |
| D-04 | Configure prod paths (DB, uploads), secrets, pragmas, request size limits | P0 | D-02 | S |
| D-05 | Production migration apply + seed admin (secure credentials) | P0 | D-04 | S |
| D-06 | Backup service (admin export/download) + nightly cron to R2/B2 + retention | P0 | F-08, C-12 | L |
| D-07 | Restore flow (validated, reversible, pre-restore snapshot) | P0 | D-06 | M |
| D-08 | DNS cutover + legacy 301 redirects live | P0 | S-06, D-03 | S |
| D-09 | Submit sitemap to search consoles | P1 | S-03, D-08 | XS |
| D-10 | Monitoring, log rotation, runbook (deploy/backup/restore/rollback) | P1 | D-02 | M |

---

## Critical Path (high-level)
`F-04 → F-05 → A-01 → A-04 → C-01 → C-03 → C-06 → P-01 → P-03 → P-07 → S-01 → S-03 → T-05 → D-02 → D-03 → D-06 → D-08`

The data model (F-04) and the shared content edit screen (C-03) are the two highest-leverage tasks: most other work depends on them. Get them right before breadth.

## Phase 3b — Content Migration / Seed (M) — *first-class task (legacy content now supplied)*

| ID | Description | Priority | Dependencies | Complexity |
|---|---|---|---|---|
| M-01 | Seed `Profile` (+AR/EN translations, personal details), `SiteSettings`, bio (`FullBio`) | P0 | C-09 | S |
| M-02 | Import **9 Publications** + **14 Books** (with Publisher/AuthorshipRole/IsFeatured) as `ContentItem`+translations | P0 | C-03 | M |
| M-03 | Import **57 Theses** (ResearcherName, RelationshipType, DegreeLevel, Year) | P0 | C-24 | M |
| M-04 | Import **16 Courses** (Level), **8 Experience** entries (PeriodLabel), **4 Qualifications**, **5 Skills**, **10 Memberships**, **5 StatItems**, **5 Credibility**, **~28 Activities** in groups | P0 | C-21, C-22 | M |
| M-05 | Import 3 images (hero/about/contact) into Media Library; wire Profile photo; ship favicon as a static app asset (not via Media Library) | P0 | C-06 | S |
| M-06 | Leave **French translations empty** (fallback applies); flag completeness in admin | P0 | M-01..M-04 | XS |
| M-07 | Apply Western-digit house style; map free-text periods to dates + `PeriodLabel` | P1 | M-04 | S |

> Source inventory & exact counts: `16_Static_To_Dynamic_Migration_Plan.md` Part A. Book covers and full-text PDFs do not exist in the legacy site — the popup/preview features launch empty (CSS book-cover fallback) and are populated as the admin uploads files.

## Notes
- P0 tasks define a shippable v1; P1 add completeness; P2 (none critical here) would be future polish.
