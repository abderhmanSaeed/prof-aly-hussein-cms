# 42 — Localization Audit & Fix Report

**Phase:** Defect repair — localization (Stage 8 follow-up).
**Date:** 2026-06-19
**Outcome:** ✅ Root cause found and fixed. `/ar`, `/en`, `/fr` render fully translated UI with **zero localization keys leaked**. Build 0/0, tests 14/14.
**Scope:** localization diagnosis and repair only — no new stage started.

> Filename note: the requested name was `41_…`, but `41_Stage8_Checkpoint_Report.md` already exists (committed). This report is therefore `42_…` to avoid overwriting it.

---

## 1. Root Cause

Every Razor view injected the **wrong localizer type**:

```cshtml
@inject IViewLocalizer L          // ← resolves PER-VIEW resource files
```

`IViewLocalizer` resolves resources **named after the view** (e.g. `Resources/Pages/Public/Shared/_PublicLayout.ar.resx`, `Resources/Areas/Admin/Pages/Shared/_Sidebar.ar.resx`). Those files do not exist — all UI strings live in the **shared** `Resources/SharedResource.*.resx`. When a key isn't found, `IViewLocalizer` returns the **key itself** (`LocalizedHtmlString` with `ResourceNotFound = true`). Hence the navigation showed `Nav_Home`, `Experience_Title`, etc.

**Why it surfaced only now:** the same defect existed in the admin views since Stage 5, but most admin keys are English words (`Save`, `Cancel`, `Edit`, `Delete`), so the leaked key was indistinguishable from correct English text. The public navigation uses semantic keys (`Nav_Home`, `Experience_Title`), which made the leak obvious. (In Arabic, the admin sidebar was in fact showing the English-looking keys too.)

**Correct mechanism:** the shared resource is reachable only via a typed localizer — `IStringLocalizer<SharedResource>` / `IHtmlLocalizer<SharedResource>`. This is exactly what the DataAnnotations/validation path already used (which is why validation messages worked), confirming `SharedResource` itself was correctly registered and resolvable.

---

## 2. The Fix

Replaced the injected localizer in **all 27 view locations** with the typed shared localizer (fully-qualified, so no `@using` changes were needed):

```cshtml
@inject Microsoft.Extensions.Localization.IStringLocalizer<ProfAly.CMS.Web.SharedResource> L
```

`IStringLocalizer<SharedResource>` resolves `Resources/SharedResource.{culture}.resx` (via `ResourcesPath = "Resources"` + the `SharedResource` marker type in the root `ProfAly.CMS.Web` namespace). `L["key"]` and `L["key"].Value` are API-compatible with the prior usage, so no call sites changed. Bonus: `LocalizedString` is HTML-encoded by Razor on output (safer than the previous `LocalizedHtmlString`).

---

## 3. Files Changed (27)

| Area | Files |
|---|---|
| **Public** | `Pages/Public/_ViewImports.cshtml` (the single inject the layout + all 10 public pages inherit) |
| **Admin (per-page injects, 26)** | `Areas/Admin/Pages/Shared/_Sidebar.cshtml`; Activities (`Index`, `Items`, `GroupUpsert`, `ItemUpsert`); Categories (`Index`, `Upsert`); Content (`Index`, `Upsert`); Credibility, Experience, Memberships, Qualifications, Skills, Stats, Teaching, Theses (`Index` + `Upsert` each); Profile (`Index`) |

No `.cs`, `.resx`, or configuration files needed changes — the bug was purely the injected localizer type.

---

## 4. Missing Keys Added

**None required.** A full audit (extract every `L["…"]` used in views → compare to each resx) found:

- **135** distinct keys used across all views.
- **147** keys defined in each of `SharedResource.resx` (neutral/English), `.ar.resx`, `.fr.resx` — perfectly parallel.
- **0** keys missing in neutral/en, ar, or fr; **0** keys present in neutral but absent from ar/fr.

So every key used by the UI is defined and translated in all three cultures.

---

## 5. Localization Architecture Validation

| Check | Result |
|---|---|
| 1. `Program.cs` config | ✅ `AddLocalization(ResourcesPath="Resources")`, `AddViewLocalization()`, `AddDataAnnotationsLocalization(→ SharedResource)` |
| 2. `RequestLocalizationOptions` | ✅ DefaultRequestCulture = `ar`; `RouteDataRequestCultureProvider{RouteDataStringKey="culture"}` inserted at index 0 (route wins over cookie/query/Accept-Language) |
| 3. SupportedCultures / SupportedUICultures | ✅ both = `[ar, en, fr]` from `SupportedCultures.All` |
| 4. SharedResource registration | ✅ marker class `ProfAly.CMS.Web.SharedResource`; resx at `Resources/SharedResource.{,.ar,.fr}.resx` (neutral = English) |
| 5. `IStringLocalizer` usage | ✅ now used in Layout, navigation/sidebar, all public pages, all admin pages; PageModels/DataAnnotations already used `IStringLocalizer<SharedResource>` |
| 6. Resource files | ✅ `SharedResource.resx` (en/neutral), `.ar.resx`, `.fr.resx` — 147 keys each. (No `SharedResource.en.resx`: the neutral file *is* English, which is the correct .NET convention.) |
| 7. Missing keys | ✅ none (§4) |
| 8. Namespace mismatches | ✅ none — type fully-qualified `ProfAly.CMS.Web.SharedResource`; marker in root namespace matches `ResourcesPath` |
| 9. Resource path mismatches | ✅ none — `Resources/SharedResource.*.resx` matches `ResourcesPath` + type name |
| 10. Culture-switch endpoint | ✅ public switcher uses route links (`/{culture}/…`) → drives `RouteDataRequestCultureProvider`; admin/auth `SetCulture` endpoint sets the culture cookie |
| 11. Culture cookie persistence | ✅ `SetCulture` writes the standard `.AspNetCore.Culture` cookie; not required for the public site (route-driven) |
| 12. AR/EN/FR updates nav/titles/labels/validation | ✅ verified (§6) — and confirmed it changes the **UI culture**, not just the cookie: a fresh `/fr` request **with no cookie** rendered French labels, proving the route provider set `CurrentUICulture = fr` and loaded `SharedResource.fr.resx` |

**Culture flow (confirmed):** route segment `/{ar|en|fr}/…` → `RouteDataRequestCultureProvider` → `CurrentCulture`/`CurrentUICulture` set by `RequestLocalizationMiddleware` → `IStringLocalizer<SharedResource>` loads the matching resx → views render translated text. Switching languages navigates to the target-culture URL, so the UI culture genuinely changes (independent of any cookie).

---

## 6. Before / After Verification

Seeded DB (`Seed:ImportStaticContent=true`), then fetched pages and scanned the HTML for leaked key tokens (`Nav_Home`, `Nav_About`, `Experience_Title`, `Activities_Title`, `Research_Title`, `Publications_Title`, `Books_Title`, `Theses_Title`, `Teaching_Title`, `Pub_InNumbers`, `About_Bio`, `Contact_Intro`).

| URL | Before | After |
|---|---|---|
| `/ar` | nav showed `Nav_Home`, `Experience_Title`, … | **0 leaked keys**; nav = `الرئيسية`, `نبذة`, `الخبرة الأكاديمية`, … |
| `/en` | same keys leaked | **0 leaked keys**; nav = `Home`, `About`, `Experience`, … |
| `/fr` | same keys leaked | **0 leaked keys**; nav = `Accueil`, `À propos`, … (French resx loaded with **no cookie**) |
| `/Admin/*` (Arabic default) | sidebar showed `Profile_Title`, … | **0 leaked keys**; sidebar renders translated labels |

Validation messages (`Field_Required`, `Field_Email_Invalid`) were already localized via `IStringLocalizer<SharedResource>` in PageModels/DataAnnotations and remain correct.

```
dotnet build → 0 warnings / 0 errors (net8.0)
dotnet test  → 14/14 passed
/ar /en /fr → 0 leaked localization keys; translated nav/titles/labels confirmed
Admin sidebar → 0 leaked keys
Throwaway App_Data deleted; no DB committed
```

---

## 7. Notes

- Pure view-layer fix; no schema, config, or resx content changes.
- Leftover `@using Microsoft.AspNetCore.Mvc.Localization` directives are now unused but harmless.
- Changes are **uncommitted** — ready for a checkpoint commit on request (these are fixes atop the pushed Stage 8 commit `3642044`).

**⏸ Localization audit & repair complete. No new stage started.**
