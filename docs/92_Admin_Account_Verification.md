# 92 — Administrator Account Verification

**Application:** Prof. Aly Hussein CMS (ASP.NET Core 8 Razor Pages + SQLite)
**Purpose:** Verify how the Super Admin is created and guarantee login works on the first
production deployment.
**Date:** 2026-07-19
**Result:** Fixed. Build clean · 42/42 tests pass · verified at runtime in Production.

---

## 1. How the admin account is created

Creation happens through the **startup seeding pipeline**, not a migration, hosted service,
or `Program.cs` inline code:

```
Program.cs  →  DatabaseInitializer.RunAsync()   (runs unconditionally on every startup)
                 ├─ ensure data dir + startup backup
                 ├─ apply EF migrations (creates app.db on first run)
                 ├─ validate connectivity
                 └─ run IDataSeeder implementations in order:
                      1. RoleSeeder          → creates the "SuperAdmin" role
                      2. SuperAdminSeeder    → creates the admin user  ← ADMIN CREATED HERE
                      3. SiteSettingsSeeder
                    100. StaticContentImporter
```

- `src/ProfAly.CMS.Infrastructure/Persistence/Seeding/DatabaseInitializer.cs` — orchestrates.
- `src/ProfAly.CMS.Infrastructure/Persistence/Seeding/Seeders/SuperAdminSeeder.cs` — creates the admin.
- `src/ProfAly.CMS.Infrastructure/Persistence/Seeding/Seeders/RoleSeeder.cs` — creates the role.

**Idempotency / safety (unchanged, already correct):** `SuperAdminSeeder` calls
`UserManager.FindByEmailAsync(email)` first. If the user already exists it only ensures the role
membership and returns — it **never** re-creates, re-hashes, or resets the password of an
existing account. So:
- Admin is created automatically **when the database has no such user** (i.e. first run).
- Runs in **every environment** (Development *and* Production) — not Development-only.
- Created **only once**; subsequent startups are a no-op for the existing account.
- Stored inside **`app.db`** (the ASP.NET Core Identity `AspNetUsers` table).

---

## 2. Where the credentials come from

| Value | Source | Notes |
|-------|--------|-------|
| Email | `AdminAccount:Email` in `appsettings.json` = `admin@aly-hussein.local` | applies in all environments |
| Password | `AdminAccount:Password` via **configuration binding** (`AdminAccountOptions`) | never hardcoded |

Password precedence (highest wins), per ASP.NET Core configuration order:

```
appsettings.json  <  appsettings.Production.json  <  [User Secrets — Development only]  <  Environment variables
```

- **Not hardcoded** — the seeder skips creation (with a warning) if no password is configured.
- **User Secrets** hold the password in **Development** only (`AdminAccount:Password = Admin#2026Dev`).
- In **Production**, `AdminAccount__Password` (environment variable) overrides everything if set.

---

## 3. Are User Secrets required?

**No — and they must not be relied on in Production.** ASP.NET Core loads User Secrets **only when
the environment is Development** (`WebApplication.CreateBuilder` adds them only then). On a
production server the User-Secrets store is never read, so the dev password `Admin#2026Dev` stored
there is invisible to the running app.

---

## 4. First deployment behavior (the problem that was found)

On a brand-new Ubuntu server where **`app.db` does not exist, User Secrets do NOT exist, and the
environment is Production**, the *original* behavior was:

1. Migrations create `app.db`.
2. `RoleSeeder` creates the `SuperAdmin` role.
3. `SuperAdminSeeder` reads `AdminAccount:Password` from configuration → **finds nothing**
   (appsettings has no password, `appsettings.Production.json` had no password, User Secrets not
   loaded, and no env var was guaranteed to be set).
4. Seeder logs *“AdminAccount:Password is not configured; skipping Super Admin seeding.”* →
   **no admin user is created at all.**

### 5. Simulated production startup — original code

> **Will I be able to log in with `admin@aly-hussein.local` / `Admin#2026Dev`?**
>
> ### NO ❌ (before the fix)

**Why:** the password lived only in User Secrets, which Production does not load. With no password
in any production configuration source, the seeder skipped admin creation entirely — there was no
account to log in as. (And even if an operator set `AdminAccount__Password` to some other value per
the Stage 91 template, the seeded password would be *that* value, not `Admin#2026Dev`.)

---

## 6. The fix (minimum, safe change)

Provide the intended first-run password through **production configuration** instead of User
Secrets, exactly as requested. Added an `AdminAccount` section to
`src/ProfAly.CMS.Web/appsettings.Production.json`:

```json
"AdminAccount": {
  "Email": "admin@aly-hussein.local",
  "Password": "Admin#2026Dev"
}
```

This is the **default used only to seed an empty database on first startup**. It is still
overridable by the `AdminAccount__Password` environment variable (which takes precedence), so
operators can supply/rotate the secret via the systemd `EnvironmentFile` without editing source.

No application/business logic was changed. The `SuperAdminSeeder` already enforced every safety
requirement; only configuration was added.

### After the fix — simulated production startup

> **Will I be able to log in with `admin@aly-hussein.local` / `Admin#2026Dev`?**
>
> ### YES ✅

**Why:** on first Production startup of an empty database, `AdminAccount:Password` now resolves to
`Admin#2026Dev` from `appsettings.Production.json` (User Secrets not required, no env var required),
so `SuperAdminSeeder` creates the account with that password, in the `SuperAdmin` role. Identity's
password hasher validates the credential on sign-in.

---

## 7. Existing-database behavior (safety guarantees)

Verified by test `SecondStartup_DoesNotDuplicate_Reset_OrOverwrite_TheExistingAdmin`:

- If the admin already exists, the seeder **does nothing** to it — no duplicate user.
- The **password hash is not regenerated** (byte-for-byte identical after a restart).
- **Existing passwords are never reset**, even if the configured password later changes — the
  original password keeps working and the new config value is ignored for the existing user.
- `app.db` is never deleted or reset; migrations are additive; a startup backup runs first.
- The real `App_Data/app.db` in this repo was **not modified** during verification (all tests and
  the runtime simulation used throwaway temp databases).

---

## 8. Fresh-server behavior (verified at runtime)

Ran the **real application host** in `ASPNETCORE_ENVIRONMENT=Production` (with
`--no-launch-profile` so no Development override, and **no** `AdminAccount__Password` env var and
**no** User Secrets) against a fresh temp database. Log excerpt:

```
DatabaseInitializer  Applying 8 pending migration(s): ...
RoleSeeder           Created role 'SuperAdmin'.
SuperAdminSeeder     Created Super Admin 'admin@aly-hussein.local'.
DatabaseInitializer  Database initialization complete.
```

EF `Information` command logs were suppressed (Production `Warning` level), confirming the run was
genuinely in the Production environment. The admin was created — proving the password was sourced
from `appsettings.Production.json`, not User Secrets.

---

## 9. Identity login / lockout / hashing / roles (verified)

Covered by `tests/ProfAly.CMS.Tests/AdminAccountSeedingTests.cs` (4 tests, all pass), which run the
real `AddInfrastructure` + `DatabaseInitializer` seeding path:

| Check | Result |
|-------|--------|
| Admin created from config on empty DB | ✅ |
| `CheckPasswordAsync(admin, "Admin#2026Dev")` (the exact check sign-in performs) | ✅ true |
| Admin is in role `SuperAdmin` | ✅ |
| Second startup: no duplicate, hash unchanged, original password still valid | ✅ |
| No configured password → admin skipped (documents the old failure) | ✅ |
| Lockout: `MaxFailedAccessAttempts=5`, `DefaultLockoutTimeSpan=15min`, `AllowedForNewUsers=true` | ✅ unchanged |
| Password policy: length 10, digit + uppercase + non-alphanumeric required | ✅ unchanged |
| Password hasher registered (Identity PBKDF2) | ✅ |

Login is protected by the same lockout and rate-limiting introduced in Stage 90; none of that was altered.

---

## Final login credentials

```
URL:      https://<your-domain>/account/login
Email:    admin@aly-hussein.local
Password: Admin#2026Dev
```

**Rotate the password after the first login** (or override it before first boot via the
`AdminAccount__Password` environment variable in `/etc/profalycms/profalycms.env`). The default in
`appsettings.Production.json` only ever seeds an empty database — it is never re-applied to an
existing account.

---

## Files modified

| File | Change |
|------|--------|
| `src/ProfAly.CMS.Web/appsettings.Production.json` | Added `AdminAccount` (email + first-run password) so Production can seed the admin without User Secrets. |
| `docs/91_Production_Deployment_Guide.md` | Env-file `AdminAccount__Password` is now an optional override (commented); clarified first-run password source + rotation. |
| `tests/ProfAly.CMS.Tests/AdminAccountSeedingTests.cs` | **New** — verifies the production seeding path, login credential, idempotency, and Identity security settings. |
| `docs/92_Admin_Account_Verification.md` | **New** — this report. |

No source/business logic was changed. `SuperAdminSeeder`, `RoleSeeder`, `DatabaseInitializer`,
`Program.cs`, and the Identity configuration are untouched.

---

## Why this is production-safe

- **Admin always exists after first startup** — the password now has a guaranteed Production
  source (`appsettings.Production.json`), so seeding never silently skips on a clean VPS.
- **No recreation / no overwrite** — creation is gated on `FindByEmailAsync`; existing users and
  their password hashes are never touched.
- **No dependency on User Secrets** — the production password comes from production configuration
  (or the `AdminAccount__Password` env var), never from the dev-only secret store.
- **Works on a clean Ubuntu VPS** — verified by a real Production run with no env var and no User
  Secrets present.
- **Existing data untouched** — no `app.db` reset/delete; migrations are additive; a startup
  backup precedes them; verification used only temp databases.
- **Override + rotation path** — the env var overrides the default before first boot, and the
  password can be changed from the admin UI afterwards.

> Security note: the default password lives in `appsettings.Production.json`, which is a tracked
> file. Treat it as a bootstrap credential: prefer setting `AdminAccount__Password` in the
> server-side env file (chmod 640) and/or rotate immediately after the first login.
