# 71 — Footer Alignment & Back-To-Top Report

**Date:** 2026-06-20
**Source of truth:** `ProfAly.Static` — `assets/css/style.css` (§19 footer, §20 back-to-top) + `assets/js/main.js` (footer template + back-to-top) + screenshots `contact Static.png` (footer) / `footer dynamic.png`.
**Constraints honored:** public layer only; no routes/schema/logic/CMS changes; localization, RTL/LTR, accessibility, responsive preserved. No commit/tag/push.

---

## 1. Screens Compared
- **Static footer** (bottom of `contact Static.png`): 3 columns — brand (logo + name + tagline + "built" line) · Quick Links (2-column) · Get in Touch (email/phone/location) — plus a floating green circular **back-to-top** button.
- **Dynamic footer** (`footer dynamic.png`): 3 sparse columns — brand = logo+name only · "Explore" (single column) · contact = email+phone (no location) · base "© year name" · **no back-to-top button**.

## 2. Footer Differences Found → Fixes Applied

| # | Aspect | Static (truth) | Dynamic (before) | Fix |
|---|---|---|---|---|
| 1 | Column widths | brand `lg-4` / links `lg-5` / contact `lg-3` | `lg-5 / lg-4 / lg-3` | columns re-proportioned to match |
| 2 | Brand block | mark + name + **tagline** + **"built" line** | mark + name only (sparse) | added `.footer-tagline` (CMS tagline) + `.footer-built` |
| 3 | Quick-links heading | "Quick Links" / "روابط سريعة" | "Explore" | switched to `Footer_QuickLinks` |
| 4 | Quick-links layout | **2-column** grid (≥576px, 5 rows, column-major) | single column | ported `.footer-links` grid CSS; renders full nav |
| 5 | Contact heading | "Get in Touch" / "للتواصل" | "بيانات التواصل" | switched to `Footer_Contact` |
| 6 | Contact rows | email + phone + **location** | email + phone | added **location** row (`.footer-loc`, CMS) |
| 7 | Heading style | **gold**, uppercase, letter-spaced, .82rem/700 | plain .95rem | ported `.footer-h` |
| 8 | Link color | `--ink`, .96rem, hover `--primary` | `--muted`, .9rem | ported |
| 9 | Footer base | "© {year} {name}. **{rights}**" | "© {year} {name}" | added `Footer_Rights` |
| 10 | Spacing | clamp top pad, 2.2rem base margin | flatter | ported `.site-footer`/`.footer-base` |

Email remains **CMS-driven** (Profile.Email → SiteSettings fallback, from report 69) — unchanged.

## 3. Back-To-Top Implementation

Restored the static button exactly:
- **Markup** (`_PublicLayout.cshtml`): `<button class="back-to-top" type="button" aria-label="@L["Back_To_Top"]" title="…">` with the static up-arrow SVG, placed after the footer.
- **CSS** (`public.css`, ported verbatim): fixed `inset-block-end/inset-inline-end: 1.5rem`, **48px circle**, `--primary` bg, white icon, `box-shadow`; hidden by default (`opacity:0; visibility:hidden; transform: translateY(12px) scale(.9)`), `.show` reveals it; hover → `--primary-700` + lift/scale; `.28s` ease.
- **JS** (`public.js`): the existing scroll listener now also toggles `.back-to-top.show` when **`scrollY > 500`**; clicking it does `window.scrollTo({ top: 0, behavior: "smooth" })`.

| Reviewed | Result |
|---|---|
| Position | fixed, inline-end / block-end 1.5rem (RTL → left, LTR → right via logical props) |
| Size / shape | 48px circle |
| Colors | `--primary` → `--primary-700` on hover; white icon |
| Hover | darken + translateY(-3px) scale(1.05) |
| Animation | fade/translate/scale over .28s; smooth scroll to top |
| Visibility threshold | appears after `scrollY > 500` |
| Accessibility | `type=button`, localized `aria-label`, global `:focus-visible` ring; SVG `aria-hidden` |
| Mobile | 48px target at 1.5rem inset — doesn't obstruct content |

## 4. Files Changed (7)

| File | Change |
|---|---|
| `Pages/Public/Shared/_PublicLayout.cshtml` | footer restructured (4/5/3, brand tagline+built, quick-links + contact headings, location row, rights) + back-to-top button |
| `Pages/Public/PublicPageModel.cs` | expose `ViewData["ContactLocation"]` (culture-aware, from `ProfileTranslation.Location`) |
| `wwwroot/css/public.css` | ported static footer block (`.footer-brand/.footer-tagline/.footer-built/.footer-h/.footer-links 2-col/.footer-loc/.footer-base`) + `.back-to-top` |
| `wwwroot/js/public.js` | back-to-top show-threshold (500) + smooth-scroll click |
| `Resources/SharedResource.{resx,ar,fr}` | +5 keys each (`Footer_QuickLinks`, `Footer_Contact`, `Footer_Built`, `Footer_Rights`, `Back_To_Top`) |

No routes/schema/business-logic/CMS/admin changes.

## 5. Responsive Verification

- **≥992px (lg):** 3-column footer (4/5/3); quick links in 2 sub-columns.
- **768–991px (md):** brand full-width, then quick-links (`md-7`) + contact (`md-5`) side by side; quick links 2-col.
- **<576px (mobile):** all columns stack; quick links collapse to a single column; back-to-top stays at 1.5rem inset (48px, non-obstructing).
- **RTL (ar) / LTR (en, fr):** footer uses Bootstrap grid + logical text; back-to-top uses `inset-inline-end` so it sits bottom-left in RTL, bottom-right in LTR. Verified all three cultures render the full footer + button.

## 6. Verification

```
dotnet build (Web) → 0 errors / 0 warnings
dotnet test → 31/31 passed
/ar,/en,/fr footer: brand+tagline+built, 2-col quick links, Get-in-Touch (CMS email aly_hussein66@yahoo.com + phone + location), rights line; .footer-h gold
back-to-top: button in DOM (localized aria-label) on all pages; CSS .back-to-top/.show served; JS threshold 500 + smooth scroll confirmed
Rendered against a throwaway temp DB; real App_Data untouched; temp deleted; no stray processes.
```

## 7. Remaining Visual Differences

- The static "built" line read "A bilingual academic website."; this project is **trilingual** (AR/EN/FR), so the text is "A trilingual academic website." / "موقعٌ أكاديميٌّ ثلاثي اللغة." / "Un site académique trilingue." — intentional, not a regression.
- Otherwise the footer (columns, brand block, headings, link grid, contact rows, base) and the back-to-top button now match the static design.

**⏸ Footer alignment + back-to-top complete (public layer only). No commit/tag/push — awaiting your review.**
