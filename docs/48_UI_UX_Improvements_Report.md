# 48 — Public Website UI/UX Improvements

**Date:** 2026-06-19
**Scope:** public layer only. Implemented **Critical + High** items from report 47. No redesign.
**Preserved (unchanged):** color palette, branding, architecture, routes, database structure.
**Not done (per instructions):** no commit, no tag, no push, no new stage.

---

## 1. What Was Implemented

### 🔴 C1 — Mobile/tablet navigation (was broken)
- **Root cause:** the burger was `d-md-none` (hidden ≥768px) while `.nav-menu` was hidden via a `max-width:860px` query → **no navigation existed between 768–860px**, and the 10-item menu crowded 768–991px.
- **Fix:** unified on a **single 992px breakpoint** — burger is now `d-lg-none` (visible <992) and `.nav-menu` is hidden <992 / inline ≥992. The mobile menu is a proper dropdown panel: full-width, **scrollable** (`max-block-size: calc(100dvh - nav-h)`), larger tap targets (`0.8rem` padding, `min 38px`), and closes on link click, outside click, or **Escape** (focus returns to the toggle). `aria-expanded` now reflects state; `aria-controls="primary-nav"` wired.

### 🟠 H1 — Skip to content
- Added a localized `.skip-link` (`Skip to content` / `تخطٍّ إلى المحتوى` / `Aller au contenu`) that appears on focus and jumps to `#main` (now `tabindex="-1"`). WCAG 2.4.1.

### 🟠 H2 — Visible keyboard focus
- Added `:focus-visible` outlines (3px accent ring, 2px offset) across links, buttons, `.btn`, `.icon-btn`, `.lang-link`, `.filter-tab`, accordion buttons, and form fields; dark-theme variant. WCAG 2.4.7.

### 🟠 H3 — Theses table responsiveness
- Added a `<caption class="visually-hidden">`, `scope="col"` on headers, and `data-label` on every cell. Below **640px** the table reflows into **stacked cards** (label/value rows) — no more horizontal scrolling for the 57-row dataset. Empty case now renders a clean `.empty-state` instead of a one-cell row.

### 🟠 H4 — Research empty state
- Replaced the bare "No items" line with a proper empty state: heading (*No research papers published yet*), explanatory line (*peer-reviewed articles are under Publications*), and a **primary CTA to the Publications page** — so the nav item no longer leads to a dead end. Global `.empty-state` restyled (dashed card, centered, headline).

### 🟠 H5 — Mobile menu UX / ARIA
- Covered with C1: `aria-expanded`, larger targets, scroll, Escape/outside-close. Language links now carry `hreflang`/`lang`/`aria-current`; the active nav link has `aria-current="page"`.

### 🟠 H6 — Color contrast (AA)
- Introduced `--accent-ink` (`#7D5E16` light ≈ **5.7:1** on the page background; `#E0C27A` dark) and repointed all **small-text** accent usages — eyebrows, hero eyebrow, `.pub-year`, `.tl-period` — to it. The gold `--accent-600` is retained for fills/borders/hovers, so **branding is unchanged** while small text now meets WCAG AA.

### Requested "improve" items folded in (low-risk)
- **Header behaviour:** elevation shadow on scroll (`.site-nav.scrolled`, toggled by JS).
- **Hero:** centers on mobile (text, eyebrow, CTAs) and the portrait is centered/constrained.
- **Footer:** quick-links wrapped in a `<nav aria-label="Explore">` landmark and the heading relabeled from the mis-used "Home" to a localized **Explore** (`استكشف` / `Explorer`); the top `<nav aria-label>` added to the header bar too.

---

## 2. Files Changed (public layer only)

| File | Change |
|---|---|
| `wwwroot/css/public.css` | `--accent-ink` token; focus-visible; skip-link & `.visually-hidden`; unified 992px nav breakpoint + scrollable mobile panel; hero mobile centering; header scroll shadow; larger lang targets; responsive theses table (≤640px stacked); improved `.empty-state` |
| `wwwroot/js/public.js` | burger `aria-expanded` toggle; close on link/outside/Escape; header scroll-elevation listener |
| `Pages/Public/Shared/_PublicLayout.cshtml` | skip link; `<nav>` landmarks (header + footer); `id="primary-nav"`; burger `d-lg-none` + ARIA; `aria-current` on nav & lang links; `hreflang`/`lang` on lang links; footer "Explore" heading; `main tabindex="-1"` |
| `Pages/Public/Theses.cshtml` | `<caption>`, `scope`, `data-label` cells, clean empty branch |
| `Pages/Public/Research.cshtml` | meaningful empty state + CTA to Publications |
| `Resources/SharedResource.{resx,ar,fr}` | new keys: `Skip_To_Content`, `Footer_Explore`, `Research_EmptyTitle`, `Research_EmptyBody` |

No changes to routes, page models, domain, persistence, or other backend code.

---

## 3. Verification

- **Build:** 0 warnings / 0 errors. **Tests:** 14/14.
- **All 10 pages × 3 cultures (ar/en/fr) = 30 responses → HTTP 200.**
- **Markup confirmed in rendered HTML:** skip-link, header/footer `<nav>` landmarks, `id="primary-nav"`, burger `d-lg-none` + `aria-expanded`, `aria-current`, theses `<caption>` + `data-label` cells, Research empty-state with CTA.
- **CSS confirmed served:** `--accent-ink`, `:focus-visible`, the `991.98px` nav breakpoint, the `640px` stacked-table breakpoint.

**Breakpoint behaviour (verified by CSS/DOM analysis at the four target widths — no screenshot tooling available in this environment):**

| Width | Result |
|---|---|
| **375px (mobile)** | Burger shown; menu hidden until toggled (scrollable, large targets); hero stacks & centers; theses → stacked cards; cards/grids single-column. |
| **768px (tablet)** | Burger shown, menu toggDLEs — **the former 768–860px dead zone is gone**; two-column page sections stack via Bootstrap `lg`; theses table scrolls within its card (>640). |
| **1366px (laptop)** | ≥992 → full inline nav (no wrap); hero 2-column; multi-column grids; container centered at 1200px. |
| **1920px (large desktop)** | As laptop; content centered at 1200px max with balanced gutters. |

RTL (ar) and LTR (en/fr) both use logical properties throughout (`inset-inline`, `margin-inline`, `text-align:start`), so all fixes apply symmetrically; the Bootstrap RTL stylesheet is still selected for Arabic.

---

## 4. Deferred (Medium / Low — documented in report 47, not implemented)

Medium: remove duplicate eyebrow/h2 on Home (M1); per-culture font loading (M3); style memberships as chips (M4); Teaching section-head consistency (M5); remove CTA-band content redundancy (M7). Low: pagination ellipsis (L1); breadcrumb separator styling (L2); theme-toggle `aria-pressed`/icon state (L3); form `autocomplete` (L4).

These are safe to address in a later pass; none affect core usability or accessibility compliance.

---

**⏸ Critical + High UI/UX fixes complete, confined to the public website layer. No commits/tags/pushes; no new stage. Awaiting your review.**
