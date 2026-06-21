# 81 — Digital Resources & Events — Implementation Report

> **Status:** Implemented and verified locally. **No commit, tag, or push** — awaiting review.
> Implements the architecture approved in `76_Digital_Resources_And_Events_Architecture_Report.md`.

---

## 1. Summary

All four areas from the brief are implemented by **extending the existing `ContentItem`
TPH model** — no duplicate content infrastructure was created. The existing media,
localization, SEO, categories, featured/publish flags, admin shell, and the
generic-content / Videos UI patterns are all reused.

| Area | Strategy | Result |
|---|---|---|
| Educational Videos | Reused `Video` | Re-homed under the new **Digital Resources** dropdown; route/data/CMS untouched |
| Enrichment Materials | Reused existing `EnrichmentItem` | Admin (generic Content module) + public listing/detail with PDF preview/download + external URL |
| Recommended Books | New subtype `RecommendedBook` (+`PurchaseUrl`) | Full admin + public listing/detail with Read/Download/Visit/Buy |
| Events | New subtype `Event` + shared `ContentImage` gallery | Dedicated admin (gallery, date, location) + public listing/detail with gallery & related |
| Homepage | Extended `Home` | Latest Videos · Latest Enrichment · Recommended Books · Featured Events — each with View All |

**Verification:** build green (0 warnings/errors), 33/33 tests pass, and a full live
walkthrough (login → create → public render → uploads) confirmed every path. Details in §6.

---

## 2. Database changes (one additive migration)

Migration `20260621180422_AddDigitalResourcesAndEvents` — **purely additive, no data loss:**

- `ContentItem.PurchaseUrl` — new nullable column (RecommendedBook only).
- `ContentItemTranslation.Location` — new nullable column (Event location, translatable).
- New table **`ContentImage`** `(Id, ContentItemId, MediaFileId, SortOrder, Caption)` with
  FKs to `ContentItem` (cascade) and `MediaFile` (restrict) + `(ContentItemId, SortOrder)` index.
- Enum `ContentType` gained `RecommendedBook = 9`, `Event = 10`; two `HasValue<>()`
  discriminator mappings registered.

Migrations apply automatically at startup (`DatabaseInitializer.MigrateAsync`). Verified the
migration applies cleanly to the existing dev DB (existing rows untouched) and re-applies on a
fresh restore. A pre-change DB backup was taken and the dev DB was returned to a clean,
test-data-free state afterwards.

---

## 3. Domain & infrastructure

**New entities**
- `Domain/Entities/Content/RecommendedBook.cs` — subtype; adds `PurchaseUrl`. Author/Publisher
  reuse the translatable `Authors`/`Publisher` fields.
- `Domain/Entities/Content/Event.cs` — subtype; reuses base `EventDateUtc` (date) and
  `ExternalUrl` (external link); location is translatable; gallery via shared `Images`.
- `Domain/Entities/Content/ContentImage.cs` — generic gallery row (reusable, not Event-specific).

**Modified**
- `ContentType.cs` (+2 values), `ContentItem.cs` (+`Images` collection),
  `ContentItemTranslation.cs` (+`Location`), `FieldLengths.cs` (+`PurchaseUrl`, `EventLocation`,
  `ImageCaption`).
- `ContentConfigurations.cs` — `RecommendedBookConfiguration`, `ContentImageConfiguration`,
  `Images` relationship, `Location` max-length.
- `AppDbContext.cs` — `ContentImage` DbSet + discriminator mappings.

---

## 4. Admin

**Reused the generic Content module** (`Areas/Admin/Pages/Content`) for **Enrichment Materials**
and **Recommended Books**:
- `IndexModel.Allowed` extended with `EnrichmentItem`, `RecommendedBook`; title switches updated.
- `UpsertModel` — `NewOfType` cases, `PurchaseUrl` load/save, per-type translatable-field routing
  (Book→Publisher/Role, RecommendedBook→Author/Publisher, Paper→Journal/Authors, Enrichment→none).
- `Upsert.cshtml` — conditional Purchase URL (RecommendedBook), Author/Publisher inputs, an
  Enrichment source hint, and Publication-Year hidden for Enrichment.
- Inherits cover/PDF upload, categories, featured, publish, reorder, SEO, AR/EN/FR unchanged.

**Dedicated Events module** (`Areas/Admin/Pages/Events/{Index,Upsert}`) — needed for gallery
management the generic form can't host. List/Create/Edit/Delete/Reorder/Publish/Feature,
categories, AR/EN/FR, cover, **event date**, **translatable location**, **external link**, and a
**multi-image gallery** (add multiple, remove via checkbox; new images append in order).

**Sidebar** (`_Sidebar.cshtml`) — new **Digital Resources** group: Videos · Enrichment Materials ·
Recommended Books · Events (replacing the old disabled "Enrichment"/"Resources" placeholders).

---

## 5. Public website

**Navigation** (`_PublicLayout.cshtml`) — flat nav became a one-level structure: a **Digital
Resources** dropdown (Educational Videos → existing `/videos`, Enrichment Materials, Recommended
Books) plus a new top-level **Events**. Accessible (`aria-haspopup`/`aria-expanded`, hover +
focus-within + click toggle, Escape to close), RTL-mirrored, and collapses inline in the mobile
drawer. Footer quick-links flatten the dropdown children. CSS in `public.css`, JS in `public.js`.

**New pages** (cloned from the Videos listing/detail pattern, culture-routed):
- `/{c}/enrichment` + `/{c}/enrichment/{slug}` — Preview PDF / Download PDF / Visit Resource
  (conditional on which fields are set).
- `/{c}/recommended-books` + `/{c}/recommended-books/{slug}` — Read PDF / Download PDF /
  Visit External Link / Buy Book (conditional).
- `/{c}/events` + `/{c}/events/{slug}` — date, location, description, **gallery** (responsive grid,
  click-to-open), related events, external event link.

**Homepage** (`Home.cshtml(.cs)`) — four CMS-driven sections, each with a localized **View All**
button: Latest Videos, Latest Enrichment Materials, Recommended Books, Upcoming/Featured Events.
Each renders only when it has published content.

**PDFs** are served from the existing `/uploads/{RelativePath}` static mount (Preview = open
inline; Download = `download` attribute with original filename).

---

## 6. Verification

| Requirement | Result |
|---|---|
| **Build green** | `dotnet build` → 0 warnings, 0 errors |
| **Tests pass** | `dotnet test` → **33/33** (added domain tests for the new discriminators, gallery default, and RecommendedBook validity) |
| **Navigation** | Dropdown + Events render; child links resolve to `/videos`, `/enrichment`, `/recommended-books`; "Educational Videos" label shown |
| **Localization** | Dropdown & labels verified in **EN / AR (RTL) / FR**; ~45 keys added to all three resx files |
| **Admin pages** | Logged in as Super Admin → all new admin pages return 200; Enrichment hides Publication-Year + shows source hint; RecommendedBook shows Purchase URL + Author; Events shows date/location/gallery |
| **Public pages** | All listing + detail routes return 200 in en/ar/fr |
| **Create flow** | Created a RecommendedBook, an Event (date+location), an Event with a 2-image gallery, and an Enrichment item with cover + PDF — all persisted and rendered, with correct conditional action buttons |
| **Uploads** | Cover image, PDF, and gallery images written under `/uploads/2026/06/…` and served (`application/pdf`, `image/png`); gallery (`ContentImage`) persisted and rendered |
| **Homepage** | Recommended Books + Featured Events sections appeared once content existed |

Live test data and orphan uploads were removed afterwards; the dev DB is clean (schema migrated,
no sample rows).

---

## 7. Files

**Created (domain):** `RecommendedBook.cs`, `Event.cs`, `ContentImage.cs`,
migration `20260621180422_AddDigitalResourcesAndEvents.*`.
**Created (admin):** `Areas/Admin/Pages/Events/{Index,Upsert}.cshtml(.cs)`.
**Created (public):** `Enrichment`, `EnrichmentDetail`, `RecommendedBooks`,
`RecommendedBookDetail`, `Events`, `EventDetail` (`.cshtml` + `.cshtml.cs`).
**Modified:** `ContentType.cs`, `ContentItem.cs`, `ContentItemTranslation.cs`, `FieldLengths.cs`,
`ContentConfigurations.cs`, `AppDbContext.cs`, `Content/Index.cshtml.cs`,
`Content/Upsert.cshtml(.cs)`, `_Sidebar.cshtml`, `_PublicLayout.cshtml`, `public.css`,
`public.js`, `Home.cshtml(.cs)`, three `SharedResource*.resx`, `DomainModelTests.cs`.

---

## 8. Notes & decisions (as built)

- **`Event` vs `EventItem`** — used `Event` for the content subtype; the existing analytics
  `ContentEvent` is untouched (no collision in practice).
- **Enrichment/Recommended Books admin** — extended the generic Content module (recommended path),
  not dedicated pages — maximises reuse.
- **Location** — implemented as a translatable column on `ContentItemTranslation` per the AR/EN/FR
  mandate.
- **Gallery** — implemented as generic `ContentImage` (reusable by any content type), not
  Event-scoped.
- **Doc numbering** — the architecture report kept the requested `76_` filename despite the
  collision; this report uses the next free number, **`81_`**.

### Not done (out of scope / optional, flagged in doc 76)
- Routing PDF downloads through a handler that increments `DownloadCount` / writes a
  `ContentEvent` — left as the existing direct `/uploads` link (counters exist but aren't wired for
  these new actions). Recommend as a small follow-up if download analytics are wanted.

---

### Awaiting review. No commit, no tag, no push.
