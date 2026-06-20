# 76 — Public Website Fixes Checkpoint Report

**Date:** 2026-06-20
**Outcome:** ✅ Committed, tagged `v0.9.5-public-fixes`, pushed. Working tree in sync with `origin/main`.

---

## 1. Commit & Tag

| Item | Value |
|---|---|
| **Commit hash** | `74fee14e626c05d5a1d13e45fbf2cb1d8455130f` (`74fee14`) |
| **Message** | `Public website fixes: hero/bio alignment, 4 home books, About image, theses sort, books search, PDF download, publications intro` |
| **Parent** | `5c39e17` |
| **Tag** | `v0.9.5-public-fixes` (annotated, object `71339d7`) → peels to `74fee14` |
| **Branch** | `main` (= `origin/main`, 0 ahead / 0 behind) |
| **Push** | ✅ `5c39e17..74fee14 main -> main`; `[new tag] v0.9.5-public-fixes` |
| **Tag URL** | https://github.com/abderhmanSaeed/prof-aly-hussein-cms/releases/tag/v0.9.5-public-fixes |

Guard: **no `App_Data` / `*.db*` / backups / secrets** staged.

## 2. Files Changed (23)

20 source/resource/css files + 1 doc (`75_Public_Website_Fixes_Report.md`) + 2 migration files (`20260620195756_AddAboutImage.cs` + Designer). Domain `Profile.cs`; `ProfileConfigurations.cs`; `AppDbContextModelSnapshot.cs`; admin `Profile/Index.cshtml(.cs)`; public `Home/About/Books/Publications/Research/Theses` pages; `public.css`; `SharedResource.{resx,ar,fr}`.

One **additive** migration (`AddAboutImage` — column + index + FK). No routes/business-logic changes.

## 3. Build & Test

| | Result |
|---|---|
| Full solution build | **0 errors / 0 warnings** (temp output dir — bypasses the Visual-Studio `Web/bin` lock) |
| Tests | **31/31 passed** |

## 4. Fixes Verified (8)

1. Hero portrait overlap aligned to static. 2. Homepage bio snapshot alignment. 3. Homepage **4 book cards** (featured first). 4. New `Profile.AboutImage` + admin upload + About-page render. 5. Public theses **newest→oldest**. 6. Books search (title/description/author/publisher, `q`-string, case-insensitive, pagination-safe). 7. View/Download **PDF** buttons on Publications + Research. 8. Removed hardcoded publications intro.

CMS-driven, AR/EN/FR, RTL/LTR, responsive preserved; migration applies after a Database-Safety-Layer backup. Verified end-to-end against throwaway temp DBs; real `App_Data` untouched.

## 5. Repository State

| Item | Value |
|---|---|
| Branch | `main` (in sync with `origin/main`) |
| HEAD | `74fee14` |
| Tags | `v0.1` … `v0.9.4-homepage-alignment`, **`v0.9.5-public-fixes`** |
| Build | 0 errors |
| Tests | 31/31 |
| Migrations | `InitialCreate`, `AddSystemSettings`, `AddContactPhoto`, `AddBioImage`, `AddAboutImage` |

**⏸ Checkpoint pushed. No new feature stage started — awaiting approval.**

*(Report 76 is committed/pushed as a follow-up `docs:` commit; the tag stays anchored to `74fee14`.)*
