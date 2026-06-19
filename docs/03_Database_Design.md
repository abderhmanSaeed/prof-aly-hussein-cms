# 03 — Database Design (FINAL)

**Engine:** SQLite (WAL mode), accessed through EF Core.
**Status:** **FINAL — single source of truth (v2.0).** Incorporates the approved decisions from `16_Static_To_Dynamic_Migration_Plan.md` and `17_Final_PreImplementation_Review.md`.
**Design intent:** settle the multilingual and content models now so later phases only add data, never restructure tables.

> **What changed in v2.0 (from the static-site migration analysis):**
> - **Trilingual from day one** — `ar`, `en`, `fr` are all first-class. French rows may be empty; render falls back to the default culture.
> - **Six new dedicated entities** (fully admin-managed): `Qualification`, `Skill`, `Membership`, `StatItem`, `Credibility`, `Activity` (+ `ActivityGroup`).
> - **Profile extended:** `DateOfBirth` (base) + `ShortName`, `Positioning`, `Nationality`, `MaritalStatus`, `Location`, `Languages`, `CvFileId` (translation).
> - **Translatable academic metadata moved to translation rows:** `Journal`, `Authors`, `Publisher`, `AuthorshipRole`, `ResearcherName` (these are bilingual in the legacy data and must be per-culture).
> - **Book:** `Publisher`, `AuthorshipRole`, `IsFeatured`. **Thesis:** `ResearcherName`, `RelationshipType` (Supervised/Examined/Ongoing); the old `Supervisor` field is **removed** (the professor *is* the supervisor — the relationship is captured by `RelationshipType`).
> - **Course:** `Level` (Undergraduate/Graduate). **ExperienceEntry:** optional `PeriodLabel` (translation) + dates made optional.
> - **`Redirect` (URL alias) table** added for legacy `*.html` → `/{culture}/{slug}` 301s and slug-change redirects.
> - **`SiteSettings.DefaultTheme`** added (Light/Dark) — dark mode is retained; the static design system is the canonical theme.

---

## 1. Design Decisions

### 1.1 Content modeling — Table-Per-Hierarchy (TPH)
All "academic collection content" types share most fields (title, summary, body, slug, cover image, attached PDF, category, publish flag, sort order, dates, SEO). They are modeled as **one base entity `ContentItem` with a `ContentType` discriminator** and type-specific nullable columns.

- **Subtypes:** `Book`, `Publication`, `ResearchPaper`, `Thesis`, `Project`, `Resource`, `EnrichmentItem`, `Video`.
- **Why TPH:** one table, no joins to read a list, trivial to add a new content type, and a sparse table is harmless at this data volume.
- **Trade-off accepted:** some columns are null for some types (e.g. `YouTubeVideoId` only for `Video`; `PdfFileId` not for `Video`). Intentional and cheap.

> **Not in the TPH hierarchy** (modeled as their own small entities because their shape and admin UX differ): `Qualification`, `Skill`, `Membership`, `StatItem`, `Credibility`, `Activity`/`ActivityGroup`, `Course`, `ExperienceEntry`, `PageSection`. These are short, list-shaped, non-sluggable records — forcing them into `ContentItem` (slug + rich body + cover/PDF) would be wrong.

### 1.2 Multilingual content — translation tables
Translatable text lives in **per-entity translation tables** keyed by `(ParentId, Culture)`. Non-translatable data (dates, flags, FKs, file references, sort order, enums) stays on the base row.

- **Cultures:** `ar`, `en`, `fr` (2-letter ISO codes) — **all three first-class from day one.**
- **Why translation tables (not language columns):** adding/withholding a language never alters schema; queries filter by culture cleanly.
- **French may be empty:** an item needs only its default-culture translation to publish; missing FR falls back at render time (see doc 10 §5).

### 1.3 Slugs and SEO
- Slugs are **per culture** and live on the translation row (supports `/{culture}/books/{slug}`).
- SEO fields (`MetaTitle`, `MetaDescription`, `MetaKeywords`) for content items live on the translation row.
- Static pages use a separate `PageSeo` table keyed by `(PageKey, Culture)`.
- **Redirects** (legacy + slug-change 301s) live in the `Redirect` table.

### 1.4 Files
- Files are stored on disk; only **metadata** is stored in `MediaFile`. Content rows reference media by FK (`CoverImageId`, `PdfFileId`, `Profile.PhotoMediaId`, `ProfileTranslation.CvFileId`, `SiteSettings.LogoMediaId`, `Credibility.LogoMediaId`). No BLOBs in the database.

### 1.5 Authentication
- **ASP.NET Core Identity** provides its own tables. A single Super Admin user and one `SuperAdmin` role.

### 1.6 Theme / design system
- The **static site's design tokens and fonts are the canonical theme** (see doc 07 §3). `SiteSettings` carries `PrimaryColor`, `SecondaryColor` (accent), and `DefaultTheme` (Light/Dark). The light/dark toggle is a client-side preference (localStorage); `DefaultTheme` sets the first-visit default.

---

## 2. Tables

### 2.1 Settings & Profile

**`SiteSettings`** (singleton row, `Id = 1`)
| Column | Type | Notes |
|---|---|---|
| Id | INTEGER PK | always 1 |
| DefaultCulture | TEXT | `ar` \| `en` \| `fr` (default `ar`) |
| DefaultTheme | TEXT | `Light` \| `Dark` (default `Light`) |
| LogoMediaId | INTEGER FK→MediaFile | nullable (brand falls back to the "ع" glyph mark) |
| FacebookUrl | TEXT | nullable |
| WhatsAppNumber | TEXT | nullable |
| ContactEmail | TEXT | receives contact-form mail |
| PrimaryColor / SecondaryColor | TEXT | theme tokens, nullable |
| CreatedUtc / ModifiedUtc | TEXT (ISO) | |

**`SiteSettingsTranslation`** — `(SiteSettingsId, Culture)` → `SiteTitle`, `FooterText`, `Tagline`.

**`Profile`** (singleton, the professor)
| Column | Type | Notes |
|---|---|---|
| Id | INTEGER PK | always 1 |
| PhotoMediaId | INTEGER FK→MediaFile | nullable |
| DateOfBirth | TEXT (ISO date) | nullable; rendered per culture |
| Email / Phone | TEXT | nullable |
| CreatedUtc / ModifiedUtc | TEXT | |

**`ProfileTranslation`** — `(ProfileId, Culture)`
| Column | Type | Notes |
|---|---|---|
| ProfileId | INTEGER FK | |
| Culture | TEXT | ar/en/fr |
| FullName | TEXT | required |
| ShortName | TEXT | nullable (brand/nav short form) |
| Title | TEXT | required (e.g. "Professor of …") |
| Positioning | TEXT | nullable (hero one-line positioning) |
| ShortBio | TEXT | nullable |
| FullBio | TEXT | nullable, rich text (multi-paragraph) |
| Nationality | TEXT | nullable |
| MaritalStatus | TEXT | nullable |
| Location | TEXT | nullable |
| Languages | TEXT | nullable (e.g. "Arabic (native), French (second)") |
| CvFileId | INTEGER FK→MediaFile (Pdf) | nullable — per-culture CV download |

### 2.2 Content (TPH)

**`ContentItem`** (base, TPH)
| Column | Type | Notes |
|---|---|---|
| Id | INTEGER PK | |
| ContentType | TEXT (discriminator) | Book/Publication/ResearchPaper/Thesis/Project/Resource/EnrichmentItem/Video |
| CoverImageId | INTEGER FK→MediaFile | nullable |
| PdfFileId | INTEGER FK→MediaFile | nullable (not for Video) |
| ExternalUrl | TEXT | nullable (project link, DOI landing) |
| YouTubeVideoId | TEXT | nullable (Video only) |
| PublicationYear | INTEGER | nullable |
| EventDateUtc | TEXT | nullable (project/thesis dates) |
| IsPublished | INTEGER (bool) | draft/published, default 0 |
| IsFeatured | INTEGER (bool) | **new** — homepage featuring (used by Book), default 0 |
| SortOrder | INTEGER | display ordering, default 0 |
| ViewCount | INTEGER | denormalized counter, default 0 |
| DownloadCount | INTEGER | denormalized counter, default 0 |
| CreatedUtc / ModifiedUtc | TEXT | |

**Type-specific NON-translatable columns** (kept on the base under TPH):
`Doi` (Publication/Research); `DegreeLevel` (Thesis: Master/PhD); **`RelationshipType`** (Thesis: Supervised/Examined/Ongoing); `ProjectStatus` (Project: Ongoing/Completed); `Role` (Project — the professor's role); `ResourceType` (Resource/Enrichment).

> **Removed in v2.0:** `Supervisor` (Thesis) — superseded by `RelationshipType` + `ResearcherName`.

**`ContentItemTranslation`** — `(ContentItemId, Culture)`
| Column | Type | Notes |
|---|---|---|
| Id | INTEGER PK | |
| ContentItemId | INTEGER FK | |
| Culture | TEXT | ar/en/fr |
| Title | TEXT | required |
| Slug | TEXT | required, unique per (ContentType, Culture) |
| Summary | TEXT | short description (card + popup) |
| Body | TEXT | full rich text, nullable |
| Journal | TEXT | **new location** — Publication/Research, nullable, translatable |
| Authors | TEXT | **new location** — Publication/Research, nullable, translatable |
| Publisher | TEXT | **new** — Book, nullable, translatable |
| AuthorshipRole | TEXT | **new** — Book (e.g. "Sole author"/"With dept. members"), nullable, translatable |
| ResearcherName | TEXT | **new** — Thesis (the student/researcher), nullable, translatable |
| MetaTitle / MetaDescription / MetaKeywords | TEXT | SEO, nullable |

> `Journal` and `Authors` were on the base row in v1.0; they are **moved to the translation row** because the legacy data proves they are per-culture (e.g. "مجلة التربية…" / "Journal of Education…").

### 2.3 Taxonomy

**`Category`** — `Id`, `SortOrder`, `CreatedUtc/ModifiedUtc`.
**`CategoryTranslation`** — `(CategoryId, Culture)` → `Name`, `Slug`.
**`ContentItemCategory`** — join table, composite PK `(ContentItemId, CategoryId)`.

### 2.4 Profile-page & list entities (NEW dedicated entities)

All trilingual via their translation tables and ordered by `SortOrder`.

**`Qualification`** — `Id`, `Year` (INTEGER, nullable), `SortOrder`, timestamps.
**`QualificationTranslation`** — `(QualificationId, Culture)` → `Degree` (req), `Institution` (req), `Grade` (nullable).

**`Skill`** — `Id`, `SortOrder`, timestamps.
**`SkillTranslation`** — `(SkillId, Culture)` → `Name` (req).

**`Membership`** — `Id`, `Kind` (TEXT: `Society` \| `Board`), `SortOrder`, timestamps.
**`MembershipTranslation`** — `(MembershipId, Culture)` → `Name` (req).

**`StatItem`** — `Id`, `Value` (INTEGER), `Suffix` (TEXT, e.g. `+`), `SortOrder`, timestamps.
**`StatItemTranslation`** — `(StatItemId, Culture)` → `Label` (req).

**`Credibility`** — `Id`, `LogoMediaId` (FK→MediaFile, nullable), `SortOrder`, timestamps.
**`CredibilityTranslation`** — `(CredibilityId, Culture)` → `Name` (req).

**`ActivityGroup`** — `Id`, `SortOrder`, timestamps.
**`ActivityGroupTranslation`** — `(ActivityGroupId, Culture)` → `Name` (req).

**`Activity`** — `Id`, `ActivityGroupId` (FK→ActivityGroup), `SortOrder`, timestamps.
**`ActivityTranslation`** — `(ActivityId, Culture)` → `Text` (req).

### 2.5 Page sections, experience, teaching

**`PageSection`** — `Id`, `PageKey` (e.g. `home`, `about`, `contact`), `SortOrder`.
**`PageSectionTranslation`** — `(PageSectionId, Culture)` → `Heading`, `Body`.

**`ExperienceEntry`** — `Id`, `StartDateUtc` (nullable), `EndDateUtc` (nullable = present), `SortOrder`.
**`ExperienceEntryTranslation`** — `(ExperienceEntryId, Culture)` → `Role` (req), `Organization` (req), `Description` (nullable), **`PeriodLabel`** (nullable — preserves free-text like "Jul 2018 – present").

**`Course`** (Teaching) — `Id`, **`Level`** (TEXT: `Undergraduate` \| `Graduate`), `Period` (nullable), `SortOrder`.
**`CourseTranslation`** — `(CourseId, Culture)` → `CourseName` (req), `Institution` (nullable), `Description` (nullable).

**`PageSeo`** — `(PageKey, Culture)` → `MetaTitle`, `MetaDescription`, `MetaKeywords`.

### 2.6 Media

**`MediaFile`**
| Column | Type | Notes |
|---|---|---|
| Id | INTEGER PK | |
| StoredFileName | TEXT | unique on disk (GUID-based) |
| OriginalFileName | TEXT | user's name (display) |
| RelativePath | TEXT | path under uploads root |
| ContentType | TEXT | MIME |
| MediaKind | TEXT | Image / Pdf / Thumbnail |
| SizeBytes | INTEGER | enforced max |
| Width / Height | INTEGER | images only, nullable |
| AltText | TEXT | nullable (accessibility) |
| CreatedUtc | TEXT | |

### 2.7 Communication

**`ContactMessage`** — `Id`, `Name`, `Email`, `Subject` (nullable), `Message`, `CreatedUtc`, `IsRead` (bool), `IpAddress` (nullable).

### 2.8 SEO / Redirects (NEW)

**`Redirect`** (URL alias / 301 map)
| Column | Type | Notes |
|---|---|---|
| Id | INTEGER PK | |
| FromPath | TEXT | required; unique; absolute path incl. legacy form (e.g. `/about.html`) |
| ToPath | TEXT | required; target path (e.g. `/ar/about`) |
| StatusCode | INTEGER | default 301 (301/302) |
| IsActive | INTEGER (bool) | default 1 |
| Notes | TEXT | nullable (e.g. "legacy netlify URL") |
| CreatedUtc / ModifiedUtc | TEXT | |

> Serves the legacy `*.html` → `/{culture}/{slug}` cutover and future slug-change redirects (doc 11 §2/§9).

### 2.9 Statistics

**`ContentEvent`** — `Id`, `ContentItemId` FK, `EventType` (View/Download/Play), `Culture`, `CreatedUtc`.
**`PageView`** — `Id`, `Path`, `Culture`, `CreatedUtc`.

### 2.10 Identity (ASP.NET Core Identity)
`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`, `AspNetRoleClaims`. Seeded with one Super Admin user + `SuperAdmin` role.

---

## 3. Relationships (summary)

- `SiteSettings 1—* SiteSettingsTranslation`; `SiteSettings *—1 MediaFile` (logo)
- `Profile 1—* ProfileTranslation`; `Profile *—1 MediaFile` (photo); `ProfileTranslation *—1 MediaFile` (CV per culture)
- `ContentItem 1—* ContentItemTranslation`; `ContentItem *—1 MediaFile` (cover); `ContentItem *—1 MediaFile` (pdf)
- `ContentItem *—* Category` via `ContentItemCategory`; `Category 1—* CategoryTranslation`
- `ContentItem 1—* ContentEvent`
- `Qualification 1—* QualificationTranslation`
- `Skill 1—* SkillTranslation`
- `Membership 1—* MembershipTranslation`
- `StatItem 1—* StatItemTranslation`
- `Credibility 1—* CredibilityTranslation`; `Credibility *—1 MediaFile` (logo)
- `ActivityGroup 1—* ActivityGroupTranslation`; `ActivityGroup 1—* Activity`; `Activity 1—* ActivityTranslation`
- `PageSection 1—* PageSectionTranslation`
- `ExperienceEntry 1—* ExperienceEntryTranslation`
- `Course 1—* CourseTranslation`
- Identity user(s) are standalone (single admin)

## 4. Constraints

- **Primary keys:** integer surrogate keys everywhere (composite on join/translation tables where natural).
- **Foreign keys:** enforced (`PRAGMA foreign_keys = ON`). Media FKs `ON DELETE SET NULL`; translations `ON DELETE CASCADE` with their parent; `Activity → ActivityGroup` `ON DELETE CASCADE`.
- **Uniqueness:**
  - `ContentItemTranslation`: unique `(ContentItemId, Culture)` and unique `(ContentType, Culture, Slug)`.
  - `CategoryTranslation`: unique `(CategoryId, Culture)`, unique `(Culture, Slug)`.
  - All `*Translation` tables: unique `(ParentId, Culture)`.
  - `MediaFile.StoredFileName`: unique.
  - `PageSeo`: unique `(PageKey, Culture)`.
  - `Redirect.FromPath`: unique.
- **Required fields:** `Title` + `Slug` on every content translation; required name fields on the new entity translations (`Degree`/`Institution`, `Skill.Name`, `Membership.Name`, `StatItem.Label`, `Credibility.Name`, `ActivityGroup.Name`, `Activity.Text`); `Culture` constrained to `ar|en|fr`.
- **Enums (CHECK or app-enforced):** `RelationshipType ∈ {Supervised, Examined, Ongoing}`; `DegreeLevel ∈ {Master, PhD}`; `Course.Level ∈ {Undergraduate, Graduate}`; `Membership.Kind ∈ {Society, Board}`; `ProjectStatus ∈ {Ongoing, Completed}`; `MediaKind ∈ {Image, Pdf, Thumbnail}`; `DefaultTheme ∈ {Light, Dark}`.
- **Culture check constraint:** `CHECK (Culture IN ('ar','en','fr'))` on every translation table.
- **Defaults:** `IsPublished = 0`, `IsFeatured = 0`, `SortOrder = 0`, counters `= 0`, `Redirect.StatusCode = 301`, `Redirect.IsActive = 1`.

## 5. Index Recommendations

| Index | Purpose |
|---|---|
| `ContentItem(ContentType, IsPublished, SortOrder)` | fast published listings per section |
| `ContentItem(ContentType, IsFeatured, IsPublished)` | homepage featured selection |
| `ContentItem(ContentType, RelationshipType, PublicationYear)` | theses facet filtering |
| `ContentItemTranslation(ContentItemId, Culture)` (unique) | translation lookup |
| `ContentItemTranslation(ContentType?, Culture, Slug)` (unique) | slug routing |
| `ContentItemCategory(CategoryId)` | category filtering |
| `CategoryTranslation(Culture, Slug)` (unique) | category routing |
| `ContentEvent(ContentItemId, EventType, CreatedUtc)` | stats aggregation |
| `PageView(CreatedUtc)` | time-window stats |
| `ContactMessage(IsRead, CreatedUtc)` | inbox views |
| `MediaFile(MediaKind, CreatedUtc)` | media library browsing |
| `Redirect(FromPath)` (unique) | redirect resolution on request |
| `*(SortOrder)` on each list entity | ordered admin/public lists |

## 6. Full-Text Search

- **SQLite FTS5** virtual table indexing `Title + Summary + Body` (and `Journal/Authors/Publisher/ResearcherName`) from `ContentItemTranslation`, filtered by `Culture`.
- Kept synchronized via the application service on create/update/delete.
- **Arabic note:** `unicode61` tokenizer with `remove_diacritics`; `LIKE` fallback over normalized text if recall is insufficient. The most-searched legacy surfaces are **theses** (researcher + title) and **publications** — ensure these are indexed.

## 7. Pragmas & Operational Settings

- `PRAGMA journal_mode = WAL;`
- `PRAGMA foreign_keys = ON;`
- `PRAGMA synchronous = NORMAL;`
- `PRAGMA busy_timeout = 5000;`
