# 76 — Digital Resources & Events — Architecture Report (Pre‑Implementation)

> **Status:** Proposal for review. **No code has been written.** Per the brief, this
> document explains the recommended architecture and waits for approval before
> implementation.
>
> **Filename note:** `76_` is already used by `76_Public_Fixes_Checkpoint_Report.md`
> and the docs sequence currently ends at `80_`. This file is named exactly as the
> deliverable requested (`76_Digital_Resources_And_Events_Architecture_Report.md`).
> I recommend renumbering it to **`81_…`** on approval to keep the sequence clean.
> Flagging rather than silently renaming.

---

## 0. TL;DR — The Decision

**Extend the existing `ContentItem` Table‑Per‑Hierarchy (TPH) model. Do not create
parallel/duplicate entity stacks.**

The codebase already contains most of what this brief asks for:

| Brief item | Already exists? | Action |
|---|---|---|
| Educational Videos | ✅ `Video : ContentItem` (`ContentType.Video`), full admin + public pages | **Re‑navigate only.** No data/route/CMS changes. |
| Enrichment Materials | ✅ `EnrichmentItem : ContentItem` (`ContentType.EnrichmentItem`) — entity, EF config, discriminator all present; **no UI yet** | Build admin + public UI on the **existing** entity. |
| Recommended Books | ⚠️ Partial — `Book : ContentItem` exists but is the *Academic Books* type. Brief says Recommended Books are **explicitly different**. | Add **one new subtype** `RecommendedBook` + one scalar (`PurchaseUrl`). Reuse everything else. |
| Events | ❌ New domain concept | Add **one new subtype** `Event : ContentItem` + a small shared **gallery** join table. |

Net new persistent structures: **2 discriminator values, 2 nullable columns, 1 join
table.** Everything else (media, localization, SEO, categories, featuring, ordering,
publish flags, analytics events, admin shell, public layout) is **reused as‑is**.

---

## 1. How the Current Architecture Works (the parts we will reuse)

### 1.1 Content model — TPH
`src/ProfAly.CMS.Domain/Entities/Content/ContentItem.cs` is an abstract base mapped
Table‑Per‑Hierarchy. One physical table (`ContentItem`) holds every content type,
distinguished by a string discriminator column `ContentType`
(`AppDbContext.OnModelCreating` → `HasDiscriminator(e => e.ContentType)`).

The **base already carries** every shared field this brief needs:

```
CoverImageId / CoverImage      → cover image (FK to MediaFile)
PdfFileId    / PdfFile          → uploaded PDF  (FK to MediaFile)
ExternalUrl                     → external link / "Visit Resource"
EventDateUtc                    → date (used by Project/Thesis today; perfect for Events)
IsPublished                     → Publish / Unpublish
IsFeatured                      → Featured flag (homepage)
SortOrder                       → Reorder
ViewCount / DownloadCount       → analytics counters
Translations  → ICollection<ContentItemTranslation>   (AR/EN/FR text + SEO)
Categories    → ICollection<ContentItemCategory>       (shared taxonomy)
Events        → ICollection<ContentEvent>              (view/download/play tracking)
```

Existing subtypes: `Book`, `Publication`, `ResearchPaper`, `Thesis`, `Project`,
`Resource`, `EnrichmentItem`, `Video` (enum `ContentType` 1‑8).

### 1.2 Translations + SEO — one row per culture
`ContentItemTranslation` holds `Title`, `Slug`, `Summary`, `Body`, the type‑specific
translatable fields (`Journal`, `Authors`, `Publisher`, `AuthorshipRole`,
`ResearcherName`), **and** the SEO triplet (`MetaTitle`, `MetaDescription`,
`MetaKeywords`). Unique indexes: `(ContentItemId, Culture)` and `(Culture, Slug)`.
→ **SEO and AR/EN/FR are already first‑class for every content type.** New types get
them for free.

> ⚠️ **Slug constraint to respect:** `(Culture, Slug)` is globally unique across *all*
> content types (one index can't span the TPH discriminator). The slug helper in
> `Content/Upsert` already de‑duplicates against the whole table — new modules must use
> the same `EnsureUniqueSlugAsync` pattern. (E.g. an Event and a Book can't both be
> `en/innovation`.)

### 1.3 Media — `MediaFile` + `IMediaUploadService`
`IMediaUploadService.UploadAsync(IFormFile, MediaKind)` validates, stores under the
uploads root, and returns a `MediaFile` row. Cover images (`MediaKind.Image`) and PDFs
(`MediaKind.Pdf`) already flow through this in `Content/Upsert.cshtml.cs`. Files are
served at `/uploads/{RelativePath}`. **Gallery is the only gap** — `MediaFile` models a
single file; there is no many‑images‑per‑item relation anywhere (Profile uses discrete
single‑image FKs).

### 1.4 Categories — shared M:N
`Category` / `CategoryTranslation` / `ContentItemCategory` (composite‑key join). Already
many‑to‑many with any `ContentItem`. New types reuse it with zero schema change.

### 1.5 Admin shell
`Areas/Admin/Pages/Shared/_Sidebar.cshtml` defines grouped nav from a C# array. Two
reusable admin UI patterns already exist:
- **Generic content pages** `Areas/Admin/Pages/Content/{Index,Upsert}` — driven by a
  `?type=` route, gated by `IndexModel.Allowed[]`. Today handles Book / Publication /
  ResearchPaper with cover + PDF upload, categories, AR/EN/FR translations, SEO,
  publish, feature, reorder. **This is the template for Enrichment & Recommended Books.**
- **Dedicated module pages** `Areas/Admin/Pages/Videos/{Index,Upsert}` — a self‑contained
  CRUD for one type. **This is the template for Events** (which needs gallery UI the
  generic page doesn't have).

The sidebar already lists `Enrichment` and `Resources` as disabled "soon" placeholders —
the intent was always to light these up.

### 1.6 Public site
`Pages/Public/*` Razor pages, culture‑routed `@page "/{culture:regex(^(ar|en|fr)$)}/…"`.
Listing + detail pattern already proven by `Videos.cshtml` + `VideoDetail.cshtml`
(`/{c}/videos` and `/{c}/videos/{slug}`). Layout `_PublicLayout.cshtml` builds the nav
from a flat `(Key, Page)[]` array and is **Bootstrap 5 + RTL‑aware**
(`bootstrap.rtl.min.css` when `ar`).

---

## 2. Recommended Architecture (per part)

### Part 1 — Header navigation: "Digital Resources" dropdown + "Events"
- Convert the flat nav array in `_PublicLayout.cshtml` into a small structure that
  supports **one level of children**. Render "Digital Resources" as a dropdown
  (Educational Videos → existing `/{c}/videos`, Enrichment Materials, Recommended Books)
  and add a top‑level **Events** item.
- The current nav is **custom CSS**, not the stock Bootstrap navbar, so the dropdown gets
  a small amount of CSS + the existing mobile burger behaviour in `public.js`. Must be
  **keyboard‑accessible** (`aria-expanded`, `aria-haspopup`, arrow/escape) and mirror in
  **RTL**. Footer "Quick Links" can stay flat or flatten the children inline.
- **Educational Videos keeps its existing route, data, and CMS** — only its menu location
  changes. No redirect needed.

### Part 2 — Enrichment Materials  → reuse `EnrichmentItem` (no new entity)
The entity, EF config (`EnrichmentItemConfiguration`), and discriminator
(`ContentType.EnrichmentItem`) **already exist**. We only build UI:
- **Admin:** add `EnrichmentItem` to the generic `Content` module's `Allowed[]` + the
  `TryResolveType`/`NewOfType`/title switches, OR clone Videos‑style dedicated pages.
  **Recommendation: extend the generic `Content` pages** (least code, full reuse of
  media/category/SEO/translation plumbing).
- **Fields** all map to existing columns: Title/Short Description → `Title`/`Summary`;
  Full Description → `Body`; Cover → `CoverImage`; Sort Order → `SortOrder`;
  Is Featured/Published → existing flags; SEO → translation SEO fields.
- **Content source rule (PDF only / URL only / both):** both `PdfFile` and `ExternalUrl`
  already live on the base and are independently nullable — the rule is purely
  validation + UI (require *at least one* of the two). No schema work.
- **Public:** new `Enrichment.cshtml` listing + `EnrichmentDetail.cshtml`
  (`/{c}/enrichment`, `/{c}/enrichment/{slug}`) cloned from the Videos pages. Detail
  renders conditionally: PDF present → **Preview** (`/uploads/…` inline) + **Download**;
  URL present → **Visit Resource**; both → show all.

### Part 3 — Recommended Books  → new subtype `RecommendedBook`
Distinct from academic `Book` (the brief is explicit). Minimal new type:
```csharp
public class RecommendedBook : ContentItem {
    public RecommendedBook() : base(ContentType.RecommendedBook) { }
    public string? PurchaseUrl { get; set; }   // only genuinely-new field
}
```
- **Reused fields:** Title, Description→`Body`/`Summary`, Cover→`CoverImage`,
  PDF→`PdfFile`, External URL→`ExternalUrl`, Featured/Published/Sort/Categories/SEO.
  **Author** → `ContentItemTranslation.Authors` (exists), **Publisher** →
  `ContentItemTranslation.Publisher` (exists) — both already translatable.
- **New:** `PurchaseUrl` scalar (nullable) → "Buy Book" action when present.
- **Admin:** same generic `Content` module extension as Enrichment, plus expose
  Author/Publisher/PurchaseUrl inputs for this type.
- **Public:** `RecommendedBooks.cshtml` + detail (`/{c}/recommended-books[/{slug}]`):
  View Details, Read PDF, Download PDF, Visit External Link, Buy Book (conditional).

### Part 4 — Events  → new subtype `Event` + shared gallery
Events need a **date**, a **location**, and a **gallery** — the first two are nearly
covered, the third is the one new shared structure.
```csharp
public class Event : ContentItem {       // ContentType.Event
    public Event() : base(ContentType.Event) { }
    // EventDateUtc (base) = Event Date
    // ExternalUrl  (base) = External Event Link
}
```
- **Event Date** → base `EventDateUtc` (already used by Project/Thesis). Drives
  "Upcoming / Featured" ordering on the homepage.
- **Event Location** → add `Location` (nullable) to `ContentItemTranslation` so it is
  **translatable** (AR/EN/FR place names), consistent with how `Journal`/`Publisher`
  already live there. (Alternative: a non‑translatable scalar on the subtype — rejected
  because the brief mandates AR/EN/FR.)
- **Gallery Images** → **new shared join entity** `ContentImage`
  `(Id, ContentItemId, MediaFileId, SortOrder, Caption?)`. Modeled generically (not
  `EventImage`) so any future content type can have a gallery. Cover image stays the
  existing single `CoverImageId`.
- **Admin:** **dedicated** `Areas/Admin/Pages/Events/{Index,Upsert}` (Videos‑style),
  because gallery management (multi‑upload, reorder, remove) doesn't fit the generic
  single‑PDF/single‑cover form.
- **Public:** `Events.cshtml` listing + `EventDetail.cshtml`
  (`/{c}/events`, `/{c}/events/{slug}`) showing date, location, description, gallery
  (Bootstrap carousel/grid + lightbox), and related/external links.

> **Naming caution:** `ContentEvent` already exists as the **analytics** event
> (view/download/play). The new calendar concept is the `Event` *subtype*. To avoid
> confusion in code, we keep `ContentEvent` for analytics and use `Event` for the
> content type (or `EventItem` if we want extra distance — decision point for review).

### Homepage integration
Extend `Home.cshtml.cs` (which already loads `FeaturedBooks`) with four CMS‑driven
sections, each with a **View All** link:
- **Latest Videos** — `OfType<Video>().Where(IsPublished).OrderBy(SortOrder)`.
- **Latest Enrichment Materials** — `OfType<EnrichmentItem>()…`.
- **Recommended Books** — `OfType<RecommendedBook>()` (Featured‑first, like books today).
- **Upcoming / Featured Events** — `OfType<Event>()` ordered by `EventDateUtc`
  (upcoming) with Featured lead.

### Admin dashboard integration
Restructure the **"Content — Collections"** sidebar group, or add a new
**"Digital Resources"** group, containing: Videos · Enrichment Materials ·
Recommended Books · Events. Replaces the current disabled `Enrichment`/`Resources`
placeholders.

---

## 3. Entity Strategy — Reuse vs. New (explicit)

| Concept | Strategy | Discriminator | New columns |
|---|---|---|---|
| Educational Videos | **Reuse** `Video` | `Video` (8) | — |
| Enrichment Materials | **Reuse** `EnrichmentItem` | `EnrichmentItem` (7) | — |
| Recommended Books | **New subtype** `RecommendedBook` | `RecommendedBook` (9, new) | `PurchaseUrl` (nullable) on Content table |
| Events | **New subtype** `Event` | `Event` (10, new) | `Location` (nullable) on `ContentItemTranslation`; `ContentImage` join table |

**Why TPH and not new tables / a new module:**
- Media, localization, SEO, categories, featuring, ordering, publish, analytics, and the
  two admin/public UI templates are **already wired to `ContentItem`**. New top‑level
  entities would duplicate all of it (the brief explicitly says *avoid duplication, reuse
  shared content infrastructure*).
- TPH columns are nullable per‑subtype, so adding `PurchaseUrl` costs one sparse nullable
  column — cheaper and simpler than a side table.
- The enum/discriminator pattern already scaled to 8 types; 10 is unremarkable.

**The one place we add genuinely new infrastructure** is the gallery (`ContentImage`),
because no many‑images relation exists yet. Built generically so it's reusable.

---

## 4. Database Impact

All changes are **additive and nullable → no data migration, no risk to existing rows.**

1. `enum ContentType`: add `RecommendedBook = 9`, `Event = 10`.
2. `AppDbContext.OnModelCreating`: add two `.HasValue<…>()` discriminator mappings.
3. **Content table:** new nullable `PurchaseUrl` column (RecommendedBook only; null
   elsewhere). New `RecommendedBookConfiguration` / `EventConfiguration`.
4. **`ContentItemTranslation`:** new nullable `Location` column (Events only).
5. **New table `ContentImage`** + FKs to `ContentItem` (cascade) and `MediaFile`
   (set‑null/restrict) + `(ContentItemId, SortOrder)` index.
6. One EF migration. Existing Videos/Enrichment rows are untouched (their schema already
   exists). New `FieldLengths` constants for `PurchaseUrl`, `Location`, `Caption`.

**Safety:** consistent with the project's backup/migration posture
(docs 12/51/52). The migration is purely additive; existing content and slugs are
preserved.

---

## 5. Admin Impact

- **Sidebar:** one grouped edit in `_Sidebar.cshtml`; remove the `Enrichment`/`Resources`
  "soon" placeholders.
- **Enrichment + Recommended Books:** extend the generic `Content/{Index,Upsert}`
  (`Allowed[]`, type switches, type‑specific fields + the "PDF/URL — at least one"
  validation, and Author/Publisher/PurchaseUrl for Recommended Books). No new media or
  translation plumbing.
- **Events:** new dedicated `Events/{Index,Upsert}` pages (List/Create/Edit/Delete/
  Reorder/Publish/Feature/Categories/AR‑EN‑FR) **plus** gallery multi‑upload/reorder/
  remove — the only net‑new admin UI surface.
- **Reused unchanged:** `IMediaUploadService`, category picker, slug generation, SEO
  inputs, the `SortOrder` move handlers, publish/feature toggles.

---

## 6. Public Website Impact

- **Navigation:** dropdown + Events top‑level in `_PublicLayout.cshtml` (+ minor
  `public.css`/`public.js`, RTL + a11y).
- **New pages (clone Videos/VideoDetail):**
  `Enrichment` + `EnrichmentDetail`, `RecommendedBooks` + `RecommendedBookDetail`,
  `Events` + `EventDetail`. Culture‑routed; conditional action buttons
  (Preview/Download/Visit/Buy) driven by which fields are populated.
- **Homepage:** four new CMS‑driven sections with View‑All, extending `Home.cshtml(.cs)`.
- **PDF actions:** files already serve from `/uploads/{RelativePath}` (Preview = inline,
  Download = `download` attr). Optional: route downloads through a handler that bumps
  `DownloadCount` / writes a `ContentEvent` (the counters and `ContentEventType.Download`
  already exist) — recommend as a small enhancement, not a blocker.
- **Reused unchanged:** `_PublicLayout`, Bootstrap 5 (incl. `.rtl`), the listing/detail/
  pagination/search pattern, SEO meta + hreflang emission.

---

## 7. Risks / Decisions for Your Review

1. **`Event` vs `EventItem` naming** — to avoid clashing with the analytics `ContentEvent`.
   *Recommend `Event` (clear in context); open to `EventItem`.*
2. **Enrichment/Recommended Books admin** — extend the generic `Content` module
   (recommended, least code) **vs.** dedicated pages (more isolated). *Recommend extend.*
3. **Event location** translatable (`ContentItemTranslation.Location`) **vs.** scalar.
   *Recommend translatable per the AR/EN/FR mandate.*
4. **Gallery** as generic `ContentImage` (reusable) **vs.** `EventImage` (scoped).
   *Recommend generic.*
5. **Nav information architecture** — Videos appears under "Digital Resources"; confirm it
   should be **removed** from any standalone top‑level slot (no duplicate entry).
6. **Doc renumbering** — adopt `81_…` (see top note)?

---

## 8. Proposed Implementation Order (after approval)

1. Domain + enum + EF config + `ContentImage` + migration (additive).
2. Admin: sidebar regroup → generic‑page extension (Enrichment, Recommended Books) →
   dedicated Events pages (incl. gallery).
3. Public: nav dropdown → Enrichment → Recommended Books → Events pages.
4. Homepage sections + View‑All.
5. Localization keys (AR/EN/FR) across all three resx files; RTL/a11y pass.
6. Verification + screenshots; checkpoint report.

---

### Awaiting your review — no implementation, no commit, no tag, no push.
