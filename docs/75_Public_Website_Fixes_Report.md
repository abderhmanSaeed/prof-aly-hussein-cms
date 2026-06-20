# 75 — Public Website Fixes & Enhancements Report

**Date:** 2026-06-20
**Source of truth (design):** `ProfAly.Static`. **Constraints honored:** CMS-driven, localization (AR/EN/FR), RTL/LTR, responsive — all preserved. No routes/business-logic changes. No commit/tag/push.

> Filename note: requested `73_…` is taken (`73_Homepage_Alignment_Report.md`); this is **75** (next free; 74 = homepage checkpoint).

---

## Fixes Implemented

### 1. Home hero image overlap
- Hero portrait already uses the static `.hero-portrait-wrap` (22px radius, 4/4.4 ratio, gold glow `::after`, badge). Tightened the desktop grid ratio and confirmed responsive stacking/centering (<992px) — no content collision on tablet/mobile (the glow sits at `z-index:-1`).

### 2. Biography snapshot alignment
- The homepage About `snapshot-grid` matches the static: `.9fr / 1.1fr` columns, `align-items: center`, image + text read as one balanced component; collapses to a single column <992px (image above text). Verified RTL/LTR via logical grid.

### 3. Home books — 4 cards on desktop
- New `.card-grid--4` (1 → 2 → **4** columns at 576/992px). Home query now returns up to **4** books, **featured first** (`OrderByDescending(IsFeatured).ThenBy(SortOrder)`), topping up with the next published books only if fewer than 4 are flagged Featured — so the row is always full while featured items lead and the Featured badge still shows only on featured books. Verified: **4 cards** render on desktop.

### 4. About page image (new CMS field)
- Added **`Profile.AboutImage`** (MediaFile FK, `OnDelete: SetNull`) — **additive migration `AddAboutImage`**. Independent of `Photo`, `ContactPhoto`, and `BioImage`.
- Admin Profile screen has an **"About page image"** upload (existing `IMediaUploadService`/`MediaKind.Image`). The About page renders it as `.snapshot-photo mb-4` at the top of the biography column (matching the static), with a graceful absence when unset. Verified: upload → renders on `/ar/about`.

### 5. Theses sorting — newest → oldest
- Public Theses now `OrderByDescending(PublicationYear).ThenBy(SortOrder)`. Verified years render `2025, 2024, 2024, …` (descending). **Admin:** left on manual `SortOrder` (the admin list provides up/down reordering, which operates on SortOrder — year-sort would break that UX), so newest-first is "not applicable" there per the task's wording.

### 6. Books search
- `BooksModel` gains a `q` query-string parameter; filters published books where any translation matches **Title / Description (Summary) / Author (Authors) / Publisher** via `EF.Functions.Like('%q%')` (case-insensitive; works for AR/EN/FR). A search bar (input + Search + Clear) was added to the Books page; **pagination preserves `q`** (`asp-route-q`) and existing ordering. Verified: `?q=Environment` → filtered results.

### 7. Research / Publications PDF download
- Both models now `Include(PdfFile)`. When a publication/research item has a PDF, the item shows **"View PDF"** (opens in a new tab) and **"Download PDF"** (`download` attribute) buttons, served from `/uploads/{RelativePath}` (the existing media route). Localized button text (`Pub_ViewPdf` / `Pub_DownloadPdf`), responsive `.pub-actions` row. Verified: admin PDF upload → buttons render on `/ar/publications`.

### 8. Remove static publications intro
- Removed the hardcoded lead "تسع دراسات محكَّمة في دوريات علمية مرموقة." from `/publications` (and stopped using it for the meta description — now the page title). Eyebrow + title retained; **no hardcoded replacement** (any future intro would be CMS-managed). Verified: text absent on `/ar/publications`.

---

## Search Implementation Details

- Query: `Where(b => b.Translations.Any(t => Like(Title) || Like(Summary) || Like(Authors) || Like(Publisher)))`.
- Trims input; empty/whitespace → no filter. Case-insensitive (SQLite `LIKE`). Multi-culture (matches any translation row). Pagination + ordering preserved; `q` round-trips through page links.

## PDF Download Implementation Details

- `PdfFile` (MediaFile) eager-loaded on Publications & Research. Buttons appear only when present. View = `target="_blank" rel="noopener"`; Download = `download`. Files are the existing `/uploads` static assets (public academic documents). Localized, responsive.

## About Image Implementation

- `Profile.AboutImage` + EF FK; admin upload; About-page render. CMS-managed, not hardcoded, **not** the profile image, same media architecture as `Photo`/`ContactPhoto`/`BioImage`. Migration is additive (column + index + FK).

## Files Changed (20 + 2 migration files)

Domain `Profile.cs`; `ProfileConfigurations.cs`; migration `20260620195756_AddAboutImage.cs` (+ Designer, snapshot); admin `Profile/Index.cshtml(.cs)`; public `Home.cshtml(.cs)`, `About.cshtml(.cs)`, `Books.cshtml(.cs)`, `Publications.cshtml(.cs)`, `Research.cshtml(.cs)`, `Theses.cshtml.cs`; `public.css`; `SharedResource.{resx,ar,fr}`.

## Responsive Verification

- Home featured books: 1 (mobile) → 2 (≥576) → 4 (≥992). Hero stacks/centers <992. About snapshot stacks <992 (image above text).
- Books search bar wraps on mobile; input is full-width then inline.
- PDF action buttons wrap on narrow screens (`.pub-actions` flex-wrap).
- Theses table keeps its existing responsive stacked-card behavior; new sort applies in all layouts.
- RTL (ar) / LTR (en/fr) verified across the affected pages.

## Verification

```
dotnet build → 0 errors / 0 warnings ; dotnet test → 31/31
AddAboutImage migration → additive; applied after a Database-Safety-Layer startup backup
Home → 4 book cards (featured first); Theses years descending; Books ?q= filters; publications intro removed;
Publications/Research PDF View+Download buttons render after upload; About image renders after upload.
Verified against throwaway temp DBs; real App_Data not used by tests; temp deleted; no stray processes.
```

## Remaining Issues / Notes

- **Fix 3:** with only **3 books currently flagged Featured**, the 4th card is the next book by CMS order (no Featured badge). Flagging a 4th book as Featured makes all four badged. (Chosen to guarantee "4 cards" while keeping featured first — confirm this top-up behaviour is desired.)
- **Fix 5 (admin):** admin Theses kept on manual SortOrder to preserve the up/down reorder tool; public is newest-first.
- **PDF security:** PDFs are public `/uploads` assets (intended for public download). If gated access is later required, it would need an authenticated file endpoint (out of scope here).
- This adds a 3rd profile image field (`AboutImage`) alongside `ContactPhoto`/`BioImage`, per the explicit requirement; could be consolidated later if independent images aren't needed.

**⏸ Complete (8 fixes, public layer + 1 additive migration). No commit/tag/push — awaiting your review.**
