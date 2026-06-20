# 69 — Contact Data Source Fix Report

**Date:** 2026-06-20
**Issue:** The public Contact page (and footer) displayed the **email from configuration** (`SiteSettings.ContactEmail` = `info@aly-hussein.local`), which was only meant as a bootstrap default. Per the business rule, once Profile data exists, public contact info must come from **CMS-managed content**.
**Expected email:** `aly_hussein66@yahoo.com` (the CMS `Profile.Email`).
**Constraints:** no commit/tag/push.

---

## 1. Current Source Identified (audit)

| Field | Was sourced from | Correct? |
|---|---|---|
| **Email (Contact page)** | `SiteSettings.ContactEmail` (appsettings/seed bootstrap) | ❌ should be `Profile.Email` |
| **Email (footer)** | `ViewData["ContactEmail"]` ← `SiteSettings.ContactEmail` (set in `PublicPageModel.LoadChromeAsync`) | ❌ same |
| **Phone** | `Profile.Phone` (DB) | ✅ already CMS |
| **Address / Location** | `ProfileTranslation.Location` (DB, per culture) | ✅ already CMS |
| **Header contact widget** | — (no email/phone in header) | n/a |
| **Structured data / JSON-LD** | — (none present) | n/a |
| **mailto links** | Contact page + footer, both driven by the values above | fixed via source |

So **only the email** was config-driven; phone and location were already database-driven. There is no header contact widget and no SEO/structured-data email to correct.

## 2. Database Source Implemented

A single shared resolver makes **`Profile.Email` authoritative**, used by both surfaces:

```csharp
// PublicPageModel
protected static string? ContactEmailOf(Profile? profile, SiteSettings? settings) =>
    !string.IsNullOrWhiteSpace(profile?.Email) ? profile!.Email : settings?.ContactEmail;
```

- **Footer:** `PublicPageModel.LoadChromeAsync` now sets `ViewData["ContactEmail"] = ContactEmailOf(profile, settings)` (it already loaded `profile` for the phone).
- **Contact page:** `Contact.cshtml.cs` now sets `ContactEmail = ContactEmailOf(profile, settings)`.

Both already use the DB for phone (`Profile.Phone`) and location (`ProfileTranslation.Location`), so all public contact info is now CMS-driven from one consistent source.

## 3. Fallback Behavior

```
IF Profile.Email has a non-empty value  → use Profile.Email   (CMS, authoritative)
ELSE                                     → use SiteSettings.ContactEmail (bootstrap default)
```

This preserves first-run/bootstrap behaviour (before any profile exists) while guaranteeing the administrator-maintained value wins once present. Identical logic on the Contact page and the footer, so they never diverge.

## 4. Pages Corrected

- **Contact page** (`/{c}/contact`) — info card email + `mailto:` link.
- **Footer** (all public pages) — contact email + `mailto:` link.
- (Phone/location unchanged — already DB-driven; verified they still resolve from the DB.)

## 5. Files Changed (2)

| File | Change |
|---|---|
| `src/ProfAly.CMS.Web/Pages/Public/PublicPageModel.cs` | added `ContactEmailOf(...)` resolver; footer `ViewData["ContactEmail"]` now Profile-first; added `using …Domain.Entities` |
| `src/ProfAly.CMS.Web/Pages/Public/Contact.cshtml.cs` | Contact-page email now uses `ContactEmailOf(...)` |

No schema, routing, localization, or admin changes.

## 6. Verification

```
dotnet build (Web) → 0 errors / 0 warnings
dotnet test → 31/31 passed
Seeded temp DB (Profile.Email = aly_hussein66@yahoo.com; SiteSettings.ContactEmail = info@aly-hussein.local):
  /ar,/en,/fr /contact → shows aly_hussein66@yahoo.com; info@aly-hussein.local ABSENT
  /ar,/en,/fr footer   → shows aly_hussein66@yahoo.com; info@aly-hussein.local ABSENT
Fallback path preserved (empty Profile.Email → SiteSettings.ContactEmail) by construction.
Rendered against throwaway temp DBs; real App_Data untouched; temp deleted; no stray processes.
```

> Filename note: requested as `66_…`, but `66_Navigation_And_Research_Page_Review.md` already exists; this report is numbered **69** (next free) to avoid overwriting prior reports.

**⏸ Contact email source fix complete (public layer only). No commit/tag/push — awaiting your review.**
