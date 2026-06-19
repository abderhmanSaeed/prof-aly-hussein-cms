# 13 — Project Roadmap

Implementation is sequenced so that foundational decisions (data model, localization, storage) are built once and everything else layers on top — minimizing rework. Each phase has clear deliverables and exit criteria. Order matters more than calendar estimates for a single-developer/agent build.

---

## Phase 1 — Foundation
**Goal:** a runnable skeleton with the data model, localization, and storage abstractions in place.

**Deliverables**
- Solution structure (Web / Application / Domain / Infrastructure) and .NET 8 project setup.
- Bootstrap 5 integrated (LTR + RTL); base layout, shared partials placeholders.
- `AppDbContext` with the full entity model from docs 03–05; TPH configured; initial EF Core **migration**.
- SQLite pragmas (WAL, FK on, busy_timeout) wired at startup.
- Request localization configured (ar/en/fr, URL-segment provider, default culture).
- `IFileStorage` abstraction + filesystem implementation; uploads root configured.
- Seed routine (default `SiteSettings`, `Profile`, Super Admin user/role placeholder, a few `Category` rows).
- Health-check page; logging configured.

**Exit criteria:** app builds and runs; database is created from migrations; switching `/{culture}/` changes culture and direction; a test file can be stored and retrieved via `IFileStorage`.

---

## Phase 2 — Authentication
**Goal:** secure the admin area for the single Super Admin.

**Deliverables**
- ASP.NET Core Identity integrated; single Super Admin seeded; `SuperAdmin` role.
- Login / logout pages (localized); password policy + lockout.
- `RequireSuperAdmin` policy applied to the entire `/admin` Area.
- Anonymous → public only; `/admin` redirects to login.
- Account & Security page (change password).

**Exit criteria:** admin can log in and reach an (empty) dashboard; anonymous users are blocked from `/admin`; lockout and password rules verified.

---

## Phase 3 — Content Management (Admin)
**Goal:** full CRUD for all content and settings — the core of the system.

**Deliverables**
- Shared content CRUD screen (list + create/edit with AR/EN/FR tabs, media pickers, categories, publish/sort) driving all `ContentType`s.
- Type-specific fields wired (YouTube ID for Video; journal/authors/DOI; degree level; project status; resource type).
- Media Library (upload with validation/sniffing, thumbnail generation, usage display, delete-with-warning).
- Categories CRUD (trilingual).
- Profile/About, Experience, Teaching (Courses), Page Sections, Header, Footer, Profile & Contact Info, SEO (PageSeo) management.
- Server-side validation rules from doc 05; rich-text sanitization.
- FTS5 index maintained on content changes.
- Contact Messages inbox (read/unread/delete).
- Statistics capture (events + counters) and a basic dashboard.

**Exit criteria:** the professor can create, translate, categorize, attach media to, publish, reorder, and delete every content type; uploads enforced; search index updates; all settings editable.

---

## Phase 4 — Public Website
**Goal:** the visitor-facing site rendering all managed content.

**Deliverables**
- Public pages and routes from doc 07 (Home with previews + View All; section lists; detail/slug pages; Videos grid with deferred embeds; Books/Resources popup with PDF preview + Read/Download; Contact form; Search results; Category pages).
- Header/footer rendered from settings; language switcher with slug mapping; RTL correctness.
- Pagination/load-more; breadcrumbs; content cards/components.
- Download/play/view metric increments wired to public actions.
- Accessibility pass (alt text, keyboard nav, semantics).

**Exit criteria:** every published item is reachable and correct in all three cultures; popups, embeds, search, contact form, and counters work end-to-end; Arabic renders RTL throughout.

---

## Phase 5 — SEO
**Goal:** make every page and language discoverable.

**Deliverables**
- SEO `<head>` component (title/description/keywords, canonical, OG/Twitter, `lang`/`dir`).
- `hreflang` alternates + `x-default` on all pages.
- Dynamic `sitemap.xml` (all cultures, alternates, lastmod) + `robots.txt` (disallow `/admin`).
- JSON-LD per page type (Person, ScholarlyArticle, Book, VideoObject, BreadcrumbList, WebSite+SearchAction).
- Slug stability + 301 redirect support; legacy-URL redirect map (when old site supplied).

**Exit criteria:** validated structured data; sitemap and robots correct; canonical/hreflang verified across cultures; redirects in place.

---

## Phase 6 — Testing
**Goal:** confidence and regression safety before deployment.

**Deliverables**
- Unit tests for services (content, media validation, localization fallback, SEO model, backup).
- Integration tests for CRUD flows and authorization (admin vs anonymous).
- End-to-end smoke tests of key user flows (browse, popup, video, search, contact).
- Security checks: upload validation, XSS sanitization, path traversal, auth boundaries, rate limiting on contact form.
- Performance/accessibility spot checks; RTL visual QA.
- Backup/restore dry run including a pre-restore snapshot and schema-version check.

**Exit criteria:** tests pass; security checklist clear; a full backup→restore cycle verified on a copy.

---

## Phase 7 — Deployment
**Goal:** live, secure, backed-up production.

**Deliverables**
- Provision VM (Oracle Always Free or Hetzner); OS hardening; service account.
- Install .NET 8 runtime; deploy artifact; `systemd` unit for Kestrel.
- Reverse proxy (Caddy auto-HTTPS or Nginx + Certbot); domain + TLS.
- Configure uploads path, DB path, app settings/secrets; `PRAGMA`s confirmed.
- Nightly backup cron to R2/B2; retention; first restore test in prod context.
- DNS cutover; legacy 301 redirects live; submit sitemap to search consoles.
- Monitoring/log rotation; documented runbook (deploy, backup, restore, rollback).

**Exit criteria:** site is live over HTTPS in all cultures; admin can manage content in production; automated off-site backups confirmed; recovery runbook validated.

---

## Sequencing Rationale
- **Model and localization first** (Phase 1) because everything else depends on them; changing them later is the most expensive refactor.
- **Auth before admin CRUD** (Phase 2 → 3) so content screens are built inside the secured area from the start.
- **Admin before public** (Phase 3 → 4) so there is real data to render and the public layer consumes a stable model.
- **SEO after public** (Phase 5) because it decorates rendered pages.
- **Test, then deploy** (Phases 6 → 7) with backup/restore proven before go-live.
