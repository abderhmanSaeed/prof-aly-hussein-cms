# 24 — Persistence Git Report

**Phase:** Source-control checkpoint after Stage 2 (Persistence).
**Date:** 2026-06-19
**Outcome:** ✅ Stage 2 committed, pushed, and tagged `v0.2-persistence`. Working tree clean and in sync with `origin/main`.

---

## 1. Commit

| Item | Value |
|---|---|
| **Subject** | `Persistence Layer completed` |
| **Commit hash** | `31c25d5b71d8c03726d12f5ea91f6d2ee74c40bb` |
| **Short hash** | `31c25d5` |
| **Branch** | `main` |
| **Parent** | `23ca754` (docs: add Git milestone report (22)) |
| **Author** | Abd Elrhman Saeed &lt;abderhmansaeed2020@gmail.com&gt; |
| **Co-author** | Claude Opus 4.8 (1M context) |

## 2. Tag

| Item | Value |
|---|---|
| **Tag** | `v0.2-persistence` (annotated) |
| **Tag object SHA** | `3da496c34e5ff4cf107b526db6d9715bffa9ae85` |
| **Peeled commit** (`^{}`) | `31c25d5b71d8c03726d12f5ea91f6d2ee74c40bb` |
| **Message** | "Milestone: Persistence Layer (Stage 2) complete" |
| **Tag URL** | https://github.com/abderhmanSaeed/prof-aly-hussein-cms/releases/tag/v0.2-persistence |

## 3. Push Status

✅ **Both pushed successfully.**
```
# branch
   23ca754..31c25d5  main -> main
# tag
 * [new tag]         v0.2-persistence -> v0.2-persistence
```
Remote verification (`git ls-remote --tags origin`):
```
3da496c…  refs/tags/v0.2-persistence
31c25d5…  refs/tags/v0.2-persistence^{}   (== commit)
```
`git status` → `## main...origin/main` (0 ahead / 0 behind).

## 4. Changed Files Summary

**28 files** — 11 modified, 17 added. No databases, `App_Data/`, or build artifacts were tracked.

### Added (17)
| Path | Purpose |
|---|---|
| `.config/dotnet-tools.json` | pinned local `dotnet-ef` 8.0.11 |
| `…/Persistence/AppDbContextFactory.cs` | design-time context factory |
| `…/Persistence/Interceptors/SqlitePragmaInterceptor.cs` | WAL/FK/synchronous/busy_timeout |
| `…/Persistence/Seeding/IDataSeeder.cs` | seed contract (scaffolding) |
| `…/Persistence/Seeding/DatabaseInitializer.cs` | migrate + run seeders |
| `…/Persistence/Configurations/` (7 files) | entity mappings (Settings, Profile, Content, Category, ProfilePage, PageContent, Infrastructure) |
| `…/Persistence/Migrations/` (3 files) | `InitialCreate` + Designer + model snapshot |
| `docs/23_Persistence_Report.md` | Stage 2 report |

### Modified (11)
| Path | Change |
|---|---|
| `…/Persistence/AppDbContext.cs` | DbSets, enum→TEXT conventions, TPH discriminator, culture-CHECK loop |
| `…/Infrastructure/DependencyInjection.cs` | register pragma interceptor + `DatabaseInitializer` |
| `…/Domain/Entities/Content/*.cs` (9 files) | discriminator-via-constructor refinement (`ContentItem` + 8 subtypes) |
| `.editorconfig` | add CA1725 to convention suppressions |

## 4a. Migration Files Summary

| File | Lines | Role |
|---|---|---|
| `Migrations/20260619134122_InitialCreate.cs` | 1,342 | `Up()` creates all tables, FKs, indexes, CHECKs; `Down()` drops them |
| `Migrations/20260619134122_InitialCreate.Designer.cs` | 1,990 | migration's model snapshot (target model) |
| `Migrations/AppDbContextModelSnapshot.cs` | 1,987 | current cumulative model snapshot |

- Migration id: **`20260619134122_InitialCreate`**.
- Created with the pinned local **`dotnet-ef` 8.0.11**; design-time context resolved via `AppDbContextFactory`.
- Verified-applicable against SQLite (`database update` succeeded, WAL active); the throwaway DB was deleted — **no database committed**.

## 4b. Database Schema Summary

| Metric | Value |
|---|---|
| Tables created by migration | **43** (35 domain + 7 ASP.NET Identity + 1 `__EFMigrationsHistory`) |
| Content model | 1 TPH table `ContentItem` (string discriminator `ContentType`, 8 subtypes) |
| Translation tables | 14 (each unique on `(ParentId, Culture)` + culture CHECK) |
| Indexes | 51 (22 unique) |
| Foreign keys | 30 (media FKs `SET NULL`; translations/joins/events `CASCADE`) |
| CHECK constraints | 19 (17 culture + `DefaultCulture` + `Redirect.StatusCode`) |
| Enum storage | TEXT (all enums + discriminator) |
| Pragmas | WAL, `foreign_keys=ON`, `synchronous=NORMAL`, `busy_timeout=5000` |

Full detail in `23_Persistence_Report.md`.

## 4c. Recommendations

1. **Runtime must create `App_Data/` before opening SQLite** — SQLite does not create the parent folder (observed as Error 14). This is implemented in Stage 3's initializer.
2. **Keep the local EF tool pinned** (`.config/dotnet-tools.json`) and run `dotnet tool restore` on new machines/CI so migrations stay reproducible against EF 8.
3. **Do not commit databases** — `App_Data/` and `*.db*` remain git-ignored; backups are a separate operational concern (doc 12).
4. **Next schema change = a new named migration**, never hand-edits; review the generated `Up/Down` before applying.

## 5. Repository State

| Item | Value |
|---|---|
| Branch | `main` (in sync with `origin/main`) |
| HEAD | `31c25d5` |
| Tags | `v0.1-foundation-domain`, `v0.2-persistence` |
| Build | 0 warnings / 0 errors (net8.0) |
| Tests | 14/14 passing |

## 6. Notes

- This report (`24_…`) is committed as a small follow-up `docs:` commit; the `v0.2-persistence` tag remains anchored to `31c25d5` and does not move.
- No database is committed — `App_Data/`, `*.db`, and the WAL/SHM sidecars stay git-ignored.

**⏸ Persistence checkpoint published. Stopping here as instructed — awaiting approval before the next stage.**
