# 16 — Static → Dynamic Migration Plan & Analysis

**Project:** Dr. Aly Hussein — Academic Portfolio & Content Management Platform
**Inputs analysed:** the complete `ProfAly.Static` website (HTML/CSS/JS) + the original `MASTER_PROMPT_Dr_Aly_Hussein_Website.md`, cross-checked against planning docs `01`–`15`.
**Purpose:** This is the "pending input" called out in `00_README.md` and `01_Project_Vision.md` §6. It produces the content inventory, the legacy→new mapping, and the gap analysis needed before Stage 1 of `15_Claude_Code_Execution_Plan.md`.
**Scope discipline:** Analysis only. No application code, no Razor Pages, no EF entities, no migrations are produced here. Recommendations are framed as decisions/tasks for the build phase.

> **Headline finding (read first).** The legacy site is **bilingual (Arabic + English only)** — there is **no French content anywhere**, even though the architecture (docs 01, 03, 10) is built around a **trilingual AR/EN/FR** model. Separately, the legacy site carries **several structured content types that have no home in the planned entity model** (Qualifications, Skills, Memberships, Stats, grouped Activities) and the **Thesis/Book/Profile entities are missing fields** that the legacy data depends on. These are "design-once" decisions per Vision principle #4 and must be settled **before** the domain model is built, or they become expensive refactors.

---

## Part A — Inventory of the Existing Static Website

### 1. Existing Pages Inventory

The static site is a flat, multi-page site (one `.html` file per section) hosted on Netlify (`aly-hussein.netlify.app`). Shared header/footer are injected by `main.js`; all content comes from `assets/js/data.js` (bilingual records) + `assets/js/i18n.js` (UI strings).

| # | File | `data-page` | Title (AR / EN) | Content rendered | Maps to CMS public page (doc 07) |
|---|---|---|---|---|---|
| 1 | `index.html` | home | الرئيسية / Home | Hero, credibility strip, stats, about snapshot, featured books, CTA band | Home |
| 2 | `about.html` | about | نبذة / About | Lecture photo, full bio, personal details, languages, skills, qualifications | About |
| 3 | `experience.html` | experience | الخبرة / Academic Experience | Career timeline, scientific societies, editorial boards | Experience |
| 4 | `publications.html` | publications | الأبحاث / **Publications** | 9 peer-reviewed journal papers; year filter + text search | Publications |
| 5 | `books.html` | books | المؤلَّفات / **Books** | 14 books as cards; text search | Books |
| 6 | `research.html` | research | المشروعات / **Research Projects** | Grouped **activities** (training, projects, consulting, conferences, curriculum dev) in accordions | Projects **/** "Activities" (see §7 mapping risk) |
| 7 | `theses.html` | theses | الرسائل / Theses | 57 supervised/examined/ongoing theses; filterable table | Theses |
| 8 | `teaching.html` | teaching | التدريس / Teaching | Undergraduate + graduate course lists (two columns) | Teaching |
| 9 | `contact.html` | contact | تواصل / Contact | Contact details, headshot card, contact form (mailto fallback) | Contact |

Supporting files: `robots.txt`, `sitemap.xml`, `netlify.toml`, `favicon.svg`, `README.md`, `MASTER_PROMPT_*.md`.

**Notable structural facts:**
- **No per-item detail/slug pages exist.** Every section is a single list/table/grid/accordion page. Slugs, detail routes, breadcrumb-to-detail, and the Book/Resource PDF popup (doc 07 §5) are **net-new** in the dynamic build.
- Two **net-new nav tabs** in the CMS plan (Videos, Enrichment, Resources) have **no legacy page or content**.
- The CV master prompt (§7) requires **"Download CV (AR/EN)" buttons** — these are specified but not implemented in the static site, and have no home in the CMS model.

### 2. Existing Reusable UI Components

Rendered from `main.js`/`style.css` (no framework components — vanilla JS string templates + Bootstrap 5 utilities):

| Component | Where | Notes for re-build |
|---|---|---|
| Sticky navbar (`.site-nav`) | global | Condenses on scroll (`is-stuck`), glassmorphism `backdrop-filter`, brand glyph "ع" (text mark, **not** an image logo) |
| Mobile offcanvas drawer (`#navDrawer`) | global | Bootstrap offcanvas; deliberately a sibling of `<nav>` (backdrop-filter containing-block bug) |
| Language toggle (`.lang-toggle`) | global | AR⇄EN only |
| **Theme toggle (`.theme-toggle`)** | global | **Light/dark mode**, persisted in `localStorage` — not in CMS scope |
| Footer (brand / quick links / contact / copyright) | global | |
| Back-to-top button | global | |
| Hero (`.hero`, portrait, badge "30+") | home | |
| Credibility chip strip (`.chip`) | home | 5 institution chips |
| Stat card with animated counter (`.stat-card`, IntersectionObserver count-up) | home | |
| About snapshot (photo + text + CTA) | home | |
| Book card (`.book-card`, **CSS-generated cover** with initial+year, no image) | home/books | |
| CTA band (`.cta-band`) | home | |
| Page hero with breadcrumb (`.page-hero`, `.crumbs`) | inner pages | |
| Panel / meta-list `<dl>` (`.panel`, `.meta-row`) | about/contact | |
| Qualification list (`.qual-list`, year + degree + institution + grade) | about | |
| Skill chips (`.skill-chip`) | about | |
| Experience timeline (`.tl-item`, period + role + org + desc) | experience | |
| Publication item (`.pub-item`, year + title + venue) | publications | |
| Theses table + filter tabs + degree/sort selects + result count | theses | Rich client-side filtering |
| Accordion (`.accordion`, Bootstrap) | research/activities | grouped activity lists |
| Course list items (`.course-item`) | teaching | |
| Contact form + contact rows + form-status | contact | mailto-only, client validation + honeypot-free |
| Empty-state, search field, select field, badges (`badge-deg`, `badge-cat`) | various | |

Doc 07 §5 already anticipates content card, book popup, video card/modal, pagination, breadcrumbs. **Gap:** the **theses filterable table** and the **activities accordion** are distinctive legacy components not enumerated in doc 07.

### 3. Existing Design System

Source of truth = `assets/css/style.css` `:root` tokens (these **differ slightly** from the master prompt; the implemented CSS values win).

| Token | Implemented value | (Master-prompt value) |
|---|---|---|
| `--primary` | `#0B5D3B` (deep Al-Azhar green) | `#1B4D3E` |
| `--primary-700` | `#084229` | `#143B30` |
| `--accent` | `#C8A45D` (brass/gold) | `#C9A24B` |
| `--bg` / `--surface` | `#F8F8F8` / `#FFFFFF` | `#FBFAF6` |
| `--ink` / `--muted` | `#1F2937` / `#5B6472` | `#14213D` / `#5B6472` |
| `--line` | `#E4E2DB` | `#E4DFD3` |
| Radius / shadow | `--radius:14px`, soft single shadow tokens | — |
| Section padding | `clamp(3.5rem, 7vw, 6.5rem)` | — |
| Nav height | `76px` | — |
| **Dark mode** | full `[data-theme="dark"]` token set | "optional/skip" |

**Typography (per-locale font swap):**
- AR headings **Amiri**, AR body **IBM Plex Sans Arabic** (fallback Cairo); `--leading: 1.95`.
- EN headings **Cormorant Garamond**, EN body **Inter**; `--leading: 1.7`.
- Loaded from Google Fonts CDN.

**Layout/RTL:** RTL-first via **CSS logical properties** (`margin-inline`, `inset-block-start`, `text-start`) — direction mirrors automatically. `prefers-reduced-motion` respected. Focus-visible rings on `--accent`. Bootstrap 5.3.3 from CDN.

> Doc 10 §4 specifies "a quality Arabic webfont" generically. **Recommendation:** pin the exact families above into the CMS theme so the dynamic site reproduces the established look.

### 4. Existing Navigation Structure

`main.js` `NAV` array — **9 items, single flat level**, no dropdowns:

```
Home · About · Experience · Publications · Books · Research(Projects) · Theses · Teaching · Contact
```
Plus header actions: **Theme toggle · Language toggle (AR/EN) · Hamburger (mobile)**. Footer mirrors the same 9 links + contact block. The same `NAV` model drives header and footer (single source).

**vs. doc 07 §2 planned nav** (13+ items): Home, About, Experience, Research, Publications, Projects, Theses, Teaching, **Videos, Enrichment, Resources**, Contact + **Search** + **AR|EN|FR**. The CMS nav adds 3 new content tabs, a search entry, and a third language — and (per doc 07) may need a "More" dropdown for width. **No search entry and no theme toggle exist in the CMS nav plan.**

### 5. Existing Content Inventory (counts & shapes)

All records in `data.js` are bilingual `{ar, en}` objects. **No `fr`.**

| Dataset | Count | Fields per record |
|---|---|---|
| `profile` | 1 | name, shortName, title, **positioning**, phone, email, location, **born**, **nationality**, **marital**, **languages** |
| `bio` | 3 paragraphs ×2 langs | rich prose (→ FullBio) |
| `stats` | 5 | value, suffix, label |
| `credibility` | 5 | institution name |
| `qualifications` | 4 | year, grade, degree, institution |
| `career` | 8 | period (free text), role, org, desc |
| `skills` | 5 | label |
| `books` | 14 | year, **featured**, **publisher**, title, **role** (authorship) |
| `publications` | 9 | year, title, **venue** (journal+issue) |
| `theses` | 57 | n, **category** (supervised/examined/ongoing), **degree** (MA/PhD), year, **researcher**, title |
| `activities` | 5 groups, ~28 lines | group name + list of one-line items |
| `teaching` | 8 UG + 8 grad = 16 | label, split by **level** (undergraduate/graduate) |
| `memberships` | 3 societies + 7 boards = 10 | label, split by type (societies/boards) |

**Total ≈ 130 content records**, each in 2 languages (and FR empty = effectively 3× seeding slots). This is a real, non-trivial seed/import effort.

### 6. Existing Images Inventory

`assets/images/` contains **4 assets only**:

| File | Used by | CMS target |
|---|---|---|
| `hero-portrait.jpeg` (420×462) | home hero, og:image, apple-touch-icon | `Profile.PhotoMediaId` |
| `about-lecture.jpeg` (≤700×560) | home snapshot, about | PageSection image / Media Library (no direct field) |
| `contact-headshot.jpeg` (500×450) | contact | PageSection / Media Library (no direct field) |
| `favicon.svg` | favicon | **No clean home** — doc 09 §1/§9 **rejects SVG** uploads (script risk) |

**Critical observations:**
- **No book covers exist** (books render CSS placeholder spines). The doc 07/08 Book popup with cover image has **no source images**.
- **No PDFs exist** (no book/paper full-texts, no CV PDF). The Book/Resource **PDF preview + Read/Download** feature (doc 07 §5, doc 09 §5) has **no data to show** at launch.
- **No logo image** — brand is the Arabic glyph "ع". `SiteSettings.LogoMediaId` will be empty; the text/glyph mark must be reproducible.

### 7. Existing Content → CMS Entity Mapping

| Legacy dataset | Natural CMS target (docs 03–05/08) | Fit | Notes / gap |
|---|---|---|---|
| `profile` (identity) | `Profile` + `ProfileTranslation` | ⚠ Partial | Missing fields — see Part B §3 |
| `bio` | `ProfileTranslation.FullBio` | ✅ Good | Rich text, AR/EN ready |
| `publications` (9 papers) | `ContentItem` type **Publication** | ⚠ | `venue` → `Journal`; no DOI/authors/PDF in data |
| `books` (14) | `ContentItem` type **Book** | ⚠ | Missing `Publisher`, authorship `Role`, `Featured` flag; no covers/PDFs |
| `theses` (57) | `ContentItem` type **Thesis** | ❌ Poor | No `researcher` field, no supervision `category`; tabular UX vs rich item — see Part B §1 |
| `activities` (grouped lines) | `ContentItem` type **Project** (+ Category) | ❌ Poor | Shape is "group → short lines", not rich items with slug/cover/detail |
| `teaching` (16 courses) | `Course` + `CourseTranslation` | ⚠ | Missing UG/Graduate **level** field |
| `career` (8) | `ExperienceEntry` | ⚠ | Free-text bilingual `period` vs structured Start/End dates |
| `qualifications` (4) | — | ❌ Missing | **No entity** |
| `skills` (5) | — | ❌ Missing | **No entity** (or PageSection list) |
| `memberships` (10) | — | ❌ Missing | **No entity** |
| `stats` (5) | — | ❌ Missing | **No entity** (homepage counters) |
| `credibility` (5) | — | ❌ Missing | **No entity** (homepage chips) |
| Home hero/CTA text | `PageSection` (`home`) | ✅ Good | |
| Contact form | `ContactMessage` + SMTP | ✅ Good (upgrade) | Legacy is mailto-only; CMS adds DB inbox |

### 8. Existing SEO Elements

Per-page in each HTML `<head>`:
- `<title>`, `<meta description>`, `<meta robots="index,follow">`, `author`, `theme-color`.
- **Canonical** → absolute `.html` URL (e.g. `…/about.html`).
- **Open Graph** (type, title, description, image, url, locale `ar_EG` + alternate `en_US`) + **Twitter card** (`summary_large_image`).
- **JSON-LD `Person`** on the homepage only (name, jobTitle, affiliation Al-Azhar, alumniOf Montpellier III + Al-Azhar, knowsLanguage ar/fr, email, telephone, address, image).
- `sitemap.xml`: **9 static URLs**, `changefreq`/`priority`, **no `hreflang`/`xhtml:link` alternates, no per-language entries**.
- `robots.txt`: `Allow: /` + sitemap reference (no `/admin` to disallow — static).
- `netlify.toml`: long-cache for `/assets/*`, `must-revalidate` for HTML, `X-Content-Type-Options: nosniff`.

**vs. doc 11 target:** the CMS adds per-culture URLs, `hreflang` + `x-default`, dynamic sitemap with alternates+lastmod, JSON-LD per type (ScholarlyArticle/Book/VideoObject/Breadcrumb/WebSite+SearchAction), `/admin` disallow, and **301 redirects from legacy URLs** (the legacy SEO equity is on `…netlify.app/*.html`).

### 9. Existing Multilingual Implementation

- **Two languages: `ar` (default, RTL) + `en` (LTR).** Stored in `localStorage("lang")`; toggled client-side; `<html lang/dir>` and per-locale font stacks switch on toggle.
- UI strings in `i18n.js` `I18N.ar` / `I18N.en`; content strings as inline `{ar,en}` objects in `data.js`; helper `L(obj)` picks the active locale with EN→AR fallback.
- **No URL-segment culture** (single URL per page; language is a client toggle) — opposite of the CMS's `/{culture}/…` SEO-indexable routing (doc 10 §2).
- **No French** UI strings and **no French** content. `knowsLanguage:["ar","fr"]` in JSON-LD is the only French trace.
- Number style is **mixed**: years/stats use Western digits; profile/period strings use Arabic-Indic digits (`٢٤ فبراير ١٩٦٦`, `٢٠١٣ – ٢٠١٨`). Master prompt §4 asked for Western digits throughout — the implementation diverged.

### 10. Required Migration Tasks (legacy → dynamic)

1. **Lock the open decisions in Part C** before building the domain model.
2. **Author a seed/import** of ~130 records (AR/EN) into the CMS model (Profile, bio, Publications×9, Books×14, Theses×57, Courses×16, Experience×8, plus the new entities for Qualifications/Skills/Memberships/Stats/Credibility/Activities).
3. **French strategy:** seed AR/EN; leave FR rows empty and rely on doc 10 §5 fallback, **or** commission FR translations (out-of-scope cost). Decide explicitly.
4. **Re-key SEO terminology** (المؤلفات/الأبحاث inversion) so each item lands on the correct `ContentType`.
5. **Build the legacy→new 301 redirect map** (all `*.html` → `/{culture}/…`), noting the domain change from `aly-hussein.netlify.app` to the production domain (doc 11 §9, doc 14 D-08).
6. **Re-create images:** import the 3 photos as media; decide favicon handling (SVG is rejected by the upload pipeline — ship it as a static app asset, not via Media Library); generate/commission book covers (or keep CSS placeholders); accept that PDF preview/download launches empty.
7. **Reproduce the design system** (tokens + per-locale fonts + dark mode if kept) in the Razor/Bootstrap theme.
8. **Re-implement client behaviours** server-side or progressively: theses table filters, publication year filter, book search, animated counters, scroll reveal, theme toggle.
9. **Map free-text `period`** strings to `ExperienceEntry.Start/EndDateUtc` (parse ranges; "present" = null end).
10. **Decide digit house style** (Western vs Arabic-Indic) and apply site-wide (doc 10 §6).

---

## Part B — Gap Analysis vs. the Architecture

### 1. Missing Entities (would force later schema refactor if not added now)

| Legacy data | Missing entity | Recommendation |
|---|---|---|
| `qualifications` (degree, year, institution, grade) | **`Qualification` + `QualificationTranslation`** | Add now. Parallels `ExperienceEntry`/`Course`. Distinct from career roles. |
| `skills` (competency chips) | **`Skill` + `SkillTranslation`** (or a typed `PageSection` list) | Lightweight; an ordered translated list. |
| `memberships` (societies + editorial boards) | **`Membership` + `MembershipTranslation`** with a `kind` (Society/Board) | Two grouped lists currently homeless. |
| `stats` (career-in-numbers counters) | **`StatItem` + `StatItemTranslation`** (value, suffix, label) | Drives homepage; structured, ordered, animated. |
| `credibility` (institution chips) | **`CredibilityLogo`/`Institution`** (name, optional logo media) | Could be a `PageSection` list, but it is structured + repeatable. |
| `activities` (grouped one-liners) | Either a **typed `Activity` + group**, or accept modelling each as a **Project `ContentItem` tagged by Category** | The Project subtype (status/role/dates/ExternalUrl) does **not** match the "group → short line" shape. Decide: lightweight `Activity` entity (recommended) vs. forcing into `ContentItem`. |

> If these are deferred to "PageSection free text", the professor loses structured editing and the established UI (timeline, chips, counters, accordion) becomes hard to manage — contradicting Vision principle #1 (self-service) and #4 (design once).

### 2. Missing Content Types

- **No new content types are required** beyond the eight planned (`Book, Publication, ResearchPaper, Thesis, Project, Resource, EnrichmentItem, Video`). Videos/Resources/Enrichment are intentionally net-new (empty at launch).
- **But the meaning of three sections must be reconciled** (see Part C §2): legacy `الأبحاث`=Publications(papers), `المؤلَّفات`=Books, `المشروعات`(research.html)=Activities — which collides with the CMS's `Research`/`Publications`/`Projects` naming.

### 3. Missing Fields on Existing Entities

| Entity | Missing field(s) | Why needed |
|---|---|---|
| **`Profile`** | `DateOfBirth`, `Nationality`, `MaritalStatus`, `Location`, `Languages`, `Positioning` (distinct from Title), `ShortName`, **CV PDF (AR/EN)** | Powers the About "Personal Details" panel, hero positioning line, brand short-name, and the master-prompt "Download CV" buttons. None exist today. |
| **`ContentItem` (Book)** | `Publisher`, authorship `Role`, `IsFeatured` | Legacy book cards show publisher + role; homepage uses an explicit "featured" flag (3 books), not just recency. |
| **`ContentItem` (Thesis)** | `ResearcherName`, supervision `Relationship`/`Category` (Supervised/Examined/Ongoing) | The 57 theses are **other people's** work the professor supervised/examined. The CMS `Supervisor` field is inverted (here the professor *is* the supervisor). Author/researcher name + relationship are mandatory facets and table columns + filters. |
| **`Course`** | `Level` (Undergraduate/Graduate/Diploma) | Teaching page splits into two columns by level. |
| **`ExperienceEntry`** | (shape) free-text `Period` vs `Start/EndDateUtc` | Optional: add a display-only `PeriodLabel` to preserve "Jul 2018 – present" / Arabic-Indic strings without lossy date parsing. |
| **`Category`** | (none) | But **no legacy categories exist** — the cross-cutting taxonomy must be curated from scratch; legacy "filters" were year/degree/supervision facets, not topical categories. |

### 4. Missing / Mismatched Admin Screens

The admin side-menu (doc 06) mirrors the CMS content types but **has no screens for the homeless legacy datasets**:

- **Missing admin screens** for: **Qualifications, Skills, Memberships, Stats ("Career in Numbers"), Credibility/Institutions, Activities** (if modelled separately).
- **Homepage admin** (doc 06 "Home") must manage hero positioning, stats, credibility chips, and the **featured-books selection** — currently only "PageSection hero" is described.
- **Theses admin** needs researcher + category fields and the public table's facets, not the generic content card screen.
- **Profile/About admin** must expose the extra Profile fields + CV PDF upload.
- **No admin control for the public theme toggle / dark mode** (if dark mode is retained as a feature).
- **No "CV document" management** anywhere.
- The generic shared CRUD screen (doc 06 §3) assumes slug + rich body + cover/PDF per item — **a poor fit for the 57 tabular theses and the ~28 one-line activities** (no slugs/detail/cover needed). A lighter list-editor variant is warranted.

### 5. Missing Database Requirements

- **New tables** for the missing entities in §1 (+ their `*Translation` tables, all keyed `(ParentId, Culture)`, with the same `CHECK (Culture IN ('ar','en','fr'))`, indexes, and cascade rules as doc 03).
- **New columns** on `Profile`, `ContentItem` (Book/Thesis fields), `Course` per §3.
- **Featured/curation flag** + homepage section ordering storage (doc 03 has `SortOrder` but no "featured" concept).
- **Legacy redirect map** storage (doc 11 §2/§9 mentions 301s but there is no `Redirect`/`UrlAlias` table in doc 03) — needed to serve `*.html` → `/{culture}/slug` 301s and future slug-change redirects.
- **FTS scope:** doc 03 §6 indexes `ContentItem` text. The legacy site's most-searched surface is the **theses table** (researcher + title) and **publications** — confirm these flow into FTS, and that the per-page facet filters (year, degree, category) are supported by indexes, not just FTS.
- **No BLOB/media gaps** — model is fine; the gap is *content* (no PDFs/covers), not schema.

### 6. Design Improvements (legacy is ahead of the plan in places)

1. **Adopt the implemented token palette + exact fonts** (Part A §3) into the CMS theme — the plan only describes them generically. Note the CSS values differ from the master prompt; the **CSS is authoritative**.
2. **Dark mode** is fully built in the legacy CSS. Decide: keep it (add a public theme toggle + persistence; minor) or drop it (out of doc-01 scope). Either way, document the decision.
3. **Preserve the distinctive components** doc 07 omits: theses filter-table, activities accordion, animated stat counters, credibility chips, CSS book-cover fallback (useful precisely because real covers are absent).
4. **Per-locale font swap + RTL logical properties** are a clean pattern to carry over verbatim.
5. **Keep `youtube-nocookie` + deferred iframes** (doc 07/11) — net-new but consistent with the legacy performance posture (long-cache assets, lazy reveal).
6. **Brand mark:** reproduce the "ع" glyph mark as the default when `SiteSettings.LogoMediaId` is empty.

### 7. Potential Migration Risks (register)

| # | Risk | Severity | Mitigation |
|---|---|---|---|
| R1 | **Trilingual goal vs bilingual content** — no French exists | **High** | Decide FR strategy (empty+fallback vs commission). Architecture supports both; the *goal* "genuinely trilingual" is otherwise unmet at launch. |
| R2 | **Section terminology inversion** (المؤلفات/الأبحاث; research.html=Activities) | **High** | Lock a naming map (Part C §2) before seeding; wrong mapping puts items under the wrong `ContentType` and wrong slugs. |
| R3 | **Thesis model mismatch** (researcher/category fields; 57 tabular rows vs rich items; 57×3 translations heavy) | **High** | Add fields (§3); use a tabular list-editor; decide whether theses need slugs/detail pages at all (legacy has none). |
| R4 | **Homeless entities** (Qualifications/Skills/Memberships/Stats/Credibility/Activities) | **High** | Add entities now (§1) — deferring violates "design once" and forces a later migration. |
| R5 | **Profile missing fields** (DOB, nationality, marital, location, languages, positioning, CV) | **Medium** | Extend Profile (§3) before Stage 1. |
| R6 | **Legacy URL breakage** (`*.html` → `/{culture}/slug`, domain change off Netlify) | **Medium** | Build complete 301 map + redirect storage; submit new sitemap (doc 14 D-08/D-09). |
| R7 | **No PDFs / no book covers exist** | **Medium** | Book popup/PDF features launch empty; generate covers or keep CSS fallback; treat PDFs as future uploads. |
| R8 | **SVG favicon vs upload security policy** (doc 09 rejects SVG) | **Low** | Ship favicon as a fixed app static asset, outside the Media Library validation path. |
| R9 | **Free-text period vs structured dates** (Arabic-Indic digits, ranges, "present") | **Low/Med** | Add `PeriodLabel` display field or parse carefully; pick a digit house style. |
| R10 | **Dark mode out of scope** but built | **Low** | Explicit keep/drop decision. |
| R11 | **Seed volume** (~130 records ×2–3 langs) under-scoped in doc 14 | **Medium** | Promote the "one-off content migration task" (doc 14 note, doc 15 Stage 6) to a first-class, sized task with this inventory as its checklist. |
| R12 | **Category taxonomy is empty in legacy** | **Low** | Curate a small topical set during seeding (doc 08 §10 guidance); don't block launch on it. |

---

## Part C — Decisions To Lock Before Stage 1 (hand-off to doc 17)

1. **French:** seed AR/EN only with FR fallback, or commission FR? *(affects "trilingual" claim, sitemap/hreflang completeness, seed effort)*
2. **Section naming map (authoritative):** confirm
   - `الأبحاث` (legacy publications.html, 9 journal papers) → CMS **Publication**
   - `المؤلَّفات` (legacy books.html, 14 books) → CMS **Book**
   - `المشروعات` (legacy research.html = grouped activities) → CMS **Project** *or* new **Activity** entity
   - reserve CMS **ResearchPaper** for future use, or merge with Publication.
3. **New entities approved?** Qualification, Skill, Membership, Stat, Credibility/Institution, Activity (Part B §1).
4. **Profile extension approved?** DOB, Nationality, Marital, Location, Languages, Positioning, ShortName, CV-PDF (AR/EN).
5. **Thesis modelling:** tabular list-editor + ResearcherName + Relationship/Category, slugs/detail pages **yes/no**.
6. **Book extension:** Publisher, Role, IsFeatured; covers (generate vs CSS fallback).
7. **Dark mode:** keep (with admin/public toggle) or drop.
8. **Digit house style:** Western vs Arabic-Indic, applied site-wide.
9. **Redirect storage:** add a `Redirect`/`UrlAlias` table to the data model.
10. **Design tokens/fonts:** adopt the implemented CSS palette + Amiri/IBM Plex Sans Arabic/Cormorant/Inter into the CMS theme.

> Once these ten are answered, the domain model (doc 15 Stage 1–2) can be built once, satisfying Vision principle #4. The detailed readiness assessment and amended build sequence are in **`17_Final_PreImplementation_Review.md`**.
