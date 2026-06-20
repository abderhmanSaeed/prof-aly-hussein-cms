# 66 — Navigation & Research Page Review

**Date:** 2026-06-20
**Source of truth (design):** `ProfAly.Static` — `nav-menu 2.png` (static) vs `nav-menu 1.png` (current dynamic) + `assets/css/style.css`.
**Constraints honored:** no code/route/entity deleted; localization, RTL/LTR, accessibility, and routing preserved. No commit/tag/push.

> Filename note: requested as `64_…`, but `64_Breadcrumb_Alignment_Report.md` and `65_Activities_Design_Alignment_Report.md` already exist from the previous task. This report is therefore numbered **66** to avoid overwriting them.

---

## 1. Issue 1 — Research Page (decision implemented)

**Decision: Option A (preferred) — hide from public navigation, keep everything else.**

- **Removed** the Research entry from the public **navigation menu** and **footer** (both are generated from one `nav` array in `_PublicLayout.cshtml`, so removing the single entry hides it in both places).
- **Route preserved:** `/{culture}/research` still resolves and renders — verified `/ar/research` and `/en/research` → **HTTP 200**.
- **Nothing deleted:** the `Research.cshtml` page, the `ResearchPaper` content type/entity, and the route all remain intact. Re-enabling later is a one-line re-add to the `nav` array.

This is the cleanest implementation — no feature flag plumbing needed; the section simply isn't linked from the public chrome while the route stays live for direct access / future re-enable.

---

## 2. Issue 2 — Navigation styling

### Screens compared
- **`nav-menu 2.png` (static, source of truth):** one clean row of **9 short labels** (الرئيسية، نبذة، الخبرة، الأبحاث، المؤلَّفات، المشروعات، الرسائل، التدريس، تواصل), compact brand (name only at desktop), EN + theme toggle. No wrapping.
- **`nav-menu 1.png` (current dynamic):** **11 long-labelled items** (الخبرة الأكاديمية، الأنشطة والمشروعات، الأبحاث العلمية + الأبحاث المنشورة، المقررات التدريسية، مقاطع الفيديو) **wrapping to two lines**, 2-line brand, three language toggles — looks oversized and cramped.

### Differences found
| # | Aspect | Static | Dynamic (before) |
|---|---|---|---|
| 1 | **Label length (AR)** | short (الخبرة، التدريس، الرسائل، المشروعات، الأبحاث) | long (الخبرة الأكاديمية، المقررات التدريسية، …) → wrapping |
| 2 | **Item count** | 9 (no Research, no Videos) | 11 (Research + Publications + Videos) |
| 3 | **Wrap behaviour** | `flex-wrap: nowrap`, centered (`margin-inline:auto`) | `flex-wrap: wrap` → 2 lines |
| 4 | **Desktop breakpoint** | full menu ≥ **1200px** (xl); hamburger below | full menu ≥ 992px → cramming at 992–1199 |
| 5 | **Brand subtitle** | shown only in tablet range (768–1199) | always shown → wider brand |
| 6 | Font size / weight / padding / gap | .9rem / 500 / .5rem .6rem / .1rem | **already matched** (prior task) |
| 7 | Active state | gold underline | **already matched** (prior task) |
| 8 | Hover state | color → primary + underline | **already matched** |
| 9 | Logo | 44px gradient, radius 12 | **already matched** |

So the type scale, active/hover, and logo were already aligned (from the earlier UI-alignment task); the remaining mismatch was **label length, item count, wrap, breakpoint, and brand width** — all of which produced the wrapping/oversized look.

### Fixes applied
1. **Short nav labels (AR):** added `Nav_Experience/Activities/Publications/Books/Theses/Teaching/Videos` keys (AR/EN/FR) with the static short forms (e.g. الخبرة، التدريس، الرسائل، المشروعات، الأبحاث، المؤلَّفات). EN/FR were already short. The nav now uses these; the page **hero** titles keep the long descriptive forms (unchanged).
2. **`.nav-menu`** → `flex-wrap: nowrap; margin-inline: auto; justify-content: center; flex: 0 1 auto; min-inline-size: 0;` (exact static behaviour — one centered row, never wraps).
3. **Breakpoint → 1200px:** desktop horizontal menu shows at ≥1200px; the hamburger drawer is used below (burger class `d-lg-none` → `d-xl-none`; CSS media query `991.98` → `1199.98`). Matches the static and removes the 992–1199 cramming.
4. **Compact brand:** `.brand-text .sub` (tagline) hidden by default, shown only in the **768–1199** tablet range (exact static rule) — so the desktop brand is name-only and the menu has room.
5. Research removed → **10 items**, short-labelled, single row.

### Preserved
- **Accessibility:** skip link, `aria-current="page"` on the active item, `aria-expanded` burger, focus-visible — all intact.
- **Responsive:** custom mobile dropdown (with Escape/outside-close) retained, now triggered below 1200px.
- **Localization / RTL-LTR:** labels are localized resources; logical properties unchanged; AR remains RTL.
- **Routing:** all routes unchanged.

---

## 3. Mobile / Tablet navigation

- **< 1200px:** hamburger (`nav-burger`) shown; tapping opens the existing accessible dropdown panel (scrollable, closes on link/outside/Escape). The brand tagline appears in the 768–1199 range as per the static.
- **≥ 1200px:** centered single-row menu, no wrapping.
- Breadcrumb sits in the page-hero **below** the sticky header (unchanged); header keeps its scroll-elevation shadow.

---

## 4. Files Changed (16)

| File | Change |
|---|---|
| `Pages/Public/Shared/_PublicLayout.cshtml` | removed Research from `nav` array; switched nav to short `Nav_*` labels; burger `d-lg-none`→`d-xl-none` |
| `wwwroot/css/public.css` | `.nav-menu` nowrap+centered; desktop breakpoint → 1200px; brand subtitle tablet-only |
| `Resources/SharedResource.{resx,ar,fr}` | +7 short `Nav_*` keys each |
| `Pages/Public/*.cshtml` (10) | *(from the breadcrumb task in the same uncommitted set — `aria-current` crumb spans; listed here for completeness)* |

*(The 10 page-level edits and the `.crumbs`/activities CSS are the still-uncommitted breadcrumb + activities work from reports 64/65; this task added the nav/research changes on top.)*

---

## 5. Remaining Visual Differences (minor / by design)

- **Videos** appears in the dynamic nav (10 items) but not in the static screenshots (Videos didn't exist when the static was built). Kept because it is a real, populated section; it is short-labelled and fits the single row. Can be hidden the same way as Research if desired.
- **Language toggles:** dynamic shows AR/EN/FR (trilingual); the static shows only EN (it was bilingual). This is intentional (the project added French) and matches the trilingual requirement.
- Everything else (type scale, active underline, hover, logo, height, spacing) now matches the static.

---

## 6. Verification

```
dotnet build (Web) → Build succeeded, 0 errors / 0 warnings
dotnet test        → 31/31 passed
Issue 1: /ar/research & /en/research → 200 (route preserved); Research absent from nav AND footer
Issue 2: AR nav uses short labels (long forms gone); 10 items; nowrap+centered; xl(1200) breakpoint;
         brand subtitle tablet-only; burger d-xl-none; aria-current preserved
Rendered against a throwaway temp DB; real App_Data untouched; temp deleted; no stray processes.
```

**⏸ Both issues implemented (public layer only). No commit/tag/push — awaiting your review.**
