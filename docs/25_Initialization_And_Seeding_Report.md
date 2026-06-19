# 25 — Initialization & Seeding Report (Stage 3)

**Phase:** Implementation — **Stage 3 (Database Initialization & Seed Infrastructure) only.**
**Date:** 2026-06-19
**Outcome:** ✅ Startup initialization pipeline + seed infrastructure implemented and **verified end-to-end** (DB auto-created, migrated, seeded; `/health` = 200). **Build 0/0 · Tests 14/14.**
**Boundaries honoured:** no admin pages, CRUD, content management, public pages, repositories, or application services. Only initialization/seed *infrastructure*.

---

## 1. Initialization Flow

Invoked once at startup in `Program.cs` (in a DI scope, before the pipeline serves traffic) via `DatabaseInitializer.RunAsync()`:

```
1. EnsureDataDirectoryExists  → parse SQLite "Data Source", create App_Data folder if missing
2. ApplyMigrationsAsync       → list pending migrations, then MigrateAsync (creates DB on first run)
3. ValidateConnectionAsync    → CanConnectAsync; throws if the DB is unreachable
4. RunSeedersAsync            → run all IDataSeeder in ascending Order (idempotent)
   ├─ (1) RoleSeeder          → SuperAdmin role
   ├─ (2) SuperAdminSeeder    → admin user from config (password from env/secrets)
   └─ (3) SiteSettingsSeeder  → SiteSettings singleton + ar/en/fr chrome
```

**Verified run log (first start):**
```
Database initialization starting.
Created database directory …\src\ProfAly.CMS.Web\App_Data.
Applying 1 pending migration(s): 20260619134122_InitialCreate
Applying migration '20260619134122_InitialCreate'.
Database connectivity validated.
Running seeder RoleSeeder (order 1).      → Created role 'SuperAdmin'.
Running seeder SuperAdminSeeder (order 2). → Created Super Admin 'admin@aly-hussein.local'.
Running seeder SiteSettingsSeeder (order 3).→ Seeded site settings (culture=ar, theme=Light).
Database initialization complete.
```

## 2. Seeded Entities

| Order | Seeder | Produces | Idempotency guard |
|---|---|---|---|
| 1 | `RoleSeeder` | `SuperAdmin` role (`AspNetRoles`) | `RoleExistsAsync` |
| 2 | `SuperAdminSeeder` | Super Admin user (`AspNetUsers`) + role membership | `FindByEmailAsync` |
| 3 | `SiteSettingsSeeder` | `SiteSettings` (Id 1) + 3 `SiteSettingsTranslation` rows (ar/en/fr) | `SiteSettings.AnyAsync` |

**Verified DB contents after first run:**
- `AspNetRoles` → `SuperAdmin`
- `AspNetUsers` → `admin@aly-hussein.local` (PasswordHash set; mapped to `SuperAdmin`)
- `SiteSettings` → `Id=1, DefaultCulture=ar, DefaultTheme=Light, ContactEmail=info@aly-hussein.local`
- `SiteSettingsTranslation` → `ar = أ. د. علي حسين`, `en = Prof. Aly Hussein`, `fr = Pr. Aly Hussein`

> **Default language & theme** are seeded onto `SiteSettings` (`DefaultCulture`, `DefaultTheme`) — tasks 8 and 9 are satisfied by the SiteSettings seeder, not separate tables.

## 3. Seeded Roles

A single role: **`SuperAdmin`** (constant `Roles.SuperAdmin`), created via `RoleManager<IdentityRole>`. This is the only principal/role in the single-admin model (doc 06 §7).

## 4. Startup Behavior

- **First run:** `App_Data/` is created, `app.db` is created and migrated, connectivity validated, all three seeders run, then the app serves traffic. WAL is active (pragma interceptor).
- **Subsequent runs:** no pending migrations → "ensuring database exists"; seeders detect existing data and skip (logged "already exists"). **Safe to run on every startup.**
- **Missing admin password:** the app still starts; `SuperAdminSeeder` logs a warning and skips user creation (no hardcoded fallback).
- **Unreachable DB:** initialization throws (fail-fast) so the app does not start in a broken state.

## 5. Health Checks

- `DatabaseHealthCheck : IHealthCheck` runs `Database.CanConnectAsync()`; registered as **`"database"`** (tag `ready`) in `AddInfrastructure`.
- Exposed at **`GET /health`** (`MapHealthChecks` in `Program.cs`). Verified returning **`200 Healthy`** with the database reachable.
- No extra NuGet package required (custom check using the shared framework).

## 6. Configuration Requirements

| Key | Source | Required? | Notes |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | appsettings | yes | `Data Source=App_Data/app.db` |
| `AdminAccount:Email` | appsettings | for admin seeding | `admin@aly-hussein.local` (placeholder) |
| `AdminAccount:Password` | **env var / user-secrets** | for admin seeding | **never in appsettings**; `AdminAccount__Password=…`. Must satisfy the policy (≥10 chars, upper+lower+digit+symbol). If absent → admin creation skipped with a warning. |
| `Localization:DefaultCulture` | appsettings | no (defaults `ar`) | drives seeded `DefaultCulture` |
| `SiteSettings:DefaultTheme` | appsettings | no (defaults `Light`) | drives seeded `DefaultTheme` |
| `SiteSettings:ContactEmail` | appsettings | no | falls back to `AdminAccount:Email` |

**Dev setup example:** `dotnet user-secrets set "AdminAccount:Password" "<StrongP@ss1>"` (or set the `AdminAccount__Password` environment variable).

## 7. Files Added / Changed

**Added:** `Identity/AdminAccountOptions.cs`; `Persistence/HealthChecks/DatabaseHealthCheck.cs`; `Persistence/Seeding/Seeders/{RoleSeeder, SuperAdminSeeder, SiteSettingsSeeder}.cs`.
**Changed:** `Persistence/Seeding/IDataSeeder.cs` (DI-based signature), `Persistence/Seeding/DatabaseInitializer.cs` (full pipeline), `Infrastructure/DependencyInjection.cs` (options + seeders + health check), `Web/Program.cs` (startup invocation; health-check registration moved to infra), `Web/appsettings.json` (AdminAccount + SiteSettings defaults).

## 8. Deviations from Architecture

1. **`IDataSeeder` signature changed** from the Stage-2 scaffold (`SeedAsync(AppDbContext, …)`) to a **dependency-injected** form (`SeedAsync(CancellationToken)`; deps via constructor). Required so seeders can use `RoleManager`/`UserManager`, not just the DbContext. The `DatabaseInitializer` was updated to match. No schema impact.
2. **Default language & theme are seeded on the `SiteSettings` singleton** (not as separate "configuration" tables) — consistent with doc 03 §2.1, which models these as `SiteSettings.DefaultCulture` / `DefaultTheme`.
3. **Health-check registration moved into `AddInfrastructure`** (the standalone `AddHealthChecks()` was removed from `Program.cs`) so DB-connectivity wiring lives with the persistence layer. The endpoint mapping stays in the web host.
4. **Profile singleton is not seeded** in this stage — only application settings (per the Stage-3 scope). Profile/content seeding belongs to the later content-migration step (doc 14 Phase 3b).
5. **Admin account is skipped (not failed) when no password is configured** — a deliberate "configurable, never hardcoded" choice; the app still boots.

None of these alter the data model; they are initialization/wiring decisions.

## 9. Verification

```
dotnet build      → 0 warnings / 0 errors (net8.0)
dotnet test       → 14/14 passed
app first run     → App_Data created, migration applied, 3 seeders ran, /health = 200 Healthy
DB inspection     → SuperAdmin role; admin user (hashed pwd) in role; SiteSettings(ar,Light) + 3 chrome rows; 19 CHECKs; WAL
teardown          → throwaway App_Data deleted (no DB committed)
```

**⏸ Stage 3 complete. Stopping here as instructed — awaiting approval.** (Stage 3 changes are uncommitted; say the word to create the checkpoint.)
