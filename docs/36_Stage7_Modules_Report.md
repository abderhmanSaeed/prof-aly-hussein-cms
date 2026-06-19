# 36 — Stage 7 Modules & Content Import Report

**Phase:** Implementation — **Stage 7 (Experience, Teaching, Theses, Activities)** + **static-content import infrastructure**.
**Date:** 2026-06-19
**Outcome:** ✅ Four admin modules implemented; the legacy `data.js` content imports automatically and **verified — all counts match `35_Content_Migration_Plan.md`**. **Build 0/0 · Tests 14/14.**
**Boundaries honoured:** no public website.

---

## 1. Admin Modules Implemented

| Module | Pages | Notes |
|---|---|---|
| **Experience** | `Experience/Index` + `Upsert` | Start/End dates + `PeriodLabel`; Role/Organization/Description (AR/EN/FR); reorder/delete |
| **Teaching** | `Teaching/Index` + `Upsert` | `Level` (Undergraduate/Graduate) + Period; CourseName/Institution/Description; reorder within level |
| **Theses** | `Theses/Index` + `Upsert` | Tabular admin with **facet filters** (Relationship, Degree); ResearcherName + RelationshipType + DegreeLevel + Year; Title/Summary/Body; optional PDF; publish toggle |
| **Activities** | `Activities/Index` (groups) + `GroupUpsert` + `Items` + `ItemUpsert` | Two-level: Activity Groups → Activities; trilingual; reorder/delete at both levels |

All use the existing admin shell (Bootstrap 5, RTL/LTR), `RequireSuperAdmin`, localized validation (`SharedResource`), and the established translation-tab pattern. Sidebar wired for Experience/Teaching/Theses/Activities (active-state highlighting).

## 2. Content Import Infrastructure

- **Source → JSON:** `data.js` was evaluated with Node and serialized to **`Infrastructure/Persistence/Seeding/Data/static-content.json`** (embedded resource).
- **Importer:** `StaticContentImporter : IDataSeeder` (Order 100) reads the embedded JSON and maps every dataset to entities (per doc 35). It is:
  - **Config-gated:** runs only when `Seed:ImportStaticContent = true` (default false).
  - **Idempotent:** each dataset imports only if its target table is empty.
  - **Slug-safe:** generates URL-safe slugs, unique per culture across all content.
  - **Fallback-aware:** AR + EN translations only; French left empty.
- Wired into the existing `DatabaseInitializer` seeder pipeline (after Roles/SuperAdmin/SiteSettings).

**To import:** set `Seed__ImportStaticContent=true` (env/user-secrets/appsettings) and run — the initializer imports on startup.

## 3. Import Verification (counts match plan exactly)

Ran with the flag on (throwaway DB), then verified:

| Entity | Imported | Plan |
|---|---|---|
| Profile | 1 (DOB → 1966-02-24) | 1 |
| StatItem · Credibility · Qualification · Skill | 5 · 5 · 4 · 5 | ✓ |
| Membership · ExperienceEntry · Course | 10 · 8 · 16 | ✓ |
| ActivityGroup · Activity | 5 · 26 | ✓ |
| Book · Publication · Thesis | 14 · 9 · 57 | ✓ |
| Featured books | 3 | ✓ |
| Theses: Supervised / Examined / Ongoing | 22 / 33 / 2 | ✓ |
| ContentItemTranslation rows | 160 | ✓ (ar+en) |
| Slug duplicates per culture | 0 | ✓ |

Then re-ran the app **without** the flag and confirmed all four admin pages load against the imported data (`/Admin/Experience`, `/Teaching`, `/Theses` (+facet filter), `/Activities` (+`Items?groupId`)) → all `200`. Throwaway DB deleted; **no DB committed**.

## 4. Files Added / Changed

**Added:** `Experience/`, `Teaching/`, `Theses/`, `Activities/` admin pages (Index/Upsert sets; Activities adds GroupUpsert/Items/ItemUpsert); `Infrastructure/Persistence/Seeding/Seeders/StaticContentImporter.cs`; `Infrastructure/Persistence/Seeding/Data/static-content.json`; `docs/35_Content_Migration_Plan.md`, `docs/36_…`.
**Changed:** `SharedResource.{neutral,ar,fr}.resx` (Stage-7 field/label keys); `Infrastructure/DependencyInjection.cs` (register importer); `Infrastructure.csproj` (embed JSON); `Web/appsettings.json` (`Seed:ImportStaticContent`); `_Sidebar.cshtml` (wire modules).

## 5. Deviations / Notes

1. **Theses get a dedicated tabular admin** (not the shared `/Admin/Content` screen) because their fields (ResearcherName/RelationshipType/DegreeLevel) and table-style UX differ — matches doc 06 §4 / doc 16.
2. **Activities are two-level** (Group → items) with its own sub-pages.
3. **Import is gated + idempotent**, not auto-run, so dev runs stay predictable; flip the flag to import. French content empty (fallback). No media/categories in the source → none created on import.
4. **No repository/service layer** — PageModels use `AppDbContext` directly (consistent with Stages 5–6); the importer is an infrastructure `IDataSeeder`.
5. Theses imported are **published** and have no cover/PDF (none in source); admin can attach PDFs later.

## 6. Verification

```
dotnet build → 0 warnings / 0 errors (net8.0)
dotnet test  → 14/14 passed
Import (Seed:ImportStaticContent=true): all entity counts == plan; 0 slug dupes; DOB parsed
Admin pages (Experience/Teaching/Theses/Activities + facets/sub-pages) → 200
Throwaway App_Data deleted; no DB committed
```

**⏸ Stage 7 complete (modules + import infrastructure). Stopping here as instructed — awaiting approval.** (Changes uncommitted; the `static-content.json` is source data and intended to be committed.)
