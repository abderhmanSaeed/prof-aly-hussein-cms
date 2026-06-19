# 07 — Public Website Structure (FINAL)

**Status:** **FINAL — single source of truth (v2.0).** The public site is server-rendered, responsive (Bootstrap 5), **trilingual (ar/en/fr)**, and requires no login. URLs are culture-prefixed: `/{culture}/...`. The **static `ProfAly.Static` site is the canonical design reference** — the dynamic site reproduces its look, components, and behaviour.

> **v2.0 additions:** French as a first-class culture; an **Activities** page (legacy `research.html`); Qualifications/Skills on About; Memberships on Experience; dynamic Stats/Credibility/Featured on Home; the theses filter-table and activities accordion as first-class components; dark-mode retained; the design-system inventory pinned (§3).

---

## 1. Public Pages

| Page | Route (example) | Content source |
|---|---|---|
| Home | `/{c}/` | `PageSection` (hero) + `Credibility` + `StatItem` + about snapshot + featured `Book`s + CTA |
| About | `/{c}/about` | `Profile` (+ personal details + CV) + biography + `Qualification` + `Skill` |
| Experience | `/{c}/experience` | `ExperienceEntry` timeline + `Membership` (Societies/Boards) |
| Activities | `/{c}/activities` | `ActivityGroup` → `Activity` (accordions) — *legacy `research.html`* |
| Research | `/{c}/research` | `ContentItem` (ResearchPaper) list |
| Research detail | `/{c}/research/{slug}` | single item |
| Publications | `/{c}/publications` | `ContentItem` (Publication) list + year/text filter |
| Books | `/{c}/books` | `ContentItem` (Book) grid + popup + text search |
| Theses | `/{c}/theses` | `ContentItem` (Thesis) **filter-table** |
| Projects | `/{c}/projects` | `ContentItem` (Project) list |
| Teaching | `/{c}/teaching` | `Course` list (Undergraduate / Graduate columns) |
| Videos | `/{c}/videos` | `ContentItem` (Video) grid (YouTube embeds) |
| Enrichment | `/{c}/enrichment` | `ContentItem` (EnrichmentItem) list |
| Resources | `/{c}/resources` | `ContentItem` (Resource) list |
| Contact | `/{c}/contact` | contact form + contact info + headshot |
| Search results | `/{c}/search?q=...` | FTS over content |
| Category view | `/{c}/category/{slug}` | items in a topic |
| Sitemap | `/sitemap.xml` | dynamic, all cultures |
| Robots | `/robots.txt` | static + sitemap reference |

Detail/slug routes exist for content types whose items warrant their own page (Research, Publications, Books, Theses, Projects, Enrichment, Resources). **Theses** are primarily a filter-table (no per-item detail required, matching the legacy UX); a detail page is optional. **Videos** live in a grid with inline modal/embed.

> **Section naming map (frozen):** legacy `الأبحاث`/`publications.html` → **Publications**; legacy `المؤلَّفات`/`books.html` → **Books**; legacy `المشروعات`/`research.html` → **Activities**. `ResearchPaper` is reserved for future academic-research items distinct from journal Publications.

## 2. Navigation (Header)

```
[Logo/ع]  Home  About  Experience  Activities  Research  Publications  Books
          Theses  Projects  Teaching  Videos  Enrichment  Resources  Contact
          [🔍 Search]  [Theme ☾]  [AR | EN | FR]
```

- All labels editable from **Header** management and localized per culture.
- Collapses into a Bootstrap offcanvas drawer on mobile; sticky condensed navbar on scroll.
- **Theme toggle** (light/dark, persisted client-side; first-visit default from `SiteSettings.DefaultTheme`).
- **Language switcher** (AR/EN/FR) preserves the current page by mapping to the equivalent slug/route.
- If the full set is too wide, group lesser-used items (Theses, Enrichment, Resources, Projects) under a **"More"** dropdown — a presentation choice, not a data one.

## 3. Design System (CANONICAL — from `ProfAly.Static/assets/css/style.css`)

The dynamic theme must reproduce these tokens and fonts (the implemented CSS values are authoritative; they differ slightly from the original master prompt).

**Color tokens**
- `--primary: #0B5D3B` (deep Al-Azhar green), `--primary-700: #084229`, `--primary-300: #2F8460`, `--primary-050: #E9F2ED`
- `--accent: #C8A45D` (brass/gold), `--accent-600: #A8842F`, `--accent-050: #F7F0DF`
- `--bg: #F8F8F8`, `--surface: #FFFFFF`, `--surface-2: #F1F0EC`, `--ink: #1F2937`, `--muted: #5B6472`, `--line: #E4E2DB`
- **Dark mode** (`[data-theme="dark"]`): `--bg:#0E1512`, `--surface:#15201B`, `--ink:#ECECE6`, `--muted:#A6B0AB`, `--line:#243029` (full token set retained).

**Typography (per-locale swap)**
- AR headings **Amiri**; AR body **IBM Plex Sans Arabic** (fallback Cairo); line-height 1.95.
- EN/FR headings **Cormorant Garamond**; body **Inter**; line-height 1.7.

**Other tokens:** `--radius:14px`, soft single shadow, section padding `clamp(3.5rem,7vw,6.5rem)`, nav height `76px`, max width `1200px`. **RTL-first via CSS logical properties** (mirrors automatically for Arabic). `prefers-reduced-motion` respected; focus-visible rings on accent. Brand mark = "ع" glyph when no logo image is set.

> French uses the Latin (EN) font stack and LTR direction.

## 4. Homepage Composition

```
Hero            → Profile photo, name, title, positioning (PageSection + Profile)
Credibility     → institution chips (Credibility entity)
Stats           → animated counters (StatItem entity)
About snapshot  → short bio + "View All → About"
Featured Books  → IsFeatured books + "View All →"
[Latest sections as configured: Publications / Videos / Resources …] + "View All →"
CTA band        → quick contact + social
```
Each preview pulls top-`SortOrder`/featured/most-recent published items of its type in the active culture. Section visibility/order is admin-controlled (doc 06 §6).

## 5. Key Content Components (reproduce from the static site)

- **Content card:** cover/thumbnail, title, summary, category badge, link. Reused across list pages.
- **Book/Resource popup (modal):** cover, summary, embedded PDF preview (PDF.js or `<embed>`), **Read/Download** buttons. Open → `ViewCount`; download → `DownloadCount`. *(Launches without PDFs/covers until the admin uploads them; CSS book-cover fallback — spine + initial + year — when no cover image.)*
- **Video card/modal:** YouTube thumbnail → embedded `youtube-nocookie` player (deferred until interaction). Play → `Play` event.
- **Theses filter-table:** columns # / Researcher / Title / Degree / Year / Category; tabs (All/Supervised/Examined/Ongoing), Master/PhD select, year sort, text search, result count.
- **Activities accordion:** one collapsible per `ActivityGroup`, item count badge, list of `Activity` lines.
- **Stat counter:** IntersectionObserver count-up to `Value` + `Suffix`.
- **Credibility chips · skill chips · qualification list · experience timeline · publication item · course lists · panels / meta-list (personal details) · CTA band · page-hero with breadcrumb · empty states.**
- **CV download buttons** (per active culture) on About/Contact when `ProfileTranslation.CvFileId` is set.
- **Pagination / "load more"** on long lists; **breadcrumbs** on detail pages.

## 6. Footer
- Editable footer text (per culture); social/contact icons **Facebook, WhatsApp, Email** (links editable from admin); copyright; quick links mirror; language switcher mirror.

## 7. User Flows

- **Browse research/publications:** Home preview → "View All" → list → filter/search → detail (abstract, PDF download, metadata) → switch culture (slug-mapped; falls back to list if no translation).
- **Read a book:** Books → click cover → popup → preview PDF → Read/Download (counters increment).
- **Watch a video:** Videos → thumbnail → embedded player.
- **Explore theses:** Theses → filter by relationship/degree/year or search researcher/title.
- **Contact:** Contact → form (name, email, subject, message; honeypot + rate limit) → submit → confirmation; message lands in admin inbox + optional email.
- **Language preference:** culture from URL; switcher sets it explicitly; first-visit default from `SiteSettings.DefaultCulture` (optionally hinted by `Accept-Language`). Arabic renders RTL; **French missing content falls back to the default culture** (doc 10 §5).

## 8. Accessibility & Performance
- Semantic HTML, alt text (`MediaFile.AltText`), keyboard-navigable nav/modals/filter-tabs, ≥44px touch targets.
- `dir="rtl"` + mirrored layout for Arabic; `lang`/`dir` per culture.
- Server-rendered pages; cached immutable (GUID-named) media; lazy-load images; defer YouTube iframes; scroll-reveal animations (respecting reduced-motion).
