# 73 — Homepage Static → Dynamic Alignment Report

**Date:** 2026-06-20
**Source of truth:** `ProfAly.Static/index.html` + `assets/css/style.css` + screenshots `Main static.png` / `main dynamic.png`.
**Constraints honored:** no redesign; CMS-driven; no routes/business-logic changes; localization, RTL/LTR, responsive preserved. No commit/tag/push.

> Filename note: requested as `72_…`, but `72_Footer_BackToTop_Checkpoint_Report.md` already exists. This report is **73** (next free) to avoid overwriting.

---

## 1. Differences Found (section by section)

| Section | Static | Dynamic (before) | Severity |
|---|---|---|---|
| **Hero portrait** | rounded 22px, aspect 4/4.4, soft gold glow behind, **badge "30+ عامًا…"** overlaid bottom-inline-start | plain 18px/5:6 portrait, **no badge**, no glow | **High** |
| **Credibility chips** | `.chip` inline-flex, **gold dot (`::before`)**, shadow, hover lift | flat chip, no dot, no hover | High |
| **About "نبذة تعريفية"** | **2-column `snapshot-grid`** (.9fr/1.1fr) — **image** + text | **text only**, full width, no image | **High** |
| Stats | 5 cards, top gradient bar | already aligned ✓ | — |
| Featured books / CTA / footer | — | already aligned (prior tasks) ✓ | — |

## 2. Issue 1 — Hero Profile Badge (dynamic, CMS-driven)

- Restored the static `.hero-portrait-wrap` + `.hero-badge` (+ gold glow `::after`).
- **Data source:** the **first `StatItem`** (by SortOrder) from the CMS — `b-num` = `Value`+`Suffix`, `b-txt` = the localized `Label`. With the seeded data this renders **"30+ عامًا في خدمة التعليم"**, matching the static. **Nothing hardcoded** — editing/reordering stats in the admin changes the badge automatically.
- CSS ported verbatim: position bottom-inline-start, blurred surface, 12px radius, `.b-num` head-font 1.6rem `--primary`, `.b-txt` .8rem `--muted`. RTL-aware via logical properties.

## 3. Issue 2 — Credibility / Institutions Section

- Ported the static `.chip`: inline-flex, gap, `.55rem 1.1rem` padding, `.92rem`, `--shadow-sm`, **gold `::before` dot**, and a hover (border `--primary-300` + lift). `.cred-row` gap aligned to `.7rem`.
- Content stays fully dynamic from the `Credibility` entities (no hardcoding); the eyebrow uses the localized `Pub_Institutions` key. Verified 5 chips render.

## 4. Issue 3 — Biography / CV Image (new dynamic media field)

- **New CMS field:** `Profile.BioImageMediaId` → `BioImage` (MediaFile), EF FK (`OnDelete: SetNull`). **Migration `AddBioImage`** — purely additive (column + index + FK). Independent of `Photo` (hero) and `ContactPhoto`.
- **Admin:** Profile screen now has a **"Biography image (homepage)"** upload via the existing `IMediaUploadService` / `MediaKind.Image` pipeline. No reuse of the profile image; no hardcoded path.
- **Homepage:** the About section is now the static **`snapshot-grid`** (image column + text column). Renders `BioImage` from `/uploads/{RelativePath}`; if unset, a graceful branded placeholder keeps the 2-column layout.
- Verified end-to-end: uploaded a real image in admin → it renders in the homepage About section (`/ar`).

## 5. Issue 4 — Additional Homepage Improvements

- Hero portrait restyled to the static (22px radius, 4/4.4 ratio, gold glow) + graceful glyph fallback.
- About section converted from a flat full-width paragraph to the static 2-column snapshot layout (the main layout regression).
- Credibility hover/lift micro-interaction restored.

## 6. Files Changed (11 + 2 migration files)

| File | Change |
|---|---|
| `Domain/Entities/Profile.cs` | `BioImage` field |
| `Infrastructure/.../ProfileConfigurations.cs` | BioImage FK |
| `Infrastructure/.../Migrations/20260620183855_AddBioImage.cs` (+ Designer, snapshot) | additive migration |
| `Web/Areas/Admin/Pages/Profile/Index.cshtml(.cs)` | Bio-image upload |
| `Web/Pages/Public/Home.cshtml(.cs)` | hero badge (first stat) + snapshot-grid with bio image |
| `Web/wwwroot/css/public.css` | hero-portrait-wrap/badge, chip, snapshot blocks (ported from static) |
| `Web/Resources/SharedResource.{resx,ar,fr}` | `Profile_BioImage` + `_Help` keys |

No routes/business-logic changes. One additive migration (`AddBioImage`), applied after the Database-Safety-Layer startup backup.

## 7. Responsive Verification

- **≥992px:** hero 2-col (text + portrait+badge); credibility centered chip row; About `snapshot-grid` .9fr/1.1fr.
- **<992px:** hero stacks & centers (existing rule); About `snapshot-grid` collapses to a single column (image above text); chips wrap.
- **RTL (ar) / LTR (en/fr):** badge sits at inline-start of the portrait via `inset-inline-start`; glow at inline-end; snapshot uses logical grid — verified across all three cultures.

## 8. Verification

```
dotnet build (Infra + Web) → 0 errors / 0 warnings
dotnet test → 31/31 passed
Migration AddBioImage → additive; applied after a safety-layer startup backup
Hero badge: b-num renders "30+" (Value 30 + Suffix "+"); text = first stat label; present /ar /en /fr
Credibility: 5 .chip items with gold-dot styling
Bio image: admin upload → 302 → BioImageMediaId persisted → renders in homepage About (/ar, real image)
Graceful: no bio image → branded placeholder keeps the 2-col layout
Rendered against throwaway temp DBs; real App_Data untouched; temp deleted; no stray processes.
```

## 9. Remaining Differences (minor / by design)

- Featured-book covers differ where a real cover image exists vs the static's letter covers — that's CMS data, not styling.
- Stat **numbers** differ from the static screenshot (different dataset); the layout/animation match.
- French bio falls back to Arabic until FR bio text is entered (project policy).

**⏸ Homepage alignment complete (CMS field + migration + public layer). No commit/tag/push — awaiting your review.**
