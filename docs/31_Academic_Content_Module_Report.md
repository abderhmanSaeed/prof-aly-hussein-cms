# 31 — Academic Content Module Report (Stage 6)

**Phase:** Implementation — **Stage 6 (Academic Content Management) only.**
**Date:** 2026-06-19
**Outcome:** ✅ Books, Publications, and Research Papers managed through one shared TPH content screen, plus a supporting Categories admin. Full features: translations (AR/EN/FR), categories, featured flag, SEO, cover + PDF uploads, reorder, publish toggle. **Build 0/0 · Tests 14/14 · verified at runtime.**
**Boundaries honoured:** no Videos/Resources, no public website, no search, no analytics.

---

## 1. Implemented Pages

| Page | Route | Purpose |
|---|---|---|
| **Content list** | `/Admin/Content?type={Book\|Publication\|ResearchPaper}` | shared list per type |
| **Content upsert** | `/Admin/Content/Upsert?type=…&id=…` | shared create/edit |
| **Categories list** | `/Admin/Categories` | supporting taxonomy list |
| **Categories upsert** | `/Admin/Categories/Upsert` | create/edit (trilingual Name + Slug) |

Per doc 06 §3, **one shared `Content` screen** drives all three content types (resolved from the `type` route value); only `Book`, `Publication`, `ResearchPaper` are accepted (others → 404). The sidebar wires Research/Publications/Books (under Collections) and Categories (under Organization) with type-aware active highlighting.

> Categories management is included because "Categories assignment" requires categories to exist; it is the doc-06 "Organization → Categories" screen.

## 2. CRUD Summary

| Operation | Implementation |
|---|---|
| **List** | `Content/Index` filters `ContentItem` by the discriminator (`ContentType`), shows cover thumb, title (default culture), year, published/featured badges, order |
| **Create / Edit** | `Content/Upsert` instantiates the correct subtype (`Book`/`Publication`/`ResearchPaper`) and upserts base + translation rows |
| **Delete** | `OnPostDelete` (confirm); cascades translations, category links, events |
| **Reorder** | `OnPostMove` normalizes + swaps `SortOrder` within the type |
| **Toggle publish / feature** | `OnPostTogglePublish` / `OnPostToggleFeature` list actions |

Type-specific handling: `Doi` (non-translatable) for Publication/ResearchPaper; translatable `Journal`/`Authors` (papers) vs `Publisher`/`AuthorshipRole` (books) written conditionally on the translation row. Data access via `AppDbContext` directly in PageModels (no repository/service layer), media via `IMediaUploadService`.

**Verified:** Book create → 302 (TPH `Book` row + translation); Publication create → 302 with **DOI persisted**; both appear in their type list.

## 3. Category Management Usage

- **Categories admin:** trilingual Name + Slug; slug auto-generated from name when blank (`SlugHelper`), made **unique per culture** (`CategoryTranslation (Culture, Slug)`), with reorder/delete.
- **Assignment:** the content Upsert form has a multi-select of all categories; `SyncCategories` reconciles `ContentItemCategory` join rows (adds new, removes deselected).
- **Verified:** created a category, assigned it to a book → **1 `ContentItemCategory` link** persisted.

## 4. Upload Handling Summary

- **Cover image** (jpg/png/webp ≤5 MB) and **PDF** (≤25 MB) per content item, via `IMediaUploadService` (extension allowlist + size + magic-byte sniff → `IFileStorage` + `MediaFile`).
- FKs set after successful upload (`CoverImageId`, `PdfFileId`); existing media previewed/linked on edit; failures surface as **localized** field errors and short-circuit the save (re-showing current media).
- Served read-only at `/uploads`.

## 5. Validation Summary

- **Server-side authoritative:** default-culture (Arabic) **Title required**; non-default cultures optional (skipped/removed when their Title is blank → fallback applies).
- **Data annotations** localized via `SharedResource` (`DataAnnotationLocalizerProvider`); upload errors localized by resource key.
- **Slug integrity:** generated + de-duplicated per culture (`-2`, `-3`, …) so the `(Culture, Slug)` unique index never trips.
- **Verified:** empty Arabic title → page redisplayed (200) with the localized required error; invalid `type` → 404.

## 6. Localization Summary

- Content edited in **AR / EN / FR** tabs (Arabic required + fallback-skip; RTL per tab). New content/SEO field labels and statuses added to `SharedResource.{neutral,ar,fr}.resx`.
- All chrome (titles, buttons, badges, validation) resolves through `IViewLocalizer` / `IStringLocalizer<SharedResource>`; admin direction follows the current culture.

## 7. Files Added / Changed

**Added:** `Web/Infrastructure/SlugHelper.cs`; `Areas/Admin/Pages/Content/{Index,Upsert}.cshtml(.cs)`; `Areas/Admin/Pages/Categories/{Index,Upsert}.cshtml(.cs)`.
**Changed:** `Resources/SharedResource{,.ar,.fr}.resx` (content/SEO/category keys); `Areas/Admin/Pages/Shared/_Sidebar.cshtml` (wire Collections + Categories with type-aware active state).

## 8. Deviations from Architecture

1. **One shared content screen** for the three types (doc 06 §3 intent) instead of three separate page sets — type via the `type` route value; the correct subtype is instantiated on create.
2. **Categories admin added** as a supporting screen (needed for assignment) — within doc 06's Organization section, not an out-of-scope item.
3. **Per-culture global slug uniqueness** `(Culture, Slug)` (matches the Stage-2 index decision); per-(type,culture) relaxation deferrable to the app layer if ever needed.
4. **No repository/application-service layer** — PageModels use `AppDbContext` directly (consistent with Stage 5).
5. **Slug-change 301 redirects not created here** — that is the SEO stage (doc 14 S-06); slugs are de-duplicated but redirect history is out of scope for this stage.

## 9. Verification

```
dotnet build → 0 warnings / 0 errors (net8.0)
dotnet test  → 14/14 passed
Runtime (seeded admin, throwaway DB):
  GET /Admin/Content?type=Book|Publication|ResearchPaper → 200 ; type=Video → 404
  GET /Admin/Categories → 200 ; create category → 302
  Create Book (category + featured + published) → 302
  Create Publication (DOI + journal) → 302
  Empty Arabic title → 200 (rejected, localized error)
  DB: ContentItem {Book:1, Publication:1}; 2 translations; 1 category link; 1 featured book; Publication.Doi=10.1000/xyz123
Throwaway App_Data deleted; no DB committed.
```

**⏸ Stage 6 complete. Stopping here as instructed — awaiting approval.** (Changes are uncommitted; say the word to create the checkpoint.)
