# 20 — Domain Model Report (Stage 1)

**Phase:** Implementation — **Stage 1 (Domain Model) only** (per `15_Claude_Code_Execution_Plan.md`, against `18`/`03`/`04`/`05`).
**Date:** 2026-06-19
**Outcome:** ✅ Complete v2.0 domain layer implemented in `ProfAly.CMS.Domain`. **Build: 0 warnings / 0 errors. Tests: 14/14 pass.**
**Boundaries honoured:** no DbContext mappings, no EF configurations, no migrations, no repositories, no services, no Razor Pages, no admin UI. **Stage 2 was not started.**

> The domain is pure POCO + enums + constants + invariant methods. EF will map these in Stage 2; nothing here references EF Core.

---

## 1. Entity Inventory

**43 domain types** under `ProfAly.CMS.Domain.Entities` (+ abstract base + join). Each translatable parent has a matching `*Translation` (keyed `(ParentId, Culture)`, implements `ITranslation`).

### 1.1 Singletons & chrome
| Entity | Translation | Notes |
|---|---|---|
| `SiteSettings` | `SiteSettingsTranslation` | + `DefaultTheme` (Light/Dark); logo FK |
| `Profile` | `ProfileTranslation` | base: `DateOfBirth`; translation: ShortName, Positioning, Nationality, MaritalStatus, Location, Languages, **CvFileId** (per-culture CV) |

### 1.2 Content — Table-Per-Hierarchy
| Type | Kind | Type-specific fields |
|---|---|---|
| `ContentItem` | abstract base | Cover/Pdf FKs, ExternalUrl, PublicationYear, EventDateUtc, IsPublished, **IsFeatured**, SortOrder, View/DownloadCount |
| `Book` | subtype | (Publisher/AuthorshipRole are translatable) |
| `Publication` | subtype | `Doi` |
| `ResearchPaper` | subtype | `Doi` |
| `Thesis` | subtype | `DegreeLevel`, `RelationshipType` |
| `Project` | subtype | `ProjectStatus`, `Role` |
| `Resource` | subtype | `ResourceType` |
| `EnrichmentItem` | subtype | `ResourceType` |
| `Video` | subtype | `YouTubeVideoId` |
| `ContentItemTranslation` | translation | Title, Slug, Summary, Body, **Journal, Authors, Publisher, AuthorshipRole, ResearcherName**, Meta* |

### 1.3 Taxonomy
`Category` · `CategoryTranslation` · `ContentItemCategory` (join, composite key — no surrogate Id).

### 1.4 Profile-page / list entities (all new in v2.0)
| Entity | Translation | Base fields |
|---|---|---|
| `Qualification` | `QualificationTranslation` | Year, SortOrder · (Degree, Institution, Grade) |
| `Skill` | `SkillTranslation` | SortOrder · (Name) |
| `Membership` | `MembershipTranslation` | `Kind`, SortOrder · (Name) |
| `StatItem` | `StatItemTranslation` | Value, Suffix, SortOrder · (Label) |
| `Credibility` | `CredibilityTranslation` | LogoMediaId, SortOrder · (Name) |
| `ActivityGroup` | `ActivityGroupTranslation` | SortOrder · (Name) |
| `Activity` | `ActivityTranslation` | ActivityGroupId, SortOrder · (Text) |
| `PageSection` | `PageSectionTranslation` | PageKey, SortOrder · (Heading, Body) |
| `ExperienceEntry` | `ExperienceEntryTranslation` | Start/EndDateUtc, SortOrder · (Role, Organization, Description, PeriodLabel) |
| `Course` | `CourseTranslation` | `Level`, Period, SortOrder · (CourseName, Institution, Description) |

### 1.5 Media, communication, SEO, statistics
`MediaFile` · `ContactMessage` · `Redirect` · `PageSeo` · `ContentEvent` · `PageView`.

> **Identity** (`ApplicationUser`, `IdentityRole`, etc.) is **not** in the domain — it is provided by ASP.NET Core Identity in `Infrastructure` (doc 03 §2.10), as wired in Stage 0.

---

## 2. Enum Inventory

`ProfAly.CMS.Domain.Enums` — 9 enums (explicit 1-based values for stable storage; Stage 2 chooses int vs string column):

| Enum | Values |
|---|---|
| `ContentType` | Book, Publication, ResearchPaper, Thesis, Project, Resource, EnrichmentItem, Video |
| `RelationshipType` | Supervised, Examined, Ongoing |
| `DegreeLevel` | Master, PhD |
| `CourseLevel` | Undergraduate, Graduate |
| `MembershipKind` | Society, Board |
| `ProjectStatus` | Ongoing, Completed |
| `MediaKind` | Image, Pdf, Thumbnail |
| `ThemeMode` | Light, Dark |
| `ContentEventType` | View, Download, Play |

---

## 3. Domain Support Types (Common)

| Type | Role |
|---|---|
| `BaseEntity` | integer surrogate key |
| `AuditableEntity` | + CreatedUtc / ModifiedUtc |
| `ITranslation` | marker for translation rows (`Culture`) |
| `IValidatableEntity` | exposes `Validate()` cross-field invariants |
| `SupportedCultures` | ar/en/fr set, default ar, RTL test (Stage 0) |
| `FieldLengths` | central max-length constants from doc 05 |
| `ContentRules` | format/range rules (YouTube id, DOI, hex colour, year bounds, redirect codes) |

**Value objects:** the architecture (docs 03–05) defines **none** — content is modeled with primitives + enums + translation rows. None were invented. *(Recorded as a deliberate non-deviation.)*

---

## 4. Relationship Summary

- `SiteSettings 1—* SiteSettingsTranslation`; `SiteSettings *—1 MediaFile` (logo)
- `Profile 1—* ProfileTranslation`; `Profile *—1 MediaFile` (photo); `ProfileTranslation *—1 MediaFile` (CV, per culture)
- `ContentItem 1—* ContentItemTranslation`; `*—1 MediaFile` (cover) and `*—1 MediaFile` (pdf); `1—* ContentEvent`
- `ContentItem *—* Category` via `ContentItemCategory`; `Category 1—* CategoryTranslation`
- `ActivityGroup 1—* Activity`; both `1—* *Translation`
- `Qualification / Skill / Membership / StatItem / Credibility / PageSection / ExperienceEntry / Course 1—* *Translation`
- `Credibility *—1 MediaFile` (logo)
- `Redirect`, `PageSeo`, `PageView` — standalone
- Navigation properties are defined on both sides; FK delete behaviours, uniqueness, and the TPH discriminator are **Stage 2** concerns.

---

## 5. Validation Summary

Two layers, both in the domain:

**(a) Cross-field invariants** via `IValidatableEntity.Validate()` (services call before persistence):
| Entity | Rule |
|---|---|
| `Profile` | DateOfBirth not in the future |
| `ContentItem` (base) | PublicationYear within [1900 … currentYear+1] |
| `Publication` / `ResearchPaper` | DOI matches DOI format (when present) |
| `Video` | valid 11-char YouTube id **and** no attached PDF |
| `ExperienceEntry` | EndDate ≥ StartDate (when both present) |
| `Redirect` | StatusCode ∈ {301,302}; From ≠ To |

**(b) Shared rules/constants** (`ContentRules`, `SupportedCultures`, `FieldLengths`) reused by entities, and later by EF config + FluentValidation, so all layers agree.

> Length limits and `Culture ∈ {ar,en,fr}` / enum CHECK constraints are recorded here as constants but **enforced physically in Stage 2** (EF) — they are persistence concerns, not domain invariants. 7 invariant tests + 7 foundation tests = **14 passing**.

---

## 6. Deviations / Clarifications vs. the Architecture

1. **Type-specific non-translatable columns live on the subtypes** (e.g. `Doi` on Publication, `YouTubeVideoId` on Video), not literally on the base class. Doc 03 describes them "kept on the base table under TPH" — that describes the **physical table**, which EF TPH produces identically. No schema difference; this is the OO-correct expression of the same model.
2. **Discriminator helper.** A get-only `ContentItem.ContentType` property returns the enum for code/queries; it is **not mapped** (no setter). The persisted `ContentType` discriminator column is configured in Stage 2 (EF `HasDiscriminator`). No conflict.
3. **Enum storage format deferred.** Enums carry explicit int values, but whether they persist as TEXT (matching doc 03's textual columns) or INTEGER is a **Stage 2 mapping decision** — flagged for that stage.
4. **`PageSeo`** carries `Culture` but is **not** an `ITranslation` (it has no parent entity; it is standalone, keyed by `(PageKey, Culture)`) — exactly as doc 03/05 describe.
5. **Identity entities are in Infrastructure, not Domain** (doc 03 §2.10) — intentional; the domain stays free of the auth framework.
6. **Helper base types** (`BaseEntity`, `AuditableEntity`) and the `ITranslation` / `IValidatableEntity` interfaces are implementation conveniences not named in the docs; they carry only the `Id`/timestamps the docs already specify and have **no schema impact**.

None of these change the data model defined in docs 03–05; they are faithful realizations of it.

---

## 7. Build / Test Evidence

```
dotnet build ProfAly.CMS.sln  → Build succeeded. 0 Warning(s), 0 Error(s)  (all net8.0)
dotnet test  ProfAly.CMS.sln  → Passed! Failed: 0, Passed: 14, Skipped: 0
```
Domain source files: **48** (entities, enums, common). No EF/DbContext/migration artifacts were added.

---

## 8. What Remains → Stage 2 (Persistence) — awaiting approval

Configure `AppDbContext`: register all entities + the TPH hierarchy (discriminator → `ContentType`), translation-table keys & `(ParentId,Culture)` uniqueness, slug uniqueness `(ContentType,Culture,Slug)`, FK delete behaviours (media `SET NULL`, translations/activities `CASCADE`), all indexes (doc 03 §5), culture + enum CHECK constraints, decide enum storage (TEXT recommended), then create the **initial EF Core migration** and generate the SQLite database (WAL/FK/busy_timeout pragmas).

**⏸ Stopping here as instructed. Awaiting approval before starting Stage 2.**
