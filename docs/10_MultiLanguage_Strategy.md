# 10 — Multi-Language Strategy (FINAL)

**Status:** **FINAL — single source of truth (v2.0).**

The platform is trilingual: **Arabic (`ar`, RTL, primary), English (`en`), French (`fr`)** — **all three first-class from day one** (database, translations, admin screens, validation, and routing all support `fr`). Localization is two separate concerns handled by two mechanisms.

> **French content policy (approved decision):** the legacy site supplied only AR/EN content, so **French content is initially empty.** The schema, admin (AR/EN/FR tabs), and `/{culture}/` routing fully support French now; missing French text **falls back to the default culture at render time** (§5). French uses the Latin font stack and LTR direction.

---

## 1. Two Layers of Localization

| Layer | What it covers | Mechanism |
|---|---|---|
| **UI / chrome** | Buttons, labels, nav text, validation messages, system strings | .NET resource files (`.resx`) + `IStringLocalizer` |
| **Content** | The professor's data — titles, summaries, bodies, slugs, SEO | Translation tables in the database (`*Translation`) |

Keeping these separate means the developer owns UI strings (in resources, versioned with code) while the admin owns content (in the database, no deploy needed).

## 2. Culture Resolution & Routing

- **URL-segment culture:** every public route is `/{culture}/...` (e.g. `/ar/books`, `/fr/recherche`). This is explicit, shareable, crawlable, and unambiguous for SEO.
- `RequestLocalizationMiddleware` with a **route-value request culture provider** reads the segment and sets `CultureInfo.CurrentCulture` / `CurrentUICulture`.
- **Default culture:** `SiteSettings.DefaultCulture` (Arabic by default). A request to `/` redirects to `/{default}/`.
- **Supported cultures:** `ar`, `en`, `fr` only; unknown cultures fall back to default.
- First-visit hint: `Accept-Language` may inform the initial redirect, but the explicit switcher and URL always win.

## 3. Language Switcher

- Present in header and footer.
- Switching **preserves the current page** by mapping to the equivalent route/slug in the target culture.
- For a content detail page, the switcher links to the same item's slug in the target culture; if that translation is missing, it falls back to the section list in the target culture (and is flagged to the admin internally).

## 4. RTL / LTR

- Arabic renders `dir="rtl"`; English/French `dir="ltr"`.
- Bootstrap 5 RTL is enabled (RTL stylesheet selected by culture), and layout (margins, alignment, icons) mirrors automatically.
- The admin UI may stay LTR with per-field direction on the Arabic content tab, so editing RTL text is correct even within an LTR dashboard.
- Fonts: a quality Arabic webfont (e.g. a Naskh/− modern Arabic family) plus a Latin family; chosen for readability in both directions.

## 5. Content Translation Model (recap)

- Each translatable entity has a translation table keyed by `(ParentId, Culture)`.
- **Publishing rule:** an item needs at least the **default-culture** translation to publish; AR/EN/FR completeness is shown but not forced.
- **Fallback at render:** if a requested culture's translation is missing for a published item, the site falls back to the default culture for that item (configurable) rather than 404, so visitors always see content.
- **Slugs per culture** enable localized, readable URLs.

## 6. Numbers, Dates, Direction Details

- Dates/numbers formatted per `CultureInfo` (e.g. Arabic vs. Latin digits handled consistently — pick a house style and apply it site-wide).
- Sorting/collation: list ordering uses `SortOrder` (admin-controlled) rather than locale collation, avoiding Arabic collation pitfalls.

## 7. SEO Interaction (see SEO Strategy)

- Each culture URL is independently indexable.
- `hreflang` alternates link the three language versions of each page.
- `<html lang>` and `dir` set per culture.
- Sitemap lists all culture variants.

## 8. Adding a Fourth Language Later

French is already a day-one culture; this section is the proof that a **fourth** language is purely additive. Because UI strings are in `.resx` and content is in translation tables keyed by culture:
- Add the culture code to the supported list + a `.resx` set.
- Admin starts adding `Culture = 'xx'` translation rows.
- **No schema change, no data migration.** This is the payoff of the translation-table design.

## 8b. Digit House Style (approved)

The legacy data mixed Western digits (years/stats) with Arabic-Indic digits (dates/periods). **House style:** use **Western digits (1, 2, 3) site-wide in all cultures** for years, counts, and dates (matching the original master-prompt intent and avoiding inconsistency). `Profile.DateOfBirth` is stored as a real date and **formatted per `CultureInfo`** at render; `ExperienceEntry.PeriodLabel` preserves any free-text wording the admin enters.

## 9. Practical Notes
- Provide the admin a clear per-item "translation status" (✅/⚠️ per language) so trilingual coverage is easy to maintain.
- Validation/error messages and emails (contact confirmation) are localized via `.resx`.
- Keep a single source of truth for culture codes (a constant/enum) used across routing, resources, and DB checks.
