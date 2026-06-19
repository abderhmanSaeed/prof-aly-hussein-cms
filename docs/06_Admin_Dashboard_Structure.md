# 06 — Admin Dashboard Structure (FINAL)

**Status:** **FINAL — single source of truth (v2.0).** The admin lives in a dedicated area (`/admin`), gated by a single authorization policy. It mirrors the public site section-for-section so the professor's mental model is "edit what I see." Every piece of content on the public site — including all profile-page data — is managed here (per the approved decision: *fully dynamic, nothing hard-coded*).

> **v2.0 additions:** dedicated screens for `Qualifications`, `Skills`, `Memberships`, `Stats`, `Credibility`, `Activities` (groups + items); expanded `Home`, `About/Profile`, `Teaching`; new `Appearance/Theme` and `Redirects` screens; trilingual (AR/EN/**FR**) tabs everywhere with a per-language completeness indicator.

---

## 1. Side Menu Structure

```
Dashboard (overview + statistics)
─────────────────────────────────
CONTENT — PROFILE & PAGES
  Home            (hero, stats selection, credibility, featured selection, section order)
  About / Profile (photo, names, title, positioning, personal details, bio, CV per language)
  Qualifications
  Skills
  Experience      (career timeline)
  Memberships     (Societies | Editorial Boards)
  Activities      (Activity Groups + Activities)
  Teaching        (Courses — Undergraduate | Graduate)
─────────────────────────────────
CONTENT — COLLECTIONS  (shared ContentItem CRUD, filtered by type)
  Research
  Publications
  Books
  Theses
  Projects
  Videos
  Resources
  Enrichment Information
─────────────────────────────────
ORGANIZATION
  Categories
  Media Library
─────────────────────────────────
SITE
  Header
  Footer
  Profile & Contact Info
  Appearance / Theme   (default theme Light/Dark, colors)
  SEO (per page)
  Redirects (URL aliases / 301)
─────────────────────────────────
COMMUNICATION
  Contact Messages   [unread badge]
─────────────────────────────────
SYSTEM
  Statistics
  Backup & Restore
  Account & Security
  Language (admin UI culture toggle)
```

The **Collections** sections (Research, Publications, Books, etc.) are all backed by the same `ContentItem` model filtered by `ContentType`, sharing one CRUD pattern with type-specific fields shown/hidden contextually. The **Profile & Pages** sections are backed by the dedicated small entities (doc 05 §5–§13) and use a lighter **list-editor** pattern (no slug/cover/body).

## 2. Admin Pages

| Page | Purpose | Backing entity |
|---|---|---|
| **Dashboard** | KPIs (totals per type, unread messages, recent activity), stats snapshot | (read-only) |
| **Home** | Hero `PageSection`; pick/order which `StatItem`s, `Credibility` chips, and **featured Books** appear; section ordering | PageSection, StatItem, Credibility, ContentItem(IsFeatured) |
| **About / Profile** | Photo, FullName, ShortName, Title, Positioning, ShortBio, FullBio, DateOfBirth, Nationality, MaritalStatus, Location, Languages, Email, Phone, **CV upload per language** | Profile + ProfileTranslation |
| **Qualifications** | List + CRUD (Year, Degree, Institution, Grade) | Qualification |
| **Skills** | List + CRUD (Name) | Skill |
| **Experience** | List + CRUD (Start/End dates, PeriodLabel, Role, Organization, Description) | ExperienceEntry |
| **Memberships** | List + CRUD; grouped by Kind (Society/Board) | Membership |
| **Activities** | Manage Activity Groups + their Activities (ordered) | ActivityGroup + Activity |
| **Teaching** | List + CRUD `Course`; **Level** = Undergraduate/Graduate | Course |
| **Research / Publications / Books / Theses / Projects / Videos / Resources / Enrichment** | List + CRUD `ContentItem` filtered by type (shared screen) | ContentItem |
| **Categories** | List + CRUD (trilingual names/slugs) | Category |
| **Media Library** | Browse/upload/delete `MediaFile`; usage display | MediaFile |
| **Header** | Logo, navigation labels, language switcher options | SiteSettings + i18n |
| **Footer** | Footer text, social links (Facebook/WhatsApp/Email) | SiteSettings(+Translation) |
| **Profile & Contact Info** | Contact email, phone, public contact details | Profile / SiteSettings |
| **Appearance / Theme** | Default theme (Light/Dark), primary/secondary colors | SiteSettings |
| **SEO** | Per-page `PageSeo` (and defaults) | PageSeo |
| **Redirects** | Manage 301/302 URL aliases (incl. legacy `*.html`) | Redirect |
| **Contact Messages** | Inbox: read/unread, view, delete | ContactMessage |
| **Statistics** | Views, downloads, plays, top content, time ranges | ContentEvent/PageView |
| **Backup & Restore** | Export a backup archive; restore from one | (service) |
| **Account & Security** | Change password, sessions | Identity |

## 3. CRUD Screen Pattern — Collections (ContentItem)

**List view**
- Table/cards: cover thumbnail, title (current admin culture), type-specific badges, published status, **featured flag**, sort order, view/download counts, actions (Edit/Delete/Toggle-publish/Toggle-feature).
- Filters: published/draft, category, **type-specific facets** (Theses: RelationshipType + DegreeLevel + Year), free-text search.
- Drag-to-reorder or numeric `SortOrder`.

**Create / Edit view**
- **Language tabs (AR / EN / FR)** for translatable fields (Title, Slug, Summary, Body, SEO meta, and the translatable type fields — Journal/Authors, Publisher/AuthorshipRole, ResearcherName) with a per-language "missing translation" indicator (**FR included**).
- **Shared fields:** cover image picker, PDF picker (document types), YouTube ID (Video), categories multi-select, publish toggle, **featured toggle**, sort order.
- **Type-specific fields** shown contextually:
  - Publication/Research: Journal, Authors, DOI, Year.
  - Book: Publisher, AuthorshipRole, Year, Featured.
  - Thesis: ResearcherName, **RelationshipType (Supervised/Examined/Ongoing)**, DegreeLevel (Master/PhD), Year.
  - Project: ProjectStatus, Role, EventDate, ExternalUrl.
  - Resource/Enrichment: ResourceType, ExternalUrl or PDF.
- **Slug** auto-suggests from Title, editable, validated live; changing a published slug offers a `Redirect`.
- **Save as draft** vs **Publish** (publish blocked until the default-culture translation is complete).

## 4. List-Editor Pattern — Profile & Pages (small entities)

For `Qualification`, `Skill`, `Membership`, `StatItem`, `Credibility`, `Activity`/`ActivityGroup`, `Course`, `ExperienceEntry`:

- **Inline/compact rows** with AR/EN/FR mini-tabs (or three columns) for the few translatable fields — no slug, no rich body, no cover (except `Credibility` optional logo).
- **Drag-to-reorder** (`SortOrder`).
- Grouping where relevant (Memberships by Kind; Activities by Group; Courses by Level).
- The same per-language completeness indicator.

## 5. Videos / Books / Resources / Enrichment specifics
- **Videos:** add by YouTube URL or ID (system extracts/validates; never uploads video); optional custom thumbnail; title/description/category/sort.
- **Books / Resources / Enrichment:** cover + optional PDF + summary + body; drives the public popup (cover, summary, preview, Read/Download). Resource/Enrichment may be link-only (`ExternalUrl`) or file-backed.

## 6. Home & Featured management (specifics)
- The **Home** screen curates the homepage: edit hero text; choose which `StatItem`s and `Credibility` chips appear and their order; select **featured Books** (`IsFeatured`); set section order. This replaces the static site's hard-coded homepage.

## 7. Permissions
- One principal: the **Super Admin**. Authorization policy **`RequireSuperAdmin`** protects the entire `/admin` area at the area level.
- Anonymous → public only; `/admin` redirects to login.
- Login via ASP.NET Core Identity (cookie auth) with lockout + strong password policy.
- Create/modify timestamps provide a basic audit trail; optional lightweight activity log feeds the dashboard "recent activity."
- Policy-based authorization keeps future roles an additive change (out of scope now).

## 8. Admin UX Notes
- Bootstrap 5; admin UI may stay LTR with per-field direction on the Arabic tab.
- **Trilingual tabs everywhere** translatable text exists; FR is flagged-but-not-forced.
- Responsive enough to edit from a tablet; consistent toasts/validation summaries; destructive actions confirmed.
- Media pickers open the Media Library (upload-or-select); SVG/exe rejected.
