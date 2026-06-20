# 77 — Visual Regression Fixes Report

**Date:** 2026-06-20
**Source of truth:** `ProfAly.Static` (`index.html`, `about.html`, `assets/css/style.css`) + the attached screenshots (`photp overlapped.png`, `about add photo.png`).
**Method:** inspected the static CSS/HTML, diffed against my rendered output, fixed, then re-rendered and compared.
**Constraints:** public layer only; CMS-driven; localization (AR/EN/FR), RTL/LTR, responsive preserved. No commit/tag/push.

> Filename note: requested `74_…` is taken (`74_Homepage_Alignment_Checkpoint_Report.md`); this report is **77** (next free) to avoid overwriting.

---

## Issue 1 — Hero Portrait Overlap

**Root cause:** my `.hero` was missing the static's **`position: relative; overflow: hidden;`**. The portrait uses a decorative glow (`.hero-portrait-wrap::after` offset `-16px`) and an overlaid badge; without `overflow:hidden` on `.hero`, the glow/badge **spilled past the hero bounds** (the "overlap" in the screenshot). Two secondary mismatches: the hero **eyebrow** wasn't the static pill, and the **grid ratio** + mobile rules diverged (I had forced `text-align:center` and shrunk the portrait to 300px on mobile — not in the static).

**What changed (`public.css` + `Home.cshtml`):**
- `.hero` → `position:relative; overflow:hidden;` + the static radial-gradient background + `::before` dotted-grid texture + `.hero .container { position:relative }`. **The `overflow:hidden` resolves the overlap.**
- `.hero-grid` → mobile-first `1fr`, `1.15fr .85fr` at ≥992 (was `1.2fr .8fr`).
- `.hero-eyebrow` → static **pill** (surface bg, border, radius 999px, shadow, `--primary` text) + gold `.dot`; markup updated to `<span class="hero-eyebrow"><span class="dot"></span><span>tagline</span></span>` inside a `.hero-copy` wrapper.
- `.hero-name/title/positioning` → static clamps/weights/`max-inline-size:38ch`.
- Removed the mobile `text-align:center` + portrait `max-inline-size:300px` overrides → the copy stays start-aligned and the portrait keeps its 420px cap, centered via `.hero-portrait-wrap { justify-self:center }` (exactly as static).

## Issue 2 — Biography Snapshot Alignment (homepage)

**Root cause:** layout was close but I confirmed it against the static and aligned it exactly.

**State:** `.snapshot-grid` = `align-items:center`, `grid-template-columns: .9fr 1.1fr` at ≥992, single column below (image above text). `.snapshot-photo` = rounded, bordered, `aspect-ratio:5/4`, `object-position:top center`; `.snapshot-text p { font-size:1.12rem }`. Image and text now read as one coordinated, vertically-centered component — matching the static. RTL/LTR use logical grid (no left/right hardcoding).

## Issue 3 — About Page Lecture Image

**Root cause:** the dedicated **`Profile.AboutImage`** CMS field + admin upload + About-page render were added in the previous task, but (a) the About aside column was missing the static **`about-aside`** class (so panels stretched to equal height instead of sizing to content), and (b) the image only appears once an administrator uploads it (it is CMS-driven, never hardcoded).

**What changed (`About.cshtml` + `public.css`):**
- Added `about-aside` to the aside column + `.about-aside { align-self:flex-start } .about-aside .panel { block-size:auto }` (matches static).
- The image renders at the top of the biography column as `<div class="snapshot-photo mb-4"><img src="/uploads/…"></div>` — **exactly** the static structure (`col-lg-7` → snapshot-photo + eyebrow + title + bio; `col-lg-5 about-aside` → panels).
- **To display it:** upload an image in **Admin → Profile → "About page image"** (separate field; not the profile photo, no hardcoded path). Verified: after upload, `/ar/about` renders the lecture image.

> No schema change this round — the `AboutImage` field/migration already exist; this fix is the layout (`about-aside`) + confirmation of the CMS render.

---

## Files Modified (3)

| File | Change |
|---|---|
| `wwwroot/css/public.css` | `.hero` overflow/bg/`::before`; `.hero-grid` ratio; `.hero-eyebrow` pill + `.dot`; hero typography; removed mobile-center overrides; `.about-aside` rules |
| `Pages/Public/Home.cshtml` | `.hero-copy` wrapper + `.hero-eyebrow` dot markup |
| `Pages/Public/About.cshtml` | `about-aside` class on the aside column |

No routes/schema/business-logic/admin changes.

## Responsive Verification

- **Desktop (≥992px):** hero 2-col `1.15fr/.85fr`, portrait+glow+badge clipped within `.hero`; About 7/5 columns, aside top-aligned; homepage snapshot `.9fr/1.1fr` centered.
- **Tablet/Mobile (<992px):** hero stacks to one column (copy start-aligned, portrait centered, capped 420px); About columns stack; snapshot stacks (image above text). Glow no longer spills (overflow clipped).
- **RTL (ar) / LTR (en/fr):** all use logical properties (`inset-inline-*`, `justify-self`, logical grid). Badge sits at the portrait's inline-start, glow at inline-end, in both directions.

## Verification

```
dotnet build → 0 errors / 0 warnings ; dotnet test → 31/31
Rendered (temp DB, images uploaded via admin):
  Hero: .hero overflow:hidden served; eyebrow pill+dot; grid 1.15/.85; hero-copy+portrait-wrap+badge+img present
  Homepage snapshot: .9/1.1 + center; bio image renders
  About: col-lg-5 about-aside + .about-aside css; lecture image renders (snapshot-photo mb-4)
Real App_Data not used (isolated temp DBs); temp deleted; no stray processes.
```

## Remaining Visual Differences

- The About lecture image (and homepage bio/hero images) appear **only after the administrator uploads them** in Admin → Profile (CMS-driven, by design — never hardcoded). With images uploaded, the layout matches the static.
- Visual confirmation here is via the static CSS spec + rendered-HTML/CSS marker checks; no pixel screenshot tool is available in this environment. If any residual pixel-level delta remains after you view it, point me to it and I'll iterate.

**⏸ Complete (public layer only). No commit/tag/push — awaiting your review.**
