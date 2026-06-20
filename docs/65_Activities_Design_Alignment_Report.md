# 65 — Activities Page Design Alignment Report

**Date:** 2026-06-20
**Task:** Make the public **Activities** page (`/{culture}/activities`) font + design match the original static design (`research 2.png`).
**Source of truth:** `ProfAly.Static/assets/css/style.css` §15 (activities accordion) + `research.html`.
**Constraints:** public layer only; routing/DB/localization/admin unchanged; RTL/LTR preserved. No commit/tag/push.

---

## 1. What differed

The hero (breadcrumb/eyebrow/title/lead) was already aligned. The **accordion** used generic Bootstrap `.accordion-button` styling rather than the static `activities-acc` component, so the **font, size, padding, chevron, and item spacing** didn't match the static exactly.

## 2. Fix

**Markup (`Activities.cshtml`):**
- Added the **`activities-acc`** class to the accordion container (`<div class="accordion activities-acc">`).
- Added the **`act-list`** class to each group's `<ul>` body list.

**CSS (`public.css`)** — replaced the generic accordion rules with the exact static `activities-acc` block:
- `.activities-acc.accordion` → `display:grid; gap:.9rem` (even spacing between group cards).
- `.activities-acc .accordion-item` → bordered, rounded, `--shadow-sm`.
- `.activities-acc .accordion-button` → **`font-weight:700; font-size:1.2rem; padding:1.1rem 1.4rem; gap:.8rem`**; head font for LTR, **`font-family: var(--font-body)` for Arabic** (`[data-lang="ar"]`).
- Open state → `--primary-050` bg, `--primary-700` text.
- Custom **green chevron** SVG (`::after`), RTL-flipped to the inline-end.
- `.acc-count` → dark-green filled circle (2rem), white bold number.
- `.act-list li::marker` → **gold** bullet dots; `line-height:1.65`.

## 3. Result (matches `research 2.png`)

- Group headers: green count circle on the start, bold title, green chevron, light-green open background.
- Body: gold-marker list, comfortable line height, Arabic uses the body font (clean sans) exactly as the static.
- Group cards evenly spaced.

## 4. Files Changed (2)

- `src/ProfAly.CMS.Web/Pages/Public/Activities.cshtml` — `activities-acc` + `act-list` classes.
- `src/ProfAly.CMS.Web/wwwroot/css/public.css` — ported static `activities-acc` accordion block.

## 5. Verification

```
dotnet build (Web) → Build succeeded, 0 errors / 0 warnings
dotnet test        → 31/31 passed
/ar/activities → 200; markup has activities-acc + act-list + 7 acc-count badges;
CSS serves the activities-acc button (700/1.2rem), AR body-font override, green chevron, gold act-list markers.
Rendered against a throwaway temp DB; real App_Data untouched; temp deleted.
```

**⏸ Activities design alignment complete (public layer only). No commit/tag/push — awaiting review.**
