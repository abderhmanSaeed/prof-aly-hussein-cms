# 68 — Contact Page Alignment Report

**Date:** 2026-06-20
**Task:** Make the dynamic Contact page visually match the original static Contact page, and make the professor photo + contact information fully CMS-driven.
**Source of truth:** `ProfAly.Static/contact.html` + `assets/css/style.css` (§18) + screenshots `contact Static.png` / `contact Dynamic.png`.
**Constraints honored:** no redesign; replicate the original layout; routing/localization/RTL-LTR/admin preserved. No commit/tag/push.

> Filename note: requested as `65_…`, but `65_Activities_Design_Alignment_Report.md` already exists. This report is numbered **68** (next free; 64–67 are taken) to avoid overwriting prior reports.

---

## 1. Differences Found (static vs dynamic)

| # | Aspect | Static (truth) | Dynamic (before) |
|---|---|---|---|
| 1 | **Layout** | 2-column `contact-grid` (1fr / 1.1fr): info column + form card | Bootstrap `row` panels |
| 2 | **Professor photo** | photo card at top of the info column | **missing entirely** |
| 3 | **Contact info** | `contact-row`s with **green circular icons** (phone/email/location), label + value | plain `label: value` list |
| 4 | **Form** | `form-card` with **"أرسل رسالة" title**, `form-grid` (name+email side-by-side, subject/message full), helper note, large button | generic panel, all fields stacked, no title |
| 5 | **Field styling** | 1.5px border, radius-sm, focus ring, 200px textarea | Bootstrap `.form-control` |
| 6 | **Spacing/credibility** | photo + card shadows give balance | flatter, weaker hierarchy |

## 2. Layout Fixes Applied

Rebuilt `Contact.cshtml` to the static structure (no redesign):
- **`.contact-grid`** (2-col ≥992px) → **info column** (`.contact-aside`) + **form column**.
- **Info column:** `.contact-photo` card (top) + `.panel` "بيانات التواصل" with **`.contact-row`** items, each a **green `.contact-ic` icon** (phone/email/location, SVG masks ported from the static) + `.contact-label` + value. Optional **social buttons** (Facebook/WhatsApp) when set.
- **Form column:** `.form-card` with **`.panel-title` "Send a message"**, **`.form-grid`** (name + email side-by-side on ≥576px via `.field-full` spanning), subject + message full-width, honeypot, large primary button.
- Ported the static **`.contact-grid / .contact-photo / .contact-row / .contact-ic* / .form-card / .form-grid / .field`** CSS verbatim into `public.css` (incl. `--radius-sm` field inputs, focus ring, 200px textarea, mobile full-width button). RTL/LTR handled via logical properties + the existing Bootstrap RTL stylesheet.

## 3. Dynamic Photo Implementation

- **New CMS field:** `Profile.ContactPhotoMediaId` → `ContactPhoto` (MediaFile), with EF FK (`OnDelete: SetNull`). **Migration `AddContactPhoto`** — purely additive (adds column + index + FK; no other table touched).
- **Admin:** the Profile screen now has a **"Contact page photo"** upload (uses the existing `IMediaUploadService` / `MediaKind.Image` pipeline — same as the main photo). No hardcoded paths.
- **Public render:** the Contact page shows `ContactPhoto`; if unset it **falls back to the main `Profile.Photo`**; if neither exists it shows a **graceful branded placeholder** (the "ع" glyph) — verified.
- Image served from `/uploads/{RelativePath}` via the existing `MediaFile` system.

## 4. Dynamic Contact Information

All contact data is CMS-managed (no hardcoding):

| Field | Source (already dynamic) | Editable in admin |
|---|---|---|
| Phone | `Profile.Phone` | Profile screen |
| Email | `SiteSettings.ContactEmail` | (Site settings seed/admin) |
| Office / Location | `ProfileTranslation.Location` (per culture) | Profile screen |
| Facebook / WhatsApp | `SiteSettings.FacebookUrl` / `WhatsAppNumber` | (Site settings) |
| **Contact photo** | `Profile.ContactPhoto` *(new)* | Profile screen |

The Contact page renders a row only when the value exists, so empty fields are hidden gracefully.

## 5. CMS Changes

- `Profile` entity: `ContactPhotoMediaId` + `ContactPhoto` nav.
- `ProfileConfiguration`: FK mapping (SetNull).
- Admin Profile page (`Index.cshtml(.cs)`): new `ContactPhotoFile` input, GET load, POST upload, reload-on-error, `LoadAsync` include.
- Resx: `Profile_ContactPhoto`, `Profile_ContactPhoto_Help`, `Contact_SendMessage` (AR/EN/FR).

## 6. Database Changes

- **One additive migration `20260620175831_AddContactPhoto`** — adds `Profile.ContactPhotoMediaId` (nullable) + index + FK to `MediaFile`. Down reverses cleanly. No data loss; the Database Safety Layer takes a **startup backup before applying it** (verified in the run log).

## 7. Files Changed (12 + 2 migration files)

| File | Change |
|---|---|
| `Domain/Entities/Profile.cs` | `ContactPhoto` field |
| `Infrastructure/.../ProfileConfigurations.cs` | ContactPhoto FK |
| `Infrastructure/.../Migrations/20260620175831_AddContactPhoto.cs` (+ Designer, snapshot) | additive migration |
| `Web/Areas/Admin/Pages/Profile/Index.cshtml(.cs)` | contact-photo upload |
| `Web/Pages/Public/Contact.cshtml` | static 2-col layout (photo + icon rows + form-card) |
| `Web/Pages/Public/Contact.cshtml.cs` | load contact photo (+fallback) + social links |
| `Web/wwwroot/css/public.css` | ported static contact/form CSS |
| `Web/Resources/SharedResource.{resx,ar,fr}` | 3 new keys each |

No routing, business-logic, or other-schema changes.

## 8. Verification

```
dotnet build (Infrastructure + Web) → 0 errors / 0 warnings
dotnet test → 31/31 passed
Migration AddContactPhoto → additive (column+index+FK); applied after a safety-layer startup backup
Admin: Profile has the Contact-photo field; uploading a PNG → 302 (saved via MediaFile)
Public /ar,/en,/fr/contact → 200; renders photo (or fallback glyph), 3 icon rows (phone/email/location), form-grid, "Send a message" title
Graceful fallback confirmed (no photo → branded placeholder glyph)
Rendered against throwaway temp DBs; real App_Data untouched; temp deleted; no stray processes.
```

## 9. Remaining Differences (minor)

- **Office/Address:** the static had only "Location" (city) — there is no separate street-address field in the model. Location is used as the office line. A distinct address field can be added later if needed.
- **Social icons:** rendered as labeled outline buttons (Facebook/WhatsApp) rather than icon glyphs — the static contact card didn't show social icons, so this is an additive, optional touch shown only when configured.
- Everything else (2-col grid, photo card, icon rows, form card, field styling, spacing, shadows, RTL/LTR, responsive stacking) now matches the static.

**⏸ Contact page alignment complete (CMS field + migration + public layer). No commit/tag/push — awaiting your review.**
