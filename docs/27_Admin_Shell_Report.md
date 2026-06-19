# 27 — Admin Shell Report (Stage 4)

**Phase:** Implementation — **Stage 4 (Authentication UI & Admin Shell) only.**
**Date:** 2026-06-19
**Outcome:** ✅ Login/logout/access-denied, the admin layout (sidebar + topbar), theme & language switchers, and the dashboard landing page implemented and **verified end-to-end**. **Build 0/0 · Tests 14/14.**
**Boundaries honoured:** no CRUD, no content-management screens, no public website pages, no repositories, no application services.

---

## 1. Authentication Flow

Auth pages live under `Pages/Account/` (outside the admin area → anonymous) and use a minimal `_AuthLayout`.

| Page | Route | Behavior |
|---|---|---|
| **Login** | `GET/POST /account/login` | `SignInManager.PasswordSignInAsync` with `lockoutOnFailure: true`; on success `LocalRedirect(returnUrl ?? ~/Admin)`; invalid → "Invalid email or password"; locked → lockout message |
| **Logout** | `POST /account/logout` | `SignInManager.SignOutAsync` → redirect to login (POST-only + antiforgery; `GET` redirects to login) |
| **Access denied** | `GET /account/denied` | friendly message + link back to sign in |
| **Set culture** | `GET /account/setculture` | writes the culture cookie (ar/en/fr) and returns to caller |

- Credentials come from the **seeded Super Admin** (Stage 3); the password is never hardcoded.
- `returnUrl` is guarded with `Url.IsLocalUrl` (defaults to `~/Admin`) — prevents open-redirect and empty-URL errors.
- Cookie endpoints (`LoginPath=/account/login`, `LogoutPath`, `AccessDeniedPath=/account/denied`) were configured in Infrastructure (Stage 0) and now resolve to real pages.

**Verified:** valid login → `302 /Admin`; wrong password → `200` with error (no access); logout clears the session.

## 2. Authorization Flow

- The entire **`/Admin` area** is gated by the **`RequireSuperAdmin`** policy via `AuthorizeAreaFolder("Admin", "/", …)` (Program.cs) — a single area-level gate (doc 06 §7).
- Policy = `RequireRole(SuperAdmin)`.
- **Anonymous → admin** redirects to `/account/login?ReturnUrl=…` (**verified `302`**).
- **Authenticated non-SuperAdmin** → `/account/denied` (only the SuperAdmin exists today).
- The admin shell pages carry **no per-page `[Authorize]`** — they inherit the area gate, keeping authorization in one place.

## 3. Navigation Structure

Mirrors `06_Admin_Dashboard_Structure.md`. The **Dashboard** is the only wired destination in this shell; every other item is a visible **placeholder** (`soon`) enabled in later stages — no dead links/404s.

```
Dashboard                         ← active link (/Admin)
Content — Profile & Pages         Home · About/Profile · Qualifications · Skills ·
                                  Experience · Memberships · Activities · Teaching   (soon)
Content — Collections             Research · Publications · Books · Theses ·
                                  Projects · Videos · Resources · Enrichment         (soon)
Organization                      Categories · Media Library                          (soon)
Site                              Header · Footer · Profile & Contact ·
                                  Appearance/Theme · SEO · Redirects                  (soon)
Communication                     Contact Messages                                    (soon)
System                            Statistics · Backup & Restore · Account & Security  (soon)
```

**Top navigation:** burger (mobile sidebar toggle) · page title · **theme toggle** · **language menu (AR/EN/FR)** · signed-in user chip · **Sign out** (POST form).

## 4. Layout Structure

- **`_AdminLayout`** (`Areas/Admin/Pages/Shared`): `admin-shell` flex layout = sticky **sidebar** (`_Sidebar` partial) + **main** (sticky topbar + content). Sets `<html lang/dir>` per culture (RTL for Arabic); applies the saved theme pre-paint via an inline script.
- **`_AuthLayout`** (`Pages/Shared`): centered card for the account pages, same tokens/theme.
- Area `_ViewStart` now points to `_AdminLayout`.
- **Styling:** `wwwroot/css/admin.css` — the canonical design tokens (deep green `#0B5D3B`, brass `#C8A45D`), full **dark-mode** token set, RTL-safe via CSS **logical properties**. **`wwwroot/js/admin.js`** — theme toggle (persisted in `localStorage`) + responsive sidebar.
- **Theme switcher:** toggles `data-theme` light/dark, persisted client-side (matches the static site behavior; doc 07 §3).
- **Language switcher:** links to `/account/setculture` which sets the `CookieRequestCultureProvider` cookie; `CurrentUICulture` then drives `lang`/`dir`. (Admin chrome labels remain English for now — `.resx` UI localization is a later enhancement, per doc 06 §7 "admin UI can be LTR".)

## 5. Security Considerations

- **Single area-level authorization gate**; no anonymous path into `/Admin`.
- **Antiforgery**: all POST forms (login, logout, language) use the Razor Pages form tag helper → automatic `__RequestVerificationToken` validation.
- **Open-redirect protection**: `Url.IsLocalUrl` on `returnUrl`.
- **Lockout** enabled on sign-in (`lockoutOnFailure: true`; 5 attempts / 15 min from Stage 0 Identity config); generic "invalid email or password" avoids account enumeration.
- **Password** never hardcoded; comes from the Stage-3 seeded account (env/user-secrets).
- **Cookie**: Identity application cookie (HttpOnly; `Secure` follows the request — HTTPS in production via the reverse proxy); 8-hour sliding expiration.
- **Logout** is POST-only (no CSRF logout via GET).
- **Culture cookie** marked `IsEssential` (no consent gate) and validated against the supported set before being written.

## 6. Files Added / Changed

**Added:** `Pages/Shared/_AuthLayout.cshtml`; `Pages/Account/{_ViewStart, Login, Logout, Denied, SetCulture}.cshtml(.cs)`; `Areas/Admin/Pages/Shared/{_AdminLayout, _Sidebar}.cshtml`; `Areas/Admin/Pages/Index.cshtml(.cs)`; `wwwroot/css/admin.css`; `wwwroot/js/admin.js`.
**Changed:** `Areas/Admin/Pages/_ViewStart.cshtml` (→ admin layout).

> No changes to Domain/Application/Infrastructure (auth services were already wired in Stages 0/3). The shell uses the framework `SignInManager` directly in PageModels — no custom repositories or application services were introduced.

## 7. Deviations from Architecture

1. **Sidebar items are placeholders** (`soon`) except Dashboard — intentional for a shell-only stage; they become links as each feature ships. No dead links.
2. **Dashboard shows no live KPIs yet** — KPI counts depend on content + statistics (later stages); the page presents the section map + session info instead of querying content (keeps Stage 4 free of content access).
3. **Admin chrome labels are English** (not yet `.resx`-localized); the language switcher is functional (sets culture, flips `dir`), with full UI-string localization deferred (doc 06 §7 permits an LTR admin).
4. **Account pages use a dedicated `_AuthLayout`** rather than the default template layout — cleaner, on-brand sign-in.

None affect the data model or architecture boundaries.

## 8. Verification

```
dotnet build → 0 warnings / 0 errors (net8.0)
dotnet test  → 14/14 passed
Runtime (seeded admin, throwaway DB):
  GET  /Admin (anon)          → 302 /account/login?ReturnUrl=%2FAdmin
  POST /account/login (valid) → 302 /Admin
  GET  /Admin (authed)        → 200 (dashboard: Welcome / Admin Console / SuperAdmin / Sign out)
  POST /account/login (bad)   → 200 "Invalid email or password"
Throwaway App_Data deleted; no DB committed.
```

**⏸ Stage 4 complete. Stopping here as instructed — awaiting approval.** (Changes are uncommitted; say the word to create the checkpoint.)
