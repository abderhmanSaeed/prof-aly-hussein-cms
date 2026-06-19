# 11 — SEO Strategy (FINAL)

**Status:** **FINAL — single source of truth (v2.0).** Adds the `Redirect`/URL-alias mechanism, partial-culture `hreflang` handling for the initially-empty French, and the legacy `*.html` → `/{culture}/{slug}` cutover map.

Server-rendered Razor Pages give complete HTML to crawlers natively — the main reason this architecture beats a SPA for an academic site. The strategy makes every page and every language independently discoverable.

---

## 1. Metadata (per page, per culture)

- **Title:** from content `MetaTitle` (falls back to `Title`) or `PageSeo.MetaTitle` for static pages; pattern e.g. `{Title} — {SiteTitle}`.
- **Meta description:** from `MetaDescription` (falls back to `Summary`, truncated ~160 chars).
- **Meta keywords:** from `MetaKeywords` (low SEO weight today but requested; supported).
- **Canonical:** every page emits a `<link rel="canonical">` to its absolute culture URL to avoid duplicate-content issues.
- **Open Graph / Twitter cards:** title, description, and cover image (`og:image`) for good link previews (important for social sharing of publications).
- **`<html lang>` and `dir`** set per culture.

A single SEO partial/view component builds the `<head>` block from a per-page SEO model so every page is consistent.

## 2. Slugs

- Human-readable, lowercase, hyphenated, **per culture**, stored on translation rows.
- Auto-generated from Title (transliteration-aware for Arabic), editable, validated unique per `(ContentType, Culture)`.
- **Stable once published:** changing a published slug offers to create a **301 redirect** from the old slug, preserving acquired SEO.
- Route shape: `/{culture}/{section}/{slug}` — descriptive and crawlable.

## 3. Sitemap

- **Dynamic `/sitemap.xml`**, generated from the database, including:
  - All published content items in all three cultures.
  - Static pages (Home, About, Experience, Teaching, Contact, listing pages) per culture.
  - Category pages.
- Each URL entry includes `lastmod` (from `ModifiedUtc`) and **`xhtml:link` alternates** for the AR/EN/FR variants.
- For a large catalogue, support a sitemap index; for this scale a single sitemap is fine.
- Referenced from `robots.txt`.

## 4. `robots.txt`

- Allow crawling of public content; **disallow `/admin`**; reference the sitemap.
- No blocking of culture paths.

## 5. `hreflang` / International SEO

- Every page emits `hreflang` alternates for the cultures that **have a published translation**, plus `x-default` (pointing at the default culture).
- **Partial-culture rule (French initially empty):** emit a `hreflang="fr"` alternate **only when a French translation exists** for that page/item. While French is empty, pages advertise `ar`/`en` + `x-default`; French alternates appear automatically as the admin fills them in. (Falling back at render does not justify advertising a `fr` URL that is really the default-culture content.)
- Alternates are reciprocal and absolute URLs.

## 6. Structured Data (JSON-LD)

Emit schema.org JSON-LD appropriate to each page type:

| Page | Schema |
|---|---|
| About / site identity | `Person` (the professor: name, jobTitle, affiliation, sameAs → social links) |
| Publications / Research | `ScholarlyArticle` (headline, author, datePublished, isPartOf journal, identifier DOI) |
| Books | `Book` (name, author, about, image) |
| Theses | `Thesis` / `CreativeWork` |
| Videos | `VideoObject` (name, description, thumbnailUrl, embedUrl) |
| Site | `WebSite` (+ `SearchAction` for site search) |
| Breadcrumbs | `BreadcrumbList` |

JSON-LD is generated from the same content data, per culture.

## 7. Performance as SEO

- Fast server-rendered pages; cached immutable assets (GUID-named media).
- Defer YouTube iframes until interaction (avoids heavy third-party load on first paint).
- Responsive images (`srcset`) from stored width/height; lazy-loading.
- Clean, semantic HTML and proper heading hierarchy.

## 8. Admin Control

- The **SEO** admin page edits `PageSeo` for static pages and defaults.
- Each content edit screen exposes `MetaTitle/MetaDescription/MetaKeywords` per language tab.
- Sensible auto-fallbacks mean the professor gets reasonable SEO even without filling every field.

## 9. Migration / Legacy URLs (Redirect / UrlAlias)

Legacy SEO equity lives on `aly-hussein.netlify.app/*.html`. The cutover changes both the host (to the production domain) and the URL shape (to `/{culture}/{slug}`), so **301 redirects are mandatory**. These are stored in the **`Redirect`** table (doc 03 §2.8 / doc 05 §16) and resolved by middleware on each request; the admin manages them on the **Redirects** screen (doc 06).

**Two redirect sources:**
1. **Legacy map (one-off, seeded at cutover)** — every old page → its default-culture equivalent:

   | Legacy URL | New target |
   |---|---|
   | `/index.html` | `/ar/` |
   | `/about.html` | `/ar/about` |
   | `/experience.html` | `/ar/experience` |
   | `/publications.html` | `/ar/publications` |
   | `/books.html` | `/ar/books` |
   | `/research.html` | `/ar/activities` *(legacy "research" = Activities)* |
   | `/theses.html` | `/ar/theses` |
   | `/teaching.html` | `/ar/teaching` |
   | `/contact.html` | `/ar/contact` |

2. **Slug-change redirects (ongoing)** — changing a published slug auto-creates a `Redirect` from the old `/{culture}/{section}/{old-slug}` to the new one (doc 03 §1.3, task S-06).

Sitemap submission to search consoles + the new `robots.txt` complete the cutover (Phase 7).
