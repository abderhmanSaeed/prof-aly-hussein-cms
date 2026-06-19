# 23 — Persistence Report (Stage 2)

**Phase:** Implementation — **Stage 2 (Persistence Layer) only** (per `15`/`18`/`03`/`04`/`05`).
**Date:** 2026-06-19
**Outcome:** ✅ `AppDbContext` fully mapped; **initial migration `InitialCreate` created and verified-applicable** against SQLite. **Build 0 warnings / 0 errors · Tests 14/14.**
**Boundaries honoured:** no repositories, services, Razor Pages, or admin UI. Seed *infrastructure* only (no seed data).

> The migration was applied once to a throwaway database purely to validate the DDL (WAL confirmed active), then that database was deleted — `App_Data/` is git-ignored and no DB is committed.

---

## 1. Entity Mappings Summary

Mappings live in `Infrastructure/Persistence/Configurations/` (10 grouped files, one `IEntityTypeConfiguration<T>` per entity) and are applied via `ApplyConfigurationsFromAssembly`. Cross-cutting rules live in `AppDbContext`.

| Area | Entities mapped |
|---|---|
| Settings | `SiteSettings` (+Translation) |
| Profile | `Profile` (+Translation, per-culture `CvFile` FK) |
| Content (TPH) | `ContentItem` base + `Book`, `Publication`, `ResearchPaper`, `Thesis`, `Project`, `Resource`, `EnrichmentItem`, `Video` + `ContentItemTranslation` + `ContentItemCategory` (join) |
| Taxonomy | `Category` (+Translation) |
| Profile-page lists | `Qualification`, `Skill`, `Membership`, `StatItem`, `Credibility`, `ActivityGroup`, `Activity` (each +Translation) |
| Page content | `PageSection`, `ExperienceEntry`, `Course` (each +Translation) |
| Media / Comms | `MediaFile`, `ContactMessage` |
| SEO | `Redirect`, `PageSeo` |
| Statistics | `ContentEvent`, `PageView` |
| Identity | `ApplicationUser` + ASP.NET Identity schema (from `IdentityDbContext`) |

**Conventions applied centrally:**
- All string properties length-bounded from `FieldLengths` (doc 05); required/optional per the entity catalogue.
- Every `Culture` column → required, `MaxLength(8)`, and a `CHECK (Culture IN ('ar','en','fr'))`.
- Translatable type-specific content fields (`Journal`, `Authors`, `Publisher`, `AuthorshipRole`, `ResearcherName`) mapped on `ContentItemTranslation`.

## 2. TPH ContentItem Hierarchy

- One physical table **`ContentItem`** with a string discriminator column **`ContentType`**.
- Discriminator is the **real `ContentType` enum property** (set by each subtype's constructor), used via `HasDiscriminator(e => e.ContentType).HasValue<Book>(ContentType.Book) …` and stored as TEXT (`Book`, `Publication`, …, `Video`).
- The discriminator is established in `OnModelCreating` **before** the assembly configuration scan, so subtypes join the hierarchy before their per-type configs run (this is what fixed the design-time `HasValue` failure — see §7).
- Type-specific non-translatable columns live on the subtypes and are flattened into the single table: `Doi` (Publication/Research), `DegreeLevel` + `RelationshipType` (Thesis), `ProjectStatus` + `Role` (Project), `ResourceType` (Resource/Enrichment), `YouTubeVideoId` (Video).

## 3. Table Count

| Group | Count |
|---|---|
| Domain tables | **35** (incl. the single TPH `ContentItem` table) |
| ASP.NET Identity tables | 7 |
| `__EFMigrationsHistory` | 1 |
| **Total created by migration** | **43** |

(The applied SQLite DB also shows `sqlite_sequence`, an internal table.)

## 4. Index Count

| Metric | Count |
|---|---|
| Explicit indexes created (migration script) | **51** |
| of which **unique** | **22** |

Highlights: unique `(ParentId, Culture)` on **all 14 translation tables**; unique `(Culture, Slug)` on `ContentItemTranslation` and `CategoryTranslation`; unique `MediaFile.StoredFileName`, `Redirect.FromPath`, `PageSection.PageKey`, `PageSeo (PageKey, Culture)`. Performance indexes: `ContentItem(ContentType, IsPublished, SortOrder)`, `ContentItem(ContentType, IsFeatured, IsPublished)`, `Thesis(RelationshipType, DegreeLevel)`, `ContentEvent(ContentItemId, EventType, CreatedUtc)`, `ContactMessage(IsRead, CreatedUtc)`, `MediaFile(MediaKind, CreatedUtc)`, `PageView(CreatedUtc)`, and `SortOrder`/grouping indexes on every list entity.

## 5. Constraint Summary

| Constraint type | Count / detail |
|---|---|
| **Primary keys** | one per table; composite PK on `ContentItemCategory (ContentItemId, CategoryId)` |
| **Foreign keys** | **30** |
| **CHECK constraints** | **19** — 17 culture checks (14 translation tables + `PageSeo`, `ContentEvent`, `PageView`) + `SiteSettings.DefaultCulture` + `Redirect.StatusCode IN (301,302)` |
| **Unique indexes** | 22 (see §4) |

**Delete behaviors:**
- **Cascade** — translations → parent; `Activity` → `ActivityGroup`; `ContentEvent` → `ContentItem`; `ContentItemCategory` → both `ContentItem` and `Category`.
- **Set null** — all media FKs (`ContentItem.CoverImage`/`PdfFile`, `Profile.Photo`, `ProfileTranslation.CvFile`, `SiteSettings.Logo`, `Credibility.Logo`) so deleting a file never deletes content.

**Enum storage:** all enums stored as **TEXT** (readable) via `ConfigureConventions` (`RelationshipType`, `DegreeLevel`, `CourseLevel`, `MembershipKind`, `ProjectStatus`, `MediaKind`, `ThemeMode`, `ContentEventType`, and the `ContentType` discriminator).

## 6. Migration Summary

| Item | Value |
|---|---|
| Migration | `20260619134122_InitialCreate` |
| Files | `InitialCreate.cs` (1,342 lines), `InitialCreate.Designer.cs`, `AppDbContextModelSnapshot.cs` |
| Location | `src/ProfAly.CMS.Infrastructure/Persistence/Migrations/` |
| Tooling | local `dotnet-ef` **8.0.11** pinned in `.config/dotnet-tools.json` (matches EF runtime) |
| Design-time | `AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>` (avoids building the web host) |
| Apply test | `database update` succeeded; 43 tables, 19 CHECKs, `journal_mode=wal` confirmed; DB then deleted |

> **Not applied to a committed database.** Creating/applying at runtime is a later stage; `App_Data/` and `*.db` remain git-ignored.

## 7. SQLite Considerations

- **Pragmas** via `SqlitePragmaInterceptor` on every opened connection: `journal_mode=WAL`, `foreign_keys=ON`, `synchronous=NORMAL`, `busy_timeout=5000` (doc 03 §7). WAL verified active on the test DB.
- **Enums as TEXT** for human-readable rows and stable diffs.
- **No idempotent SQL script** — SQLite doesn't support EF's idempotent script generation; migrations are applied via `database update`/`Migrate()` instead.
- **Directory pre-creation required:** SQLite does not create the parent folder of the database file. The runtime initializer (Stage 3) must ensure the `App_Data` directory exists before opening the connection (observed as SQLite Error 14 when the folder was absent).
- **Tooling version match:** the global `dotnet-ef` (10.x) could not load the EF 8 design assembly (NRE); pinning a local 8.0.11 tool resolved it and makes migrations reproducible.

## 8. Deviations from Architecture

1. **Discriminator is a real `ContentType` enum property** (set in subtype constructors) rather than the Stage-1 get-only helper. This was required: a get-only CLR property named `ContentType` collided with the desired `"ContentType"` discriminator column (NRE at design time). The property now serves both as the readable type and the persisted TEXT discriminator — column name and values match the architecture exactly. *(Minor Stage-1 refinement; domain tests unchanged and passing.)*
2. **Slug uniqueness is enforced per `(Culture, Slug)`** at the database level (globally unique per culture, routing-safe) rather than per `(ContentType, Culture, Slug)`. A single SQLite index cannot span the parent's discriminator; the doc's per-type intent (which is *less* strict) can be relaxed later in the application layer if ever needed. Documented in `ContentItemTranslation` config.
3. **No DB-generated defaults for `SiteSettings.DefaultTheme` and `Redirect.IsActive`/`StatusCode`** — these come from the domain CLR initializers. EF's sentinel behavior would otherwise make `DefaultTheme` ambiguous and prevent storing `IsActive = false`. CHECK constraints still enforce valid `StatusCode`/culture values.
4. **Culture CHECK applied to all culture-bearing tables** (incl. `PageSeo`, `ContentEvent`, `PageView`), slightly broader than doc 03's "translation tables only" — uniform and safe since the app only ever writes supported cultures.

None of these change the architecture's intended schema; they are faithful, well-justified realizations of it.

## 9. Files Added/Changed (Stage 2)

**Added:** `Persistence/Configurations/*` (10 files), `Persistence/Interceptors/SqlitePragmaInterceptor.cs`, `Persistence/Seeding/{IDataSeeder, DatabaseInitializer}.cs`, `Persistence/AppDbContextFactory.cs`, `Persistence/Migrations/*` (3 files), `.config/dotnet-tools.json`.
**Changed:** `AppDbContext.cs` (DbSets, conventions, discriminator, culture loop), `Infrastructure/DependencyInjection.cs` (interceptor + initializer), the 9 content entity files (discriminator-via-constructor refinement), `.editorconfig` (CA1725 convention).

## 10. Verification

```
dotnet build  → 0 warnings / 0 errors (net8.0)
dotnet test   → 14/14 passed
dotnet ef migrations add InitialCreate  → Done (43 tables modelled)
dotnet ef database update (throwaway)   → applied; WAL on; 19 CHECKs; DB deleted
```

**⏸ Persistence complete. Stopping here as instructed — awaiting approval before the next stage.**
