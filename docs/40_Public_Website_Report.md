# 40 — Public Website Report (Stage 8)

**Phase:** Implementation — **Stage 8: Public Website**.
**Date:** 2026-06-19
**Outcome:** ✅ Ten public pages implemented, **100% database-driven** (no `data.js` / `static-content.json` / hardcoded content). Trilingual (AR default RTL / EN / FR fallback), responsive, Bootstrap 5, SEO-friendly, clean URLs. **Build 0/0 · Tests 14/14.**
**Boundaries honoured:** no Search, Analytics, Videos, or Resources.

---

## 1. Pages Implemented

| # | Page | Route | Data source (DB) |
|---|---|---|---|
| 1 | **Home** | `/{c}` | Profile + bio, Credibility, StatItem, featured Books |
| 2 | **About** | `/{c}/about` | Profile (bio paragraphs, personal details), Qualifications, Skills |
| 3 | **Experience** | `/{c}/experience` | ExperienceEntry (timeline), Memberships (societies/boards) |
| 4 | **Teaching** | `/{c}/teaching` | Course (split by level) |
| 5 | **Books** | `/{c}/books` | ContentItem=Book (paginated, featured flag) |
| 6 | **Publications** | `/{c}/publications` | ContentItem=Publication |
| 7 | **Research** | `/{c}/research` | ContentItem=ResearchPaper |
| 8 | **Theses** | `/{c}/theses` | ContentItem=Thesis (filterable table) |
| 9 | **Activities** | `/{c}/activities` | ActivityGroup → Activity (accordion) |
| 10 | **Contact** | `/{c}/contact` | Profile/SiteSettings (info) + form → ContactMessage |

`{c}` = `ar` \| `en` \| `fr` (route constraint). `/` → 302 → `/ar`.

---

## 2. Database-Only Rendering

- Every page inherits **`PublicPageModel`**, which queries `AppDbContext` directly and loads header/footer chrome (site title, tagline, contact) from `SiteSettings`/`Profile`.
- **`Localized.Pick(translations, culture)`** selects the current-culture translation with **fallback to the default culture (ar)** when missing (doc 10 §5) — so French (empty) renders Arabic content transparently.
- Published filter (`IsPublished`) applied to Books/Publications/Research/Theses; ordering by `SortOrder`.
- **No reference** to `data.js`, `static-content.json`, or any hardcoded content anywhere in the public pages.

## 3. Design Fidelity

- New **`wwwroot/css/public.css`** ports the static site's palette (`--primary #0B5D3B`, `--accent #C8A45D`), warm surfaces, dark-mode tokens, fonts (Amiri/IBM Plex Sans Arabic for AR; Cormorant Garamond/Inter for EN/FR), spacing, radii, shadows.
- Components mirror the original: hero, credibility chips, animated stat counters, book cards (CSS-generated covers when no image), publication/timeline lists, theses table with filter tabs, activities accordion, CTA band, footer.
- Bootstrap 5 (RTL stylesheet auto-selected for Arabic) for grid/responsiveness; custom CSS for brand components.

## 4. Requirements Coverage

| Requirement | Status |
|---|---|
| Arabic default + RTL | ✅ `dir="rtl"` on `/ar`, RTL Bootstrap |
| English supported | ✅ `dir="ltr"`, English translations render |
| French fallback | ✅ FR routes resolve; empty FR falls back to AR |
| Mobile responsive | ✅ Bootstrap grid + responsive nav (burger), fluid hero/grids |
| Bootstrap 5 | ✅ |
| SEO friendly | ✅ per-page `<title>`/description, canonical, `hreflang` (ar/en/fr + x-default), `og:*`, `lang`/`dir`; raw UTF-8 output |
| Clean URLs | ✅ `/{culture}/section` |
| Pagination | ✅ Books (12/page) |
| Featured content | ✅ Home featured books (Book.IsFeatured) |
| Dynamic statistics | ✅ StatItem → animated counters |
| Dynamic credibility | ✅ Credibility → chips |
| Language switching | ✅ switcher rebuilds current page URL per culture |
| Theme (dark/light) | ✅ toggle + `localStorage` |
| Contact form | ✅ validation + honeypot → `ContactMessage` (PRG) |

**UTF-8 fix:** registered `HtmlEncoder.Create(UnicodeRanges.All)` so Arabic renders as raw UTF-8 (not numeric entities) — better SEO/readability, matches the static site.

## 5. Verification (live, against imported DB)

Seeded with `Seed:ImportStaticContent=true`, then:

- **All 10 pages → HTTP 200** in `/ar` and `/en`; `/` → 302 → `/ar`.
- **RTL/LTR:** `<html dir="rtl">` on `/ar`, `dir="ltr"` on `/en`.
- **Arabic content renders** (raw UTF-8): e.g. `/ar` 1240, `/ar/theses` 6215, `/ar/about` 1648 Arabic chars.
- **English content renders:** `/en` full English name; `/en/books` English titles; `/en/theses` English researchers.
- **Dynamic sections:** stats counters (`data-target`) + credibility chips present on home; featured books shown.
- **Books pagination** active (14 books → 2 pages).
- **Contact form:** `POST /en/contact` → 302 (PRG); `ContactMessage` row persisted (`IsRead=0`).
- **Admin still gated:** `/Admin` (anon) → 302.
- **Build:** 0 warnings / 0 errors. **Tests:** 14/14. No database committed (throwaway `App_Data` deleted).

## 6. Files Added / Changed

**Added (public site):**
- `Pages/Public/PublicPageModel.cs`, `_ViewImports.cshtml`, `_ViewStart.cshtml`, `Shared/_PublicLayout.cshtml`
- 10 page sets: `Home`, `About`, `Experience`, `Teaching`, `Books`, `Publications`, `Research`, `Theses`, `Activities`, `Contact` (`.cshtml` + `.cshtml.cs`)
- `Infrastructure/Localized.cs` (translation fallback helper)
- `wwwroot/css/public.css`, `wwwroot/js/public.js`
- `docs/40_Public_Website_Report.md`

**Changed:**
- `Pages/Index.cshtml(.cs)` → root culture redirect
- `Program.cs` → `HtmlEncoder` (UnicodeRanges.All)
- `Resources/SharedResource.{neutral,ar,fr}.resx` → public chrome keys (nav, sections, contact)
- `.editorconfig` → suppress CA1716 (`Public` namespace)

## 7. Deviations / Notes

1. **Research vs Activities:** per the frozen naming map (doc 07), the legacy `research.html` content was imported as **Activities** (5 groups / 26 items) — shown on the **Activities** page. The **Research** page renders the `ResearchPaper` content type, which currently has **0 records** (none in the legacy data); it displays an empty-state and is ready for future research papers. This is intentional and matches the entity model.
2. **No profile photo / book covers** in the legacy data → CSS-generated portrait glyph and book covers are used as graceful fallbacks; real images can be uploaded via admin and will render automatically.
3. **Contact form** stores messages in the DB (`ContactMessage`); an admin inbox screen and email delivery are later-stage items (not in Stage 8 scope). Honeypot anti-spam included.
4. **PageModels query `AppDbContext` directly** (consistent with Stages 5–7); `PublicPageModel` centralises culture + chrome.
5. Excluded as instructed: **Search, Analytics, Videos, Resources**.

## 8. Verification Commands

```
dotnet build → 0 warnings / 0 errors (net8.0)
dotnet test  → 14/14 passed
Seeded import → all 10 pages HTTP 200 (ar/en); / → 302 /ar
Arabic raw UTF-8 confirmed; RTL/LTR confirmed; pagination + contact POST confirmed
Throwaway App_Data deleted; no DB committed
```

**⏸ Stage 8 complete. Stopping here as instructed — awaiting approval.** (Changes uncommitted; ready for a checkpoint on request.)
