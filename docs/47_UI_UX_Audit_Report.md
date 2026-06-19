# 47 — Public Website UI/UX Audit

**Date:** 2026-06-19
**Reviewers' lenses:** Senior UX · Senior UI · Frontend Architect · Accessibility.
**Scope:** public website only (`Pages/Public/*`, `wwwroot/css/public.css`, `wwwroot/js/public.js`, `_PublicLayout.cshtml`). No backend.
**Method:** static review of layout/CSS/JS + DOM structure of every page, evaluated at 375 / 768 / 1366 / 1920 px and in RTL (ar) / LTR (en, fr).

---

## 1. Cross-Cutting Findings (apply to all pages)

| ID | Area | Severity | Finding |
|---|---|---|---|
| X1 | Mobile/tablet nav | **Critical** | Breakpoint mismatch: the burger is `d-md-none` (hidden ≥768px) but `.nav-menu` is hidden via a custom `max-width:860px` query. **Between 768–860px there is NO navigation at all** (both hidden). The 10-item menu also wraps/crowds 768–991px. |
| X2 | Mobile menu behaviour | **Critical** | Open menu has no `aria-expanded`, no close affordance beyond re-tap, no max-height/scroll; tap targets (`.nav-menu a` ≈ 0.4rem padding, `.lang-link` ≈ 0.35rem) are well under the 44px guideline. |
| X3 | Keyboard focus | **High** | No `:focus-visible` styles anywhere. Custom buttons/cards/links show no visible focus ring → fails WCAG 2.4.7. |
| X4 | Skip link | **High** | `<main id="main">` exists but there is no "skip to content" link → fails WCAG 2.4.1; keyboard users tab through 10 nav items on every page. |
| X5 | Color contrast | **High** | `--accent-600 #A8842F` used for **small** text (eyebrows, `.tl-period`, `.pub-year`, `.lang-link`) ≈ 3.5:1 on light surfaces → fails AA (4.5:1) for normal-size text. |
| X6 | Tables a11y | Medium | `.theses-table` has no `<caption>`; header cells lack scope. |
| X7 | Heading hierarchy | Medium | On Home, several sections repeat the same string as both `.eyebrow` and `<h2>` (e.g. "A career in numbers", "About") → redundant. Footer uses `<h2>` "Home" as the quick-links heading (mis-labeled, duplicate landmark heading). |
| X8 | Fonts performance | Medium | All 5 font families (Amiri, Cairo, Cormorant, IBM Plex Sans Arabic, Inter) load on every page though only 2 are used per culture; render-blocking `<link>`. |
| X9 | Header behaviour | Medium | Sticky header has a static border but no elevation change on scroll; on long pages it reads flat against content. |
| X10 | Crumbs separator | Low | Breadcrumb " / " is a literal text node (not styled, reads awkwardly in RTL). |
| X11 | Theme toggle state | Low | Toggle glyph never changes and has no `aria-pressed`. |

---

## 2. Page-by-Page

### Home (`/{c}`)
- **UI/Visual hierarchy:** duplicate eyebrow≈h2 in Stats & About sections (X7). Hero portrait is an empty placeholder (giant "ع" glyph) — reads as missing content (no real photo in data). **CTA band** repeats the hero's name + positioning (content redundancy).
- **Layout/Responsive:** `hero-grid` 1.2fr/0.8fr collapses to 1 col ≤860px but text/portrait are left-aligned and the 320px portrait floats awkwardly; not centred on mobile.
- **A11y:** stat counters animate from 0 — fine, but final value only set via JS; with JS off the IntersectionObserver fallback covers it (OK).
- Severity: **High** (hero mobile balance, redundant headings — folded into X7), Medium (CTA redundancy).

### About (`/{c}/about`)
- Solid two-column → stacks well. `<dl>` meta rows clean. Qualifications timeline good.
- **UX:** skills chips and languages panel fine. Minor: aside panels could lead on mobile after bio (order acceptable).
- Severity: Low.

### Experience (`/{c}/experience`)
- Timeline good. **Memberships** rendered as bare `<li>` text lines (chip-set overridden to column) — low visual treatment, reads as flat list. Medium (visual/content density).
- Severity: Medium.

### Teaching (`/{c}/teaching`)
- Two `<h2>` ("Undergraduate"/"Graduate") are bare (no eyebrow/section-head) — inconsistent with the page-hero/eyebrow pattern used elsewhere (component consistency). Items reuse `.pub-item` (a publication component) for courses — semantically loose. Medium.
- Severity: Medium.

### Books (`/{c}/books`)
- Card grid responsive (auto-fill minmax 260px). Covers are CSS gradient + first glyph — consistent. **Pagination** lists every page number (no ellipsis) — fine at 2 pages, would overflow with many. Low.
- Severity: Low.

### Publications (`/{c}/publications`)
- Clean list; `.pub-year` accent contrast (X5). Otherwise good.
- Severity: High (only via X5).

### Research (`/{c}/research`)
- **UX/IA:** renders an **empty page** (0 `ResearchPaper`) with only "No items to display." A primary nav item leading to a dead/empty page is poor IA. **High.**
- Severity: **High**.

### Theses (`/{c}/theses`)
- **Responsive:** 5-column table in `.table-wrap` overflow-x → **horizontal scroll on mobile** for the site's largest dataset (57 rows); long Arabic titles make columns very wide. Poor mobile readability. **High.**
- **A11y:** no caption (X6); filter tabs are `<button>` (good) but the active filter isn't announced.
- **Content density:** 57 rows unpaginated — acceptable for a reference table, but heavy on mobile.
- Severity: **High**.

### Activities (`/{c}/activities`)
- Accordion (7 groups). First open. Items as `<ul><li>`. Works; counts badge nice. Long group (20 training items) is fine collapsed.
- **A11y:** Bootstrap accordion provides aria; OK. Focus ring missing (X3).
- Severity: Medium (focus only).

### Contact (`/{c}/contact`)
- Info panel + form. Labels present, validation wired, honeypot hidden. **UX:** inputs lack `autocomplete` (name/email). Info panel reuses `.footer-contact` class (semantically loose). PRG success alert good.
- Severity: Medium/Low.

---

## 3. Prioritized Issue List

### 🔴 Critical
- **C1 (X1+X2):** Mobile/tablet navigation is broken — dead zone 768–860px, breakpoint mismatch, non-accessible/oversized-target menu with no `aria-expanded`. Core navigation fails on phones/tablets.

### 🟠 High
- **H1 (X4):** Add skip-to-content link.
- **H2 (X3):** Add visible `:focus-visible` indicators across interactive elements.
- **H3 (Theses):** Make the theses table responsive (stacked card layout on mobile + `<caption>`).
- **H4 (Research):** Replace the dead empty page with a meaningful empty state that points to Publications; improve the global empty-state styling.
- **H5 (X2):** Mobile menu UX — `aria-expanded` toggling, larger tap targets (≥44px), scrollable panel, single breakpoint.
- **H6 (X5):** Fix accent small-text contrast to meet AA (introduce a darker `--accent-ink` for small text; keep the gold for fills/borders → branding preserved).

### 🟡 Medium (documented; not implemented this pass)
- M1 (X7) Remove duplicate eyebrow/h2 on Home; relabel footer quick-links heading.
- M2 (X9) Header elevation on scroll.
- M3 (X8) Load fonts per culture.
- M4 (Experience) Style memberships as chips/cards.
- M5 (Teaching) Use a consistent section-head + dedicated course component.
- M6 (X6) Header-cell `scope`, table caption (caption covered by H3).
- M7 (Home) Remove CTA-band content redundancy.
- M8 (hero) Centre hero on mobile, improve portrait placeholder.

### 🟢 Low (documented)
- L1 Pagination ellipsis for many pages. L2 Breadcrumb separator styling/RTL. L3 Theme-toggle `aria-pressed` + icon state. L4 Contact `autocomplete` attributes.

---

## 4. Implementation Plan (this pass: Critical + High only)

Per instructions, only **C1 + H1–H6** are implemented, confined to the public layer, preserving palette, branding, architecture, routes, and DB. A few explicitly-requested "improve" items that are cheap and ride along with High fixes (header scroll elevation, hero mobile centring, footer landmark/relabel, empty-state styling) are included as low-risk enhancements. Everything else (Medium/Low) is left for review. See **48_UI_UX_Improvements_Report.md**.
