# 74 — Homepage Alignment Checkpoint Report

**Date:** 2026-06-20
**Outcome:** ✅ Committed, tagged `v0.9.4-homepage-alignment`, pushed. Working tree in sync with `origin/main`.

---

## 1. Commit & Tag

| Item | Value |
|---|---|
| **Commit hash** | `42e98eac70258d40ccc425cba975ace9ebb389c0` (`42e98ea`) |
| **Message** | `Homepage static alignment: hero stat badge, credibility chips, dynamic bio image` |
| **Parent** | `47e2acc` |
| **Tag** | `v0.9.4-homepage-alignment` (annotated, object `89f496b`) → peels to `42e98ea` |
| **Branch** | `main` (= `origin/main`, 0 ahead / 0 behind) |
| **Push** | ✅ `47e2acc..42e98ea main -> main`; `[new tag] v0.9.4-homepage-alignment` |
| **Tag URL** | https://github.com/abderhmanSaeed/prof-aly-hussein-cms/releases/tag/v0.9.4-homepage-alignment |

Guard: **no `App_Data` / `*.db*` / backups / secrets** staged.

## 2. Files Changed (14)

| Area | Files |
|---|---|
| Domain | `Entities/Profile.cs` (BioImage field) |
| Infrastructure | `Configurations/ProfileConfigurations.cs`; migration `20260620183855_AddBioImage.cs` (+ `.Designer.cs`); `AppDbContextModelSnapshot.cs` |
| Admin | `Areas/Admin/Pages/Profile/Index.cshtml(.cs)` (bio-image upload) |
| Public | `Pages/Public/Home.cshtml(.cs)` (hero badge from first stat + snapshot-grid bio image) |
| CSS | `wwwroot/css/public.css` (hero-portrait-wrap/badge, chip, snapshot) |
| Resources | `SharedResource.{resx,ar,fr}` (`Profile_BioImage` + `_Help`) |
| Docs | `73_Homepage_Alignment_Report.md` (+ this report, 74) |

One **additive** migration (`AddBioImage` — column + index + FK on `Profile`). No routes/business-logic changes.

## 3. Build & Test

| | Result |
|---|---|
| Full solution build | **0 errors / 0 warnings** (temp output dir — bypasses the Visual-Studio `Web/bin` lock) |
| Tests | **31/31 passed** |

## 4. Verified State

- **Hero badge:** CMS-driven from the first `StatItem` → "30+ عامًا في خدمة التعليم"; present in ar/en/fr.
- **Credibility:** static-styled chips (gold dot, shadow, hover) from `Credibility` entities.
- **Bio image:** new `Profile.BioImage` (separate from Photo/ContactPhoto); admin upload → renders in the homepage 2-column About `snapshot-grid`; graceful placeholder when unset. Verified upload → render on `/ar`.
- Localization, RTL/LTR, responsive preserved; migration applied after a Database-Safety-Layer startup backup.
- Verified against throwaway temp DBs; real `App_Data` untouched.

## 5. Repository State

| Item | Value |
|---|---|
| Branch | `main` (in sync with `origin/main`) |
| HEAD | `42e98ea` |
| Tags | `v0.1` … `v0.9.3-footer-backtotop`, **`v0.9.4-homepage-alignment`** |
| Build | 0 errors |
| Tests | 31/31 |
| Migrations | `InitialCreate`, `AddSystemSettings`, `AddContactPhoto`, `AddBioImage` |

**⏸ Checkpoint pushed. No new feature stage started — awaiting approval.**

*(Report 74 is committed/pushed as a follow-up `docs:` commit; the tag stays anchored to `42e98ea`.)*
