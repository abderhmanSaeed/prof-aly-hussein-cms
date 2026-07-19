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
| Email | `AdminAccount:Email` in `appsettings.json` / `appsettings.Production.json` = `admin@aly-hussein.local` | applies in all environments |
| Password | `AdminAccount:Password` via **configuration binding** (`AdminAccountOptions`) | never hardcoded, never committed |

Password precedence (highest wins), per ASP.NET Core configuration order:

```
appsettings.json  <  appsettings.Production.json  <  [User Secrets — Development only]  <  Environment variables
```

- **Not hardcoded and never stored in source control** — the seeder skips creation (with a
  warning) if no password is configured.
- **Development:** the password comes from **User Secrets** (`AdminAccount:Password = Admin#2026Dev`).
- **Production:** the password comes **only** from the `AdminAccount__Password` **environment
  variable** (the server-side EnvironmentFile). No source-controlled file contains a password —
  `appsettings.Production.json` holds only the email.

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

## 6. The fix (minimum, safe change — env-var-only, no committed secret)

The bootstrap password is supplied **only** through the server-side environment variable
`AdminAccount__Password` (the systemd `EnvironmentFile` `/etc/profalycms/profalycms.env`). **No
source-controlled file contains a password.** `src/ProfAly.CMS.Web/appsettings.Production.json`
holds only the email:

```json
"AdminAccount": {
  "Email": "admin@aly-hussein.local"
}
```

The operator sets the password once, before the first deployment (the deployment guide marks it
**required**):

```bash
# /etc/profalycms/profalycms.env   (chmod 640, root:profalycms)
AdminAccount__Password=Admin#2026Dev
```

If the variable is **absent** on first startup, `SuperAdminSeeder` logs a clear warning
(*"AdminAccount:Password is not configured; skipping Super Admin seeding…"*) and creates **no**
admin — it never falls back to a hardcoded or committed password.

No application/business logic was changed. `SuperAdminSeeder` already enforced the skip-with-warning
behaviour and every safety requirement; only configuration and docs changed.

### After the fix — production startup

> **Will I be able to log in with `admin@aly-hussein.local` / `Admin#2026Dev`?**
>
> ### YES ✅ — once `AdminAccount__Password=Admin#2026Dev` is set in the env file (a required, documented first-deploy step).

**Why:** on first Production startup of an empty database, `AdminAccount:Password` resolves from the
`AdminAccount__Password` environment variable, so `SuperAdminSeeder` creates the account with that
password, in the `SuperAdmin` role. Identity's password hasher validates the credential on sign-in.
(User Secrets are irrelevant in Production; the secret never lives in source.)

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

Ran the **real application host** in `ASPNETCORE_ENVIRONMENT=Production` (with `--no-launch-profile`
so no Development override, and **no** User Secrets) against a fresh temp database, both with and
without the environment variable. EF `Information` command logs were suppressed (Production
`Warning` level), confirming the runs were genuinely in the Production environment.

**With `AdminAccount__Password=Admin#2026Dev` set (the documented first-deploy step):**

```
RoleSeeder        Created role 'SuperAdmin'.
SuperAdminSeeder  Created Super Admin 'admin@aly-hussein.local'.
```

→ admin created from the environment variable (no committed password involved).

**Without any `AdminAccount__Password` (misconfiguration):**

```
RoleSeeder        Created role 'SuperAdmin'.
SuperAdminSeeder  AdminAccount:Password is not configured; skipping Super Admin seeding.
                  Set it via the AdminAccount__Password environment variable or user-secrets.
```

→ the role is created but **no admin is seeded**, and a clear warning is logged. No hardcoded or
committed password is ever used.

---

## 9. Identity login / lockout / hashing / roles (verified)

Covered by `tests/ProfAly.CMS.Tests/AdminAccountSeedingTests.cs` (4 tests, all pass), which run the
real `AddInfrastructure` + `DatabaseInitializer` seeding path with the password supplied **only via
the `AdminAccount__Password` environment variable** (`AddEnvironmentVariables()`):

| Check | Result |
|-------|--------|
| Admin created from the **environment variable** on empty DB | ✅ |
| `CheckPasswordAsync(admin, "Admin#2026Dev")` (the exact check sign-in performs) | ✅ true |
| Admin is in role `SuperAdmin` | ✅ |
| Second startup: no duplicate, hash unchanged, original password still valid | ✅ |
| **No** env var (and no committed password) → admin skipped **with a warning** (asserted from captured logs) | ✅ |
| Lockout: `MaxFailedAccessAttempts=5`, `DefaultLockoutTimeSpan=15min`, `AllowedForNewUsers=true` | ✅ unchanged |
| Password policy: length 10, digit + uppercase + non-alphanumeric required | ✅ unchanged |
| Password hasher registered (Identity PBKDF2) | ✅ |

Login is protected by the same lockout and rate-limiting introduced in Stage 90; none of that was altered.

---

## Final login credentials

```
URL:      https://<your-domain>/account/login
Email:    admin@aly-hussein.local
Password: Admin#2026Dev   (set via AdminAccount__Password in the server-side env file)
```

Set `AdminAccount__Password=Admin#2026Dev` in `/etc/profalycms/profalycms.env` **before the first
startup**, then **rotate the password after the first login**. The password only ever seeds an empty
database — it is never re-applied to an existing account, and it is never stored in source control.

---

## Files modified

| File | Change |
|------|--------|
| `src/ProfAly.CMS.Web/appsettings.Production.json` | Holds **only** `AdminAccount:Email`. No password — a comment documents that the bootstrap password comes solely from the `AdminAccount__Password` env var. |
| `docs/91_Production_Deployment_Guide.md` | Env-file `AdminAccount__Password` is now **required** for the first deploy; clarified that it is the only password source and never stored in source. |
| `tests/ProfAly.CMS.Tests/AdminAccountSeedingTests.cs` | **New** — verifies the **environment-variable** seeding path, login credential, idempotency, the skip-with-warning behaviour, and Identity security settings. |
| `docs/92_Admin_Account_Verification.md` | **New** — this report. |

No source/business logic was changed. `SuperAdminSeeder`, `RoleSeeder`, `DatabaseInitializer`,
`Program.cs`, and the Identity configuration are untouched — only configuration/docs/tests.

---

## Why this is production-safe

- **No committed secret** — no source-controlled file contains an administrator password;
  `appsettings.Production.json` holds only the email. The password comes solely from the
  server-side `AdminAccount__Password` environment variable (EnvironmentFile, chmod 640).
- **Fails safe, never insecure** — if the env var is missing on first startup, the seeder logs a
  clear warning and creates no admin; it never falls back to a hardcoded or committed password.
- **No recreation / no overwrite** — creation is gated on `FindByEmailAsync`; existing users and
  their password hashes are never touched, even if the env-var value later changes.
- **No dependency on User Secrets** — those load only in Development; Production uses the env var.
- **Works on a clean Ubuntu VPS** — verified by real Production runs, both with the env var (admin
  created) and without it (skipped + warned).
- **Existing data untouched** — no `app.db` reset/delete; migrations are additive; a startup
  backup precedes them; all verification used throwaway temp databases.
- **Rotation path** — change the password from the admin UI after first login; the bootstrap value
  only ever seeds an empty database.
