# 78 — Visual Regression Fixes Checkpoint Report

**Date:** 2026-06-20
**Outcome:** ✅ Committed, tagged `v0.9.6-visual-regression-fixes`, pushed. Working tree in sync with `origin/main`.

---

## 1. Commit & Tag

| Item | Value |
|---|---|
| **Commit hash** | `b7a8499544a92385716759f4db36599aebf2d1f1` (`b7a8499`) |
| **Message** | `Visual regression fixes: hero overlap (overflow/pill/grid), bio snapshot, About aside image` |
| **Parent** | `f5bd8af` |
| **Tag** | `v0.9.6-visual-regression-fixes` (annotated, object `010cc27`) → peels to `b7a8499` |
| **Branch** | `main` (= `origin/main`, 0 ahead / 0 behind) |
| **Push** | ✅ `f5bd8af..b7a8499 main -> main`; `[new tag] v0.9.6-visual-regression-fixes` |
| **Tag URL** | https://github.com/abderhmanSaeed/prof-aly-hussein-cms/releases/tag/v0.9.6-visual-regression-fixes |

Guard: **no `App_Data` / `*.db*` / backups / secrets** staged.

## 2. Files Changed (4)

| File | Change |
|---|---|
| `wwwroot/css/public.css` | `.hero` overflow/bg/`::before`; grid `1.15/.85`; `.hero-eyebrow` pill + `.dot`; hero typography; removed mobile-center overrides; `.about-aside` rules |
| `Pages/Public/Home.cshtml` | `.hero-copy` wrapper + eyebrow dot markup |
| `Pages/Public/About.cshtml` | `about-aside` class on the aside column |
| `docs/77_Visual_Regression_Fixes_Report.md` | implementation report |

No schema/routes/business-logic/admin changes (the `AboutImage` field already existed).

## 3. Build & Test

| | Result |
|---|---|
| Full solution build | **0 errors / 0 warnings** (temp output dir — bypasses the Visual-Studio `Web/bin` lock) |
| Tests | **31/31 passed** |

## 4. Fixes Verified

1. **Hero overlap** — `.hero { overflow:hidden }` clips the portrait glow/badge spill (root cause); eyebrow pill + dot; grid `1.15/.85`; portrait-wrap/badge render.
2. **Homepage bio snapshot** — `.9fr/1.1fr`, `align-items:center`; image renders.
3. **About page** — `about-aside` (panels size to content); CMS `AboutImage` renders at the top of the bio column like the static (shows once uploaded in admin).

Localization (AR/EN/FR), RTL/LTR, responsive preserved. Verified against rendered output on a throwaway temp DB; real `App_Data` untouched.

## 5. Repository State

| Item | Value |
|---|---|
| Branch | `main` (in sync with `origin/main`) |
| HEAD | `b7a8499` |
| Tags | `v0.1` … `v0.9.5-public-fixes`, **`v0.9.6-visual-regression-fixes`** |
| Build | 0 errors |
| Tests | 31/31 |

**⏸ Checkpoint pushed. No new feature stage started — awaiting approval.**

*(Report 78 is committed/pushed as a follow-up `docs:` commit; the tag stays anchored to `b7a8499`.)*
