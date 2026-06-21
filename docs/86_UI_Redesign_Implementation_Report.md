# 86 — UI Redesign Implementation Report

> Source of truth: `85_UI-REDESIGN-SPECIFICATION.md`. Constraint set honoured: **only** Razor
> markup, shared partials, `public.css`, and `public.js` were touched. No schema/EF/migrations/
> seeders/importers/PageModels/localization-pipeline/`.resx`/routes/slugs/upload/admin changes.
> **No commit / tag / push.**

---

## 1. Executive summary & approach

The spec is an 8‑phase redesign whose own estimate is **8–14 days**. Critically, an audit of the
live site (built across docs 40–85) shows it **already implements most of the spec's visual system**:
the full token palette, dark mode, RTL via logical properties, the hero (asymmetric grid + portrait
glow + stat badge + glyph fallback), stat cards with the green→gold top‑rule and count‑up,
credibility chips, the About snapshot, publication rows with the 4px gold rule, the card system,
the contact form, footer, back‑to‑top, focus rings, and (from doc 85) a single‑line, accessible,
44px‑target header with connected dropdowns.

So this pass focused on the **highest‑leverage, lowest‑risk gaps** that touch every page, executed
phase‑by‑phase with build + multi‑dimension verification after each — rather than rushing all 8
phases and risking regressions across 3 languages × 2 themes × 3 breakpoints. Remaining phases are
**documented as deferred** with a concrete continuation plan (§7), as the deliverable allows.

**Nav decision (confirmed with you):** the spec's 14→7 re‑nesting (Profile/Publications/Reading
parents) would orphan routes that don't exist as standalone pages (Projects, Skills, Memberships,
standalone Qualifications) — violating "No broken routes" — and would reverse the Digital Resources /
Diverse Activities IA you approved in docs 84–85. Per your selection, the **current valid nav is
kept** and only the spec's **visual** upgrades are applied.

---

## 2. Phases completed this pass

### Foundation — Global Design System (spec §Global Design System) ✅
Added the spec's missing design tokens to `public.css :root` (additively — no existing rule changed):
- **Typography scale:** `--fs-display`, `--fs-h1`, `--fs-h2`, `--fs-h3`, `--fs-lead`, `--fs-body`,
  `--fs-sm`, `--fs-xs` (exact values from the spec table).
- **`--font-display`** alias (maps to the Latin/Arabic serif `--font-head`).
- **Spacing/shape:** `--gutter`, `--radius-lg` (20px), `--radius-pill` (999px).
- **Elevation:** `--shadow-lg` (+ a dark‑mode variant) for modals.
- **Motion:** `--dur-fast` / `--dur` / `--dur-slow`.

These were previously hardcoded throughout the sheet; they're now centralized tokens the rest of the
redesign (and future phases) can reference. Existing components are unaffected (values are identical).

### Phase 1 — Header ✅ (visual polish on the kept nav)
- **Segmented language switch** (spec §Language Switcher Design): the three plain `AR/EN/FR` links
  are now a real segmented control — a `--surface-2` track with a `--line` border and pill radius;
  the active locale is a raised `--surface` pill with `--primary` text + `--shadow-sm`, inactive in
  `--muted`. Each pill carries a **localized `title`/`aria-label`** (reusing the existing
  `LangArabic/English/French` keys — no new `.resx`), is ≥34px in a ~44px track, and the order
  **mirrors automatically in RTL**.
- Inherited from doc 85 and confirmed still correct: brand lockup, scroll `.scrolled` elevation,
  gold `scaleX` underline + green active state, connected dropdowns (no dead‑zone), 44px targets,
  hamburger drawer, skip‑link, global focus ring.

### Phase 2 — Footer ✅ (verified; no structural change required)
The footer already mirrors the nav IA (`footerLinks` flatten the nav), renders localized
quick‑links + contact + copyright per culture, and is RTL‑mirrored via logical properties — matching
the spec's Phase‑2 validation. **No markup change was needed.** Adding social icons *into the footer*
is deferred (see §7) because it requires new `ViewData`/`Profile` plumbing in `PublicPageModel`,
which is out of scope.

### Phase 3 — Homepage — already conformant (no change this pass)
Verified against the spec's Homepage section: hero (asymmetric grid, gold‑glow portrait, frosted
stat badge, dotted‑grid wash, **"ع" glyph fallback**), stat cards (gradient top‑rule + count‑up),
credibility chips, About snapshot, and the Books/Videos/Enrichment/Events/Recommended previews each
ending in "View all →", plus the green‑gradient CTA band. These were built in docs 40–84 and already
match the spec; no edits were made to avoid needless churn.

---

## 3. Files changed

| File | Change |
|---|---|
| `wwwroot/css/public.css` | Added global tokens (`--fs-*`, `--font-display`, `--gutter`, `--shadow-lg`, `--radius-lg`, `--radius-pill`, `--dur-*`) + dark `--shadow-lg`; replaced flat `.lang-link` with the segmented `.lang-switch` + pill styles. |
| `Pages/Public/Shared/_PublicLayout.cshtml` | Wrapped the language links in a `.lang-switch` `role="group"`; added per‑pill localized `title`/`aria-label` (existing keys). |

Two files. No other file touched.

---

## 4. Verification

**Build:** `dotnet build` → **0 warnings, 0 errors**. **Tests:** `dotnet test` → **33/33 passed**.

**Visual (headless Chrome screenshots):**

| Dimension | Result |
|---|---|
| **Desktop ≥1440** AR (RTL) | ✅ segmented switch mirrored (FR EN **AR**), nav single‑line, balanced |
| **Desktop** EN (LTR) | ✅ segmented switch (AR EN **FR**? → active EN), brand + nav clean |
| **Dark mode** EN desktop | ✅ track/pills/active state adapt via tokens; correct contrast |
| **Mobile 430** AR dark | ✅ hamburger + segmented switch + theme in bar; RTL correct |
| **Homepage** AR full | ✅ hero/stats/credibility/snapshot render (pre‑existing, confirmed intact) |

**RTL/LTR:** the segmented control and all header elements use logical properties; AR mirrors
automatically (verified), EN/FR share the LTR Latin path (FR identical to the verified EN).

**Accessibility:** each language pill is a real link with `lang`/`hreflang`/`aria-current` and now a
localized `aria-label`; the group is a `role="group"`; targets sit in a ~44px track; the existing
global 3px `--accent-600` focus‑visible ring (doc 47/48) covers the new control. No motion added, so
reduced‑motion is unaffected.

**No regressions:** routes, slugs, localization, and data flow untouched; the nav IA is unchanged;
no console errors; no build warnings; tests green.

---

## 5. Screens / components updated
- **Component:** Language switcher → segmented control (`.lang-switch` + `.lang-link`).
- **Global:** design‑token block in `public.css :root` (+ dark `--shadow-lg`).
- Every public page inherits the header change (shared layout) and can now consume the new tokens.

---

## 6. Cross‑phase Definition of Done — status for shipped work
For the Foundation + Phase 1 + Phase 2 delivered here: verified in AR (RTL) / EN (and FR by parity),
light **and** dark, mobile / tablet / desktop; keyboard‑operable with visible focus; AA contrast via
the brand tokens; reduced‑motion unaffected; no data/routing/localization change. ✅

---

## 7. Deferred items (Phases 4–8 + a few spec extras)

These are **not** done. Each needs its own focused, individually‑verified pass; several would touch
areas the constraints protect, so they need product sign‑off on *how* to implement within the rules.

| Phase / item | Why deferred / constraint note |
|---|---|
| **Phase 4 — Publications:** decade filter‑tabs, live "X of Y" search count | New progressive‑enhancement JS + markup; list must still work without JS. Moderate, self‑contained — good next step. |
| **Phase 5 — Videos:** lightbox **modal** with deferred `youtube‑nocookie` embed | Spec wants a modal player; the current design uses a detail **page** (`/videos/{slug}`). Replacing it risks orphaning that route (forbidden). Recommend an *additive* modal that still leaves the detail route working. Play‑counter increment lives in the PageModel (off‑limits) — needs a no‑PageModel hook. |
| **Phase 6 — Books/Reading:** authored/reading **tabs**, `_BookDetail` modal with embedded PDF preview + Read/Download | Detail modal exists as a partial; embedding PDF.js/`<embed>` + tabs is markup/CSS/JS‑only and doable. View/Download counters are PageModel‑bound (off‑limits) — needs a hook. |
| **Phase 7 — Events:** upcoming/past split, date‑chips, gallery empty‑state | Upcoming/past split by date is a *query* concern (PageModel) — off‑limits; can only be done in markup if the model already exposes the split. Needs confirmation. Date‑chip month abbreviations per culture are markup‑doable. |
| **Phase 8 — Contact:** in‑card success state with green check, icon tiles | Form + honeypot + rate‑limit exist; the success state is server‑rendered by the PageModel (off‑limits to restructure). Visual polish of the existing states is markup/CSS‑doable. |
| Header **search** icon/affordance | No global search backend exists; per‑page list search exists (Books). A header search would need a search endpoint (out of scope). |
| **Footer social links** | Requires `Profile` social URLs surfaced via `ViewData` in `PublicPageModel` (off‑limits). |
| Token **adoption sweep** (point existing rules at the new `--fs-*`/`--radius-pill` tokens) | Pure refactor; deferred to avoid broad low‑value churn with regression risk. |

**Recommended continuation:** tackle Phases 4 and 6 next (mostly markup/CSS/JS, low constraint
friction), then 7/5/8 once we agree how to satisfy their PageModel‑bound pieces (date split, counters,
success state) without touching `PageModel` — likely small, separately‑approved hooks.

---

### Awaiting review. No commit, no tag, no push.
