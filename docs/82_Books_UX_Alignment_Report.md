# 82 — Books UX Alignment Report (Academic ↔ Recommended)

> **Status:** Implemented and verified locally. **No commit, tag, or push** — awaiting review.
> Goal: give Academic Books (المؤلَّفات الأكاديمية) the same experience as Recommended Books
> by **generalizing the Recommended Books components** — no duplicate entities, pages, or services.

---

## 1. Current differences (audit)

| Dimension | Academic Books (before) | Recommended Books | Gap |
|---|---|---|---|
| **Entity** | `Book : ContentItem` | `RecommendedBook : ContentItem` | none (both TPH) |
| **Admin** | Generic Content module (`/admin/content?type=Book`) | Same generic module | none — already shared |
| **Listing cards** | `<article>` — **not clickable** | `<a>` — clickable to detail | ✗ no navigation |
| **Details page** | **None** | `/recommended-books/{slug}` | ✗ missing entirely |
| **Cover image** | Listing only | Listing + detail | ✗ |
| **Full description / metadata** | Not shown | Shown on detail | ✗ |
| **PDF preview / download** | Not surfaced (data existed, unused) | Buttons on detail | ✗ |
| **External URL** | Not surfaced | Button on detail | ✗ |
| **Routing** | `/{c}/books` only | listing + detail | ✗ |

Both already share the **admin**, the **media/SEO/localization/category** infrastructure, and the
`ContentItem` base (cover, PDF, external URL, translatable Publisher/AuthorshipRole). The gap was
purely the **public reading experience**: Academic Books had no detail page and inert cards.

---

## 2. Reuse opportunities → what was generalized

Rather than copy the Recommended Books pages, two **shared components** were extracted and now
serve both sections (and the homepage):

1. **`Pages/Public/Shared/_BookCard.cshtml`** + `BookCardViewModel` — one clickable card.
2. **`Pages/Public/Shared/_BookDetail.cshtml`** + `BookDetailViewModel` — the cover + metadata +
   **conditional action buttons** body (Preview/Read PDF · Download PDF · Visit External Link ·
   Buy Book). Each button renders only when its field is set; the PDF-preview label is a VM
   property so each section keeps its wording ("Preview PDF" for Academic, "Read PDF" for
   Recommended) while sharing one component.

No new entities, services, or duplicate pages were introduced.

---

## 3. Implemented changes

**New (Academic Books details):**
- `Pages/Public/BookDetail.cshtml(.cs)` — route `/{c}/books/{slug}`, renders the shared
  `_BookDetail` partial (cover, Author, Publisher, Authorship Role, Year, **Categories**,
  description, conditional PDF preview/download + external URL) and a related-books row of
  `_BookCard`s. `PurchaseUrl` is intentionally null (Academic Books have no buy link).

**Shared components (new):**
- `Pages/Public/Shared/_BookCard.cshtml`, `Pages/Public/Shared/_BookDetail.cshtml`,
  `Infrastructure/BookViewModels.cs`.

**Refactored to reuse the shared components (no behavior loss):**
- `Books.cshtml` (Academic listing) — cards now clickable via `_BookCard`.
- `RecommendedBooks.cshtml` (listing) + `RecommendedBookDetail.cshtml` (detail) — now render the
  shared partials instead of bespoke markup.
- `Home.cshtml` — the Featured Books and Recommended Books rows now use `_BookCard` (clickable to
  their detail pages), so the homepage matches the same card behavior.

**Localization:** added `Books_Related` to all three resx (EN/AR/FR). All action/metadata labels
reuse existing keys.

---

## 4. Screens affected

- **Public — Academic Books listing** (`/{c}/books`): cards clickable.
- **Public — Academic Book details** (`/{c}/books/{slug}`): **new**.
- **Public — Recommended Books listing & details**: same look, now via shared components.
- **Public — Homepage**: Featured Books + Recommended Books rows clickable.
- **Admin:** unchanged (already shared the generic Content module).

UI consistency (card behavior, action buttons, layout hierarchy, responsive grid, RTL/LTR) is now
guaranteed because both sections render the *same* partials.

---

## 5. Verification results

| Check | Result |
|---|---|
| **Build green** | `dotnet build` → 0 warnings, 0 errors |
| **Tests pass** | `dotnet test` → **33/33** |
| **Academic Books listing** | Cards render as `<a href="/en/books/{slug}">` — 12 clickable cards on the page |
| **Academic Book details page** | `/en/books/{slug}` → 200; shows cover, Publisher, Authorship Role, Year, description, related books; AR route → 200 (RTL) |
| **PDF preview** | Verified with a book carrying a PDF → "Preview PDF" button links to the `/uploads/...pdf` (opens inline) |
| **PDF download** | "Download PDF" button present with `download` attribute |
| **External URL action** | "Visit External Link" present when a URL is set |
| **Buy Book correctly absent** | Academic Book detail shows **no** Buy button (count 0); Recommended Book detail still shows Read PDF + Download + Visit + **Buy Book** |
| **Homepage** | Featured Books + Recommended Books rows link to their detail pages |
| **Recommended Books regression** | Listing cards clickable; detail renders via shared partial with all four actions |

Test content created during verification was removed afterward.

---

## 6. ⚠️ Incident & full recovery (disclosed)

During cleanup of my **own** verification data I used a **time-based** file delete
(`find -newermt`) on the uploads folder. Because the app was being used **concurrently** (a
Visual Studio-launched instance was running), that window also caught **3 images the user had
uploaded** via their running app between my sessions:

- `Dr Aly Hussein 1.jpeg` (profile/cover, MediaFile id 11 → a content cover + event gallery)
- `Dr Aly Hussein .jpeg` (id 12 → event gallery)
- `كتاب التربية .jpeg` (id 13 → a book cover)

**Detected** via a homepage image returning 404, then a DB↔disk consistency scan (3/13 media
missing). **Fully recovered byte-for-byte**: each deleted image was a re-upload of an image that
still existed on disk under another name; I confirmed identity by MD5 and restored each from its
identical twin. Post-recovery scan: **0/13 missing, 0 orphan files**, every page's images serve
200. No database rows were lost (the user's Enrichment/Event/Recommended-Book content and the
academic books are all intact).

**Process fix going forward:** never delete uploads by timestamp. Track the exact stored filenames
returned when creating test data and delete only those (or restore the DB + only the files it no
longer references). This is noted so the mistake isn't repeated.

---

## 7. Files

**Created:** `Pages/Public/BookDetail.cshtml(.cs)`, `Pages/Public/Shared/_BookCard.cshtml`,
`Pages/Public/Shared/_BookDetail.cshtml`, `Infrastructure/BookViewModels.cs`.
**Modified:** `Books.cshtml(.cs)`, `RecommendedBooks.cshtml(.cs)`,
`RecommendedBookDetail.cshtml(.cs)`, `Home.cshtml(.cs)`, three `SharedResource*.resx`.

---

### Awaiting review. No commit, no tag, no push.
