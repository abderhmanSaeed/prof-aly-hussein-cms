# 85 — Header Navigation UX Fix Report

> Scope: fix the navigation **usability/layout** issues only. Branding, colours, typography
> (fonts/sizes/weights), and the existing navigation structure are **unchanged**. Verified with
> headless-browser screenshots across desktop/tablet, RTL/LTR.
> **No commit / tag / push.**

Files touched: `wwwroot/css/public.css` (CSS) and `Pages/Public/Shared/_PublicLayout.cshtml`
(one class on the hamburger button).

---

## Issue 1 — Labels breaking into two lines  ✅ Fixed

**Cause:** `.nav-menu a` / `.nav-dropdown-toggle` had no `white-space: nowrap`, and the shared
`.container` capped the header at **1200px**, which physically squeezed the 13 items — so the
multi-word Arabic labels **المصادر الرقمية** and **فعاليات متنوعة** collapsed onto two lines.

**Fix:**
- `white-space: nowrap` on every nav link **and** dropdown toggle → labels can never wrap.
- Header given its own `max-width: 1440px` (the content column stays 1200px) so all items have room
  for a single line. The brand name also gets `nowrap` (it was wrapping too — visible in the
  original screenshot).
- Text stays centered; font family/size/weight unchanged (readability preserved).

Verified single-line at 1568 / 1440 px in **both** AR and EN; the two previously-wrapping items
are now one line.

---

## Issue 2 — Dropdown gap / hover dead-zone  ✅ Fixed

**Cause:** the panel was positioned `inset-block-start: calc(100% + .25rem)` — a **4px gap** between
the trigger and the menu. Moving the cursor across that gap dropped `:hover` and the menu closed.

**Fix:**
- Panel now `inset-block-start: 100%` — it **touches** the trigger (no gap).
- Added an invisible **hover bridge** (`.nav-dropdown::after`, a 0.6rem transparent strip) so the
  `:hover`/`:focus-within` region is continuous from trigger to menu even during the open
  animation. The bridge is disabled in the mobile drawer (where the menu is static).

Verified by force-opening the menus: each panel connects flush to its trigger, no dead-zone.

---

## Issue 3 — Hover target too small  ✅ Fixed

**Cause:** items were text + ~`.5rem` padding ≈ 30–32px tall — the hit area felt glued to the text.

**Fix:** every item (link and toggle) is now an equal **`inline-flex` chip, `min-block-size: 44px`**
— the **entire chip** is the hover/click target, meeting the 44px accessibility minimum. Keyboard
focus rings already exist globally (`:focus-visible`, report 47/48) and now apply to these larger
targets.

---

## Issue 4 — Layout review  ✅ Improved

- **Alignment:** all items are the same 44px flex chip → one shared horizontal baseline (the
  floating-caret / off-baseline look is gone).
- **Spacing:** consistent `gap` + symmetric `padding-inline` across links and toggles.
- **Dropdown positioning:** flush, connected, with the hover bridge.
- **Responsive:** see below — the one-line nav now appears only when it actually fits; otherwise the
  existing hamburger drawer is used (no more squeezed/wrapped state at any width).
- **Hover behaviour:** full-chip hover; smooth trigger→menu travel.
- Branding/colours/type untouched.

---

## Responsive behaviour (after)

The 13 top-level items + brand + utilities need ~**1440px** in **English** (the wider language) to
sit on one clean line; Arabic needs less. Rather than shrink type or cut items (out of scope), the
horizontal nav now shows **only when it fits**, and the existing drawer covers the rest:

| Width | Behaviour |
|---|---|
| **≥ 1440px** | Full one-line horizontal nav (header capped at 1440px). Clean in AR and EN. |
| **768–1439px** | Hamburger **drawer** (brand + tagline + utilities stay in the bar). The trigger threshold was raised from 1200→1440 and the burger is now driven by the **same** breakpoint as the menu (it was a Bootstrap `d-xl-none`, which would have desynced and hidden the burger in 1200–1440). |
| **< 768px** | Mobile drawer (unchanged). |

In the drawer, the two dropdowns expand **inline** (Digital Resources → its 3 items; Diverse
Activities → its 2), RTL-mirrored.

> Note: this means some mid-size laptops (e.g. 1366px) now get the drawer instead of a *cramped,
> wrapped* horizontal bar. That is a deliberate trade-off — the previous horizontal state at those
> widths was the broken two-line layout in the screenshot; a clean drawer is strictly better. The
> only structural change is the header spanning up to 1440px (wider than the 1200px content column)
> so the one-line nav has room.

---

## Verification (headless Chrome screenshots)

| Case | Result |
|---|---|
| **Desktop AR 1568** (user's width) | ✅ All items one line incl. المصادر الرقمية / فعاليات متنوعة; brand one line; balanced |
| **Desktop AR/EN 1440** | ✅ One line, no overlap, comfortable spacing |
| **Desktop EN 1568** | ✅ Clean (English labels are wider; still fits) |
| **Dropdown connection** (forced open, AR + EN) | ✅ Panel flush to trigger — no gap/dead-zone |
| **Tablet/drawer EN 1300, AR 1024** | ✅ Hamburger visible, nav hidden; drawer opens; dropdowns inline; RTL correct |
| **No text wraps** | ✅ at every tested width |
| **No overlap / no nav shift** | ✅ (the 1440 breakpoint guarantees the one-line nav only renders when it fits) |
| **Hover → move to menu → stays open** | ✅ by construction (flush panel + hover bridge); can't be shown in a static image but the gap that caused closing is removed |
| **Build** | ✅ 0 errors |
| **Tests** | ✅ 33/33 |

Light theme verified; the changes are layout-only and theme-agnostic (driven by the existing CSS
variables), so dark mode is unaffected.

---

## Exact changes

**`public.css`**
- `.nav-inner`: `max-width: 1440px` (header wider than the 1200 content column).
- `.brand-text`: `white-space: nowrap`.
- `.nav-menu a` & `.nav-dropdown-toggle`: `display:inline-flex; align-items:center; min-block-size:44px; white-space:nowrap; padding-inline:.5rem` (equal full-height chips, no wrap).
- `.nav-dropdown-menu`: `inset-block-start: 100%` (gap removed).
- `.nav-dropdown::after`: hover bridge (disabled in the drawer query).
- `.nav-burger { display:none }` + shown inside the `≤1439.98px` query (decoupled from Bootstrap breakpoints).
- Menu/tagline media queries: `1199.98px` → `1439.98px`.

**`_PublicLayout.cshtml`**
- Hamburger button class `d-xl-none` → removed (visibility now handled by the CSS breakpoint above).

---

### Awaiting review. No commit, no tag, no push.
