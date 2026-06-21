# UI Redesign Specification — Prof. Aly Hussein Academic Portfolio

**Target stack:** ASP.NET Core Razor Pages · Bootstrap 5 · server-rendered · trilingual (Arabic RTL / English / French)
**Canonical theme file:** `src/ProfAly.CMS.Web/wwwroot/css/public.css`
**Shared layout:** `src/ProfAly.CMS.Web/Pages/Public/Shared/_PublicLayout.cshtml`
**Status:** Implementation-ready. This document is the single source of truth for the visual redesign. It preserves the existing data model, routing, and localization — it changes only presentation (markup structure, CSS, and component composition).

> **Golden rule for the implementing agent:** Do **not** touch controllers, `PageModel` classes, Entity Framework, the localization pipeline (`SharedResource`, `Localized.Pick`), routing, or upload handling. Edit Razor *markup* and `public.css` only. When a value below names a CSS custom property (e.g. `--primary`), it already exists or should exist in `public.css :root`; reuse it rather than hardcoding.

---

# Project Overview

## Purpose of the website
A self-managed, trilingual academic presence for **Prof. Aly Hussein**, Professor of Curriculum & Teaching Methods. It centralizes his complete academic record — biography, experience, publications, books, theses, teaching, educational videos, recommended reading, events, and contact — in Arabic (primary, RTL), English, and French. The professor maintains every piece of content through an admin dashboard with no developer involvement.

## Design goals
1. **Match the scholarship's authority.** The interface should feel as credible and established as a 30-year academic career.
2. **Make breadth legible.** 14 books, 9 publications, 57+ theses must be scannable in seconds, not hunted line by line.
3. **Be genuinely trilingual.** Every surface works in Arabic RTL with correct mirroring, and in English/French LTR.
4. **Preserve a coherent brand.** Deep Al-Azhar green + brass gold, scholarly serif headings, warm paper neutrals — refined, not replaced.
5. **Stay lightweight.** Server-rendered HTML/CSS only; no client framework added. Enhancements are progressive.

## User experience goals
- A visitor understands **who this is** and **what they do** within the first viewport.
- Any single publication, book, or video is reachable in **≤ 2 interactions** from the homepage.
- One **clear primary action** per page (read work, download CV, or get in touch).
- **Accessible by default:** keyboard navigable, visible focus, ≥ 44px touch targets, AA contrast, reduced-motion respected.
- **No "broken" states:** missing photos/PDFs render as designed fallbacks, never as errors.

---

# Current Design Problems

### Header
- Brand mark, name, and tagline lack a clear lockup; the identity does not anchor the page.
- The header carries too many actions at one visual weight (search, theme, language, full menu), so nothing reads as primary.
- On scroll the header does not visibly separate from content, reducing wayfinding.

### Navigation
- **14 top-level items** (Home, About, Experience, Activities, Research, Publications, Books, Theses, Projects, Teaching, Videos, Enrichment, Resources, Contact). This overflows the bar on laptops and overwhelms users.
- No information hierarchy — primary destinations sit at the same level as niche ones.
- The active-page indicator is subtle and easily missed.

### Dropdown menus
- The single "Digital Resources" dropdown behaves inconsistently between hover, focus, and click; its open/close timing feels unpredictable.
- In the mobile drawer the dropdown is always expanded, adding length without grouping benefit.
- Dropdown items lack clear affordance that they are a grouped set rather than siblings of the top nav.

### Typography
- Heading and body sizes sit too close together, flattening hierarchy; sections all read at one "volume."
- Display type is under-scaled for a portfolio — the name and section titles should command more presence.
- Eyebrow/label treatment is inconsistent across pages.

### Layout
- Content lists (publications, theses, books) are dense and undifferentiated, with weak scanning structure.
- Hero composition under-uses the available space; the portrait and positioning statement compete rather than complement.
- Section rhythm is uneven — some sections crowd, others float.

### Spacing
- Vertical rhythm between sections is inconsistent; padding does not scale smoothly across breakpoints.
- Card interiors are tight, reducing readability.
- Gutters collapse awkwardly on tablet widths.

### Visual hierarchy
- No single primary call-to-action per page; contact, CV, and key work blend into the page.
- Gold accent is applied broadly rather than reserved for genuine emphasis, diluting its signal.
- Stat figures, which should be a highlight, are not given enough scale or separation.

### Mobile responsiveness
- The 14-item menu produces a long, flat drawer.
- Touch targets for the language switcher and icon buttons fall below the 44px minimum.
- The theses table is hard to read on small screens without a card fallback.
- Some Arabic RTL spacing and iconography do not mirror, breaking the reading flow.

---

# Design Principles

1. **Editorial calm.** Generous whitespace, a strict eyebrow → title → content rhythm, and restraint in color. The page should feel like a well-set journal, not a dashboard.
2. **Confident scholarship.** Large Cormorant Garamond display type and prominent stat numerals carry the brand's authority.
3. **One path at a time.** Each view has a single, obvious primary action. Gold marks only true emphasis.
4. **Scan before read.** Long records get filters, tabs, grouping, and a unified card system so visitors orient before committing to read.
5. **Trilingual and inclusive by construction.** RTL is a first-class layout via CSS logical properties; accessibility (focus, contrast, target size, reduced motion) is built in, not bolted on.
6. **Honest empty states.** Absent media becomes a branded, intentional placeholder (the Arabic ʿayn "ع" glyph on a green gradient), never a broken frame.

---

# Global Design System

All values below live in `public.css :root` as CSS custom properties. The redesign **keeps the existing token names** (they are already referenced throughout the stylesheet) and tightens their application.

## Colors

### Brand — Primary (deep Al-Azhar green)
| Token | HEX | Use |
|---|---|---|
| `--primary` | `#0B5D3B` | Identity, primary buttons, links, active nav |
| `--primary-700` | `#084229` | Primary hover/pressed, gradients |
| `--primary-300` | `#2F8460` | Gradient stop, hover borders |
| `--primary-050` | `#E9F2ED` | Tints, focus ring, skill chips, hero wash |

### Brand — Accent (brass / scholarly gold)
| Token | HEX | Use |
|---|---|---|
| `--accent` | `#C8A45D` | Eyebrows, emphasis, "Featured", CTA gold buttons, dots |
| `--accent-600` | `#A8842F` | Gold hover, link hover, accessibility focus ring |
| `--accent-ink` | `#7D5E16` | Eyebrow text on light surfaces |
| `--accent-050` | `#F7F0DF` | Gold tints (e.g. MA badge) |

### Neutrals & surfaces (warm — never cold grey)
| Token | HEX | Use |
|---|---|---|
| `--bg` | `#F8F8F8` | Page background |
| `--surface` | `#FFFFFF` | Cards, nav, panels |
| `--surface-2` | `#F1F0EC` | Alternating sections, sunken areas |
| `--ink` | `#1F2937` | Primary text |
| `--muted` | `#5B6472` | Secondary text |
| `--line` | `#E4E2DB` | Default borders |
| `--line-strong` | `#D4D1C7` | Input borders, stronger dividers |

### Dark mode (`[data-theme="dark"]`)
| Token | HEX |
|---|---|
| `--bg` | `#0E1512` |
| `--surface` | `#15201B` |
| `--surface-2` | `#111A16` |
| `--ink` | `#ECECE6` |
| `--muted` | `#A6B0AB` |
| `--line` | `#243029` |
| `--line-strong` | `#324034` |
| `--primary-050` | `#14241C` |
| `--accent-050` | `#241F12` |
| `--accent-ink` | `#E0C27A` |

**Usage rules:** Green = identity + primary actions. Gold = emphasis only (eyebrows, the 4px left rule on publication rows, the stat top-rule, "Featured" flags, the CTA gold button). White on green and `#1F1605` on gold for text. Maintain AA contrast (≥ 4.5:1 for body text).

## Typography

### Font families
| Role | Latin (EN/FR) | Arabic (RTL) | Token |
|---|---|---|---|
| Display / headings | **Cormorant Garamond** (serif) | **Amiri** (serif) | `--font-head` / `--font-display` |
| Body / UI | **Inter** (sans) | **IBM Plex Sans Arabic** (fallback Cairo) | `--font-body` |

Families swap automatically via the `[data-lang="ar"]` scope. French uses the Latin stack, LTR. Fonts load from Google Fonts (Cormorant Garamond, Inter, Amiri, IBM Plex Sans Arabic); if self-hosting is required, replace the `@import` with local `@font-face`.

### Sizes (fluid; clamp where noted)
| Token | Value | Role |
|---|---|---|
| `--fs-display` | `clamp(2.2rem, 5.5vw, 3.7rem)` | Hero name |
| `--fs-h1` | `clamp(1.8rem, 4vw, 2.7rem)` | Section / page title |
| `--fs-h2` | `clamp(1.5rem, 3vw, 2.1rem)` | Sub-section title |
| `--fs-h3` | `1.3rem` | Card / panel title |
| `--fs-lead` | `1.12rem` | Section lead, intro paragraph |
| `--fs-body` | `1.0625rem` (17px) | Base body |
| `--fs-sm` | `0.92rem` | Meta, captions |
| `--fs-xs` | `0.82rem` | Eyebrow, fine print |

### Weights
400 (regular), 500 (medium — nav, chips), 600 (semibold — buttons, labels), 700 (bold — headings, stat numerals).

### Heading hierarchy
- **Display (hero name):** Cormorant 700, line-height 1.04–1.18, letter-spacing `-0.01em`.
- **H1 (page/section title):** Cormorant 700, line-height 1.15.
- **H2:** Cormorant 700.
- **H3 (card/panel):** Cormorant 700, 1.3rem.
- **Eyebrow:** Inter 600, `0.8rem`, `letter-spacing: 0.14em`, uppercase, color `--accent-ink`, preceded by a 1.8rem × 2px gold rule. Always pair an eyebrow above a serif title to open a section.
- **Body:** Inter 400, 17px, line-height 1.7.

### Arabic RTL considerations
- Set `dir="rtl"` and `lang="ar"` and `data-lang="ar"` on `<html>` for the Arabic culture.
- Headings use **Amiri**; body uses **IBM Plex Sans Arabic**; line-height increases to **1.95**; letter-spacing resets to **0** (no negative tracking on Arabic).
- All directional spacing/borders use **CSS logical properties** (`margin-inline`, `padding-inline`, `inset-inline-start`, `border-inline-start`) so the layout mirrors automatically.
- Icons that imply direction (chevrons, arrows, the "back to list" affordance) must flip in RTL.
- Numerals and the stat counters remain LTR within RTL text.

## Spacing System
4px base scale: `4, 8, 12, 16, 20, 24, 32, 40, 48, 64`.
| Token | Value |
|---|---|
| Layout max width `--maxw` | `1200px` |
| Nav height `--nav-h` | `76px` |
| Section padding `--section-pad` | `clamp(3.5rem, 7vw, 6.5rem)` (vertical) |
| Page gutter `--gutter` | `clamp(1.1rem, 4vw, 2rem)` (horizontal) |

**Rhythm:** Sections alternate `--surface` / `--surface-2`. Section content is centered to `--maxw` with `--gutter` side padding. Card interiors use `clamp(1.4rem, 3vw, 2rem)`. Vertical gaps within a section step on the 4px scale (typically 16 / 24 / 40).

## Shadows
| Token | Value | Use |
|---|---|---|
| `--shadow-sm` | `0 1px 2px rgba(16,24,40,.05), 0 6px 18px -10px rgba(16,24,40,.10)` | Card at rest, chips |
| `--shadow` | `0 1px 2px rgba(16,24,40,.04), 0 12px 32px -12px rgba(16,24,40,.12)` | Card hover, forms, dropdown |
| `--shadow-lg` | `0 2px 4px rgba(16,24,40,.05), 0 24px 56px -20px rgba(16,24,40,.18)` | Modals |

One soft, layered family — never hard or colored. Dark mode deepens the same family. **Hover** raises elevation from `--shadow-sm` to `--shadow`.

## Border Radius
| Token | Value | Use |
|---|---|---|
| `--radius-sm` | `9px` | Inputs, small controls |
| `--radius` | `14px` | Default card |
| `--radius-lg` | `20px` | Hero portrait, CTA band, modals |
| `--radius-pill` | `999px` | Buttons, chips, badges, search, filter tabs |

## Animations
- **Easing:** single curve `cubic-bezier(.22, .61, .36, 1)` (`--ease`).
- **Durations:** fast `0.18s`, default `0.22s`, slow `0.3s`.
- **Hover (cards/list rows):** `translateY(-2px to -3px)` + elevation increase.
- **Hover (links):** color shifts from `--primary` to `--accent-600`.
- **Hover (nav links):** a 2px gold underline grows via `transform: scaleX(0 → 1)` from center.
- **Buttons:** primary/gold lift `translateY(-2px)` on hover; outline fills with a tint.
- **Focus:** 3px ring — `--primary-050` for inputs/controls, `--accent-600` outline for the audited accessibility ring (offset 2px).
- **Scroll reveals:** sections fade + rise 14px via IntersectionObserver; stat counters count up on entry.
- **All motion gated behind `@media (prefers-reduced-motion: no-preference)`**; reduced-motion shows final states immediately.

---

# Header Redesign

The header is the existing `.site-nav` (sticky, translucent surface with `backdrop-filter: blur(10px)`, bottom `--line` border, height `--nav-h` = 76px). Add a `.scrolled` class on scroll (already wired in `public.js`) that applies a subtle elevation shadow.

## Desktop Layout (≥ 1200px)
A single 76px bar at `--maxw`, three zones:
- **Start:** brand lockup — the 44px green-gradient glyph tile (`--brand-mark`, the "ع" or uploaded logo) + two-line text (name in Cormorant 600 ~1.12rem; tagline in `--muted` 0.72rem).
- **Center:** horizontal primary menu, 7 items, Inter 500 0.9rem, with the gold `scaleX` underline on hover and a green, semibold active state.
- **End:** actions — search icon button, theme toggle, and a segmented AR | EN | FR language switch. Icon buttons are 38–44px, bordered `--line`, hover border `--accent`.

## Tablet Layout (768px – 1199px)
- The horizontal menu collapses into a **hamburger drawer** (offcanvas), opened by a 44px icon button at the end.
- The brand tagline (`.brand-text .sub`) becomes visible in this range (room under the name).
- Actions (search, theme, language) remain in the bar; the menu lives in the drawer.

## Mobile Layout (< 768px)
- Brand lockup (glyph + name) at start; hamburger at end.
- Drawer slides from the inline-start edge, full menu as a vertical list with ≥ 44px rows; grouped sections (see Dropdown) shown as an indented, labeled subgroup.
- Language switch and theme toggle appear at the top of the drawer as full-width, ≥ 44px controls.

## Navigation Structure
Reduce 14 top-level items to **7 primary destinations**; nest the rest:

```
Home
Profile          (= About; nests Experience, Qualifications, Skills, Memberships)
Publications     (nests Research, Theses, Projects)
Videos
Reading          (= Books; nests Recommended Books, Enrichment, Resources)
Events
Contact
```

- Primary items are top-level links/buttons.
- "Profile", "Publications", and "Reading" act as **section parents**; their nested pages are reachable via an on-page sub-nav or in-page sections, and (optionally) a single grouped dropdown labeled **"More"** if any item must remain in the header.
- All labels stay **admin-editable and localized** via the existing Header management + `SharedResource`. Do not hardcode label text.

## Dropdown Design
A single optional **"More"** (or "Reading") dropdown for grouped/secondary destinations:
- **Container:** `--surface`, 1px `--line` border, `--radius` (12px), `--shadow`, min-width 220px, 0.4rem padding.
- **Reveal:** on **hover** and **focus-within** (desktop) and on **click** (sets `aria-expanded` + `.open`), with a 0.18s opacity + 6px translateY transition. A small caret rotates 180° when open.
- **Items:** block links, 0.55rem 0.7rem padding, `--radius-sm`, hover background `--primary-050` + green text. No underline pseudo-element on dropdown items.
- **Mobile:** dropdown renders **inline and expanded** inside the drawer (static position, no shadow), indented under its parent label.
- **Keyboard:** toggle is a real `<button>`; Escape closes; Tab cycles items; first item focusable on open.

## Language Switcher Design
- Segmented control of three pills (AR | EN | FR) inside a `--surface-2` track with a 999px radius, 1px `--line` border.
- Active language: `--surface` pill with `--primary` text and `--shadow-sm`; inactive: `--muted` text.
- Each target ≥ 38–44px. Switching maps to the equivalent localized slug/route (existing behavior); French missing content falls back to the default culture.
- In RTL the order and alignment mirror automatically.

## Dark Mode Toggle Design
- A single 38–44px bordered icon button (moon ↔ sun glyph).
- Toggles `[data-theme="dark"]` on the document; **persist client-side** (existing `public.js` behavior); first-visit default from `SiteSettings.DefaultTheme`.
- The icon swaps with the state; transition is instant (no flash). Respect the full dark token set.

## Accessibility Requirements
- Skip-to-content link (`.skip-link`) as the first focusable element, revealed on focus.
- Visible focus on every interactive element: 3px `--accent-600` outline, 2px offset (`--accent` in dark mode).
- All nav/menu/drawer/dropdown operable by keyboard; `aria-expanded`, `aria-current="page"` on the active link.
- Touch targets ≥ 44px. Color contrast AA. `lang`/`dir` set per culture.

---

# Homepage Redesign

Route `/{culture}/` → `Pages/Public/Home.cshtml`. Compose top-to-bottom; each preview pulls top-`SortOrder`/featured/recent items in the active culture and ends with a "View all →" link. Section visibility/order remain admin-controlled.

- **Hero (`.hero`):** asymmetric two-column grid (1.15fr / 0.85fr at ≥ 900px). Left: a pill hero-eyebrow (tagline with a gold dot), the name in `--fs-display` Cormorant, the title in `--primary` semibold, a ≤ 42ch positioning line in `--muted`, then two CTAs (primary "View publications" + outline "Contact me"). Right: a rounded (`--radius-lg`) portrait in a 4/4.4 frame with a gold offset glow behind and a frosted stat badge ("57+ Theses supervised"). Background: radial `--primary-050` wash top-corner over a faint 26px dotted grid masked to fade. **Portrait fallback:** the "ع" glyph on green gradient when no photo.
- **Statistics (`.stats-row` of `.stat-card`):** centered eyebrow + title, then auto-fit cards. Each card: green→gold 3px top rule, a large Cormorant numeral (`clamp(2.4rem,5vw,3.2rem)`) that **counts up** on scroll, and a `--muted` label. Source: `StatItem` (Books 14, Publications 9, Theses 57+, Years 30+).
- **Academic highlights / credibility (`.cred-row` of `.chip`):** a centered row of institution chips (each with a gold dot, hover lift). Source: `Credibility`.
- **About snapshot (`.snapshot-grid`):** two columns — a 5/4 photo (or glyph fallback) and a short bio with eyebrow, title, first bio paragraph, and an outline "Read full biography" → Profile.
- **Publications preview:** the 3–4 most recent `PublicationItem` rows (serif year + title + venue + 4px gold left rule), then "View all →".
- **Books preview (`.card-grid--4`):** 4 featured `BookCard`s (cover or gradient-spine fallback with initial + year; "Featured" flag), then "View all →".
- **Videos preview (`.grid-3` of video cards):** 3 latest video cards (16:9 gradient thumbnail + centered play button + title + duration), then "View all →".
- **Events preview:** 2–3 upcoming event cards (date chip + title + place + kind badge), then "View all →".
- **Contact CTA (`.cta-band`):** full-width green-gradient band, `--radius-lg`, centered title + supporting line + a single gold "Get in touch" button.

---

# Publications Page

Route `/{culture}/publications` → `Publications.cshtml` (also the pattern for Research).

- **Layout:** page-hero with breadcrumb + title ("Publications") + lead ("Nine peer-reviewed studies in respected scholarly journals."), then a single-column list at `--maxw`.
- **Cards (rows):** the `.pub-item` pattern — a large Cormorant **year** in the gutter, title (Cormorant 1.1rem), venue/authors line (`--muted`), a **4px gold left border**, `--radius`, `--shadow-sm` → `--shadow` and `translateY(-2px)` on hover. Inline action buttons (View PDF / DOI / external link) as small pills under the venue when sources exist.
- **Filters:** a pill **filter-tab** group by decade (All / 2020s / 2010s / 2000s) in a `--surface-2` track; active tab is green-filled. (For Theses, reuse the existing tabbed filter-table: All / Supervised / Examined / Ongoing + Master/PhD select + year sort.)
- **Search:** a rounded `.search-input` with a leading magnifier; filters the list by title/venue. Show a live "**X of Y publications**" result count above the list.
- **Responsive behavior:** rows stack naturally; the year stays in the inline-start gutter; on < 640px the year sits above the title. Filter tabs wrap; search becomes full-width.

---

# Educational Videos Page

Route `/{culture}/videos` → `Videos.cshtml`. YouTube is referenced (link/ID), never hosted; defer the iframe until interaction.

- **Layout:** page-hero, then an optional **featured video** (large thumbnail + title + short blurb + "Watch now"), then "All videos" as a `.grid-3`.
- **Cards (`.media-card`):** clickable; 16:9 thumbnail, title (Cormorant 1.05rem), meta line (kind · duration). Hover: `translateY(-3px)` + elevation; the play button scales up slightly.
- **Thumbnails:** use the YouTube thumbnail when available; otherwise a **green gradient** thumbnail with a centered circular play button and a duration chip (bottom inline-end, dark translucent).
- **Video modal behavior:** clicking a card opens a centered lightbox (scrim `rgba(15,20,17,.7)` + `blur(4px)`, modal `--radius-lg`, `--shadow-lg`). The modal embeds the deferred `youtube-nocookie` player (loaded only on open), with title, meta, a "Watch on YouTube" external link, and a Close action. Closes on scrim click, Close button, and Escape. Increment the existing **Play** event on open.

---

# Recommended Books Page

Routes: `Books.cshtml` (authored) and `RecommendedBooks.cshtml` (reading list) — present together with tabs, or as sibling pages sharing one layout.

- **Layout:** page-hero, optional **Authored books / Books worth reading** tab group (`.filter-tabs`), a `.search-input`, then a responsive `.card-grid` (auto-fill, min 240px).
- **Book cards (`.book-card`):** 3:2 cover; when no image, the **gradient-spine fallback** (green gradient, Cormorant initial, year bottom inline-end). Optional "Featured" flag (gold, top inline-start). Title (Cormorant 1.05rem) + meta (publisher/author/role, `--muted`). Hover lift + elevation.
- **Detail/popup (reuse `_BookDetail` partial):** modal with cover (or fallback spine), summary, an **embedded PDF preview** (PDF.js / `<embed>`), and **Read / Download** actions. Open increments `ViewCount`; download increments `DownloadCount`. Launches gracefully with no PDF/cover until uploaded.
- **Responsive behavior:** grid reflows 1 → 2 → 3–4 columns; the detail modal stacks cover above content under 560px; search becomes full-width.

---

# Events Page

Route `/{culture}/events` → `Events.cshtml` (with `EventDetail.cshtml`).

- **Upcoming events:** an eyebrow + "What's next" title, then a column of **event cards** filtered to upcoming, ordered soonest-first. Upcoming cards carry a solid-gold kind badge and a green primary "Event details" action.
- **Past events:** a `--surface-2` section, "Past events" title, the remaining cards with a neutral kind badge and a ghost "View recap" action.
- **Event cards (`.event-card`):** a left **date chip** (green month band over a Cormorant day numeral, bordered, `--radius` ~12px), then body — kind badge, title (Cormorant 1.18rem), place line with a location pin icon (`--muted`), and the action button. Hover: `translateY(-2px)` + elevation.
- **Gallery:** where an event has images, a `.gallery-grid` (auto-fill, min 160px) of thumbnails linking to the full image, with a subtle zoom-on-hover. When no images exist, a dashed-border "galleries appear here once images are uploaded" panel (designed empty state).

---

# Contact Page

Route `/{culture}/contact` → `Contact.cshtml`.

- **Layout:** page-hero, then a two-column `.contact-grid` (1fr / 1.1fr at ≥ 900px): an info aside and the message form.
- **Contact form (`.form-card`):** panel with `--shadow`, `--radius`, `clamp(1.6rem,4vw,2.4rem)` padding. Fields: Name, Email (two-up on ≥ 560px), Subject (full width), Message (full-width textarea, ≥ 160px). Inputs: `--surface`, 1.5px `--line-strong` border, `--radius-sm`; hover border `--primary-300`; focus `--primary` border + `--primary-050` ring. Submit: large green primary button (full width on mobile). Keep the existing honeypot + rate-limit; on success show the existing confirmation ("Your message has been sent. Thank you.") as an in-card success state with a green check.
- **Contact information section:** an aside with a portrait (or glyph fallback) and a panel listing Email / Phone / Location, each as a row with a 44px `--primary-050` icon tile (green icon), a `--muted` label, and a bold value. Below, social actions (Facebook, WhatsApp) as small outline buttons. All values come from Profile/Contact data — do not hardcode.

---

# Components Library

Reusable presentation primitives. Implement once (CSS classes in `public.css`; markup as Razor partials where shared) and compose everywhere. Each entry: **purpose / structure / states / hover.**

### Buttons (`.btn` + `.btn-primary` / `.btn-outline` / `.btn-gold` / `.btn-ghost`, sizes `.btn-sm`)
- **Purpose:** all calls to action.
- **Structure:** pill (`--radius-pill`), Inter 600, inline-flex with optional leading icon, 0.75rem 1.5rem padding (sm: 0.45rem 1.05rem; lg: 0.9rem 2rem, ≥ 54px).
- **States:** default, hover, focus (accessibility ring), disabled (opacity 0.5, no pointer).
- **Hover:** primary/gold deepen color + `translateY(-2px)`; outline fills with `--primary-050` and green border; ghost gets a `--surface-2` background.

### Cards (`.card` family: `.book-card`, `.media-card`, `.event-card`, `.stat-card`, `.panel`)
- **Purpose:** group related content.
- **Structure:** `--surface`, 1px `--line`, `--radius`, `--shadow-sm`; interior padding on the 4px scale.
- **States:** rest, hover, focus-within (for interactive cards).
- **Hover:** `translateY(-2px to -3px)` + elevation to `--shadow`. Publication rows add a 4px gold left rule (static).

### Navbar (`.site-nav`, `.nav-inner`, `.brand`, `.nav-menu`, `.nav-actions`)
- **Purpose:** primary wayfinding + global actions.
- **Structure:** sticky 76px translucent bar; brand start, menu center, actions end (see Header Redesign).
- **States:** default, `.scrolled` (elevation), active link (`aria-current`), drawer open (mobile).
- **Hover:** nav links show the gold `scaleX` underline and shift to green.

### Dropdown (`.nav-dropdown`, `.nav-dropdown-toggle`, `.nav-dropdown-menu`)
- **Purpose:** group secondary destinations.
- **Structure:** toggle button + absolutely positioned menu panel (desktop) / inline expanded list (mobile).
- **States:** closed, open (`.open` + `aria-expanded`), hover, focus-within.
- **Hover:** items get `--primary-050` background + green text; caret rotates 180° when open.

### Search box (`.search-bar`, `.search-input`)
- **Purpose:** filter lists / global search.
- **Structure:** pill input with a leading magnifier icon; optional trailing Search/Clear button; RTL-aware icon side.
- **States:** default, focus (`--primary` border + `--primary-050` ring), with-value (Clear visible).
- **Hover:** border to `--primary-300`.

### Pagination / "Load more"
- **Purpose:** traverse long lists (publications, theses, books).
- **Structure:** either numbered pill page links or a single centered outline "Load more" button; current page green-filled.
- **States:** default, current (active), disabled (first/last), hover.
- **Hover:** page pill gains `--primary-050`; active stays green.

### Modal (book detail, video lightbox)
- **Purpose:** focused content (PDF preview, video player) without leaving the page.
- **Structure:** scrim (`rgba(15,20,17,.7)` + `blur(4px)`) + centered panel (`--radius-lg`, `--shadow-lg`, max-width ~640–860px). Header/body/actions.
- **States:** open, closing; focus trapped inside while open.
- **Hover:** action buttons follow button rules. Close on scrim click, Close button, Escape.

### Tags / chips (`.chip`, `.skill-chip`)
- **Purpose:** institutions (credibility) and skills/competencies.
- **Structure:** pill; institution chip = `--surface` + 1px `--line` + gold dot + `--shadow-sm`; skill chip = flat `--primary-050` green-tint, no shadow.
- **States:** default, hover (institution only).
- **Hover (institution):** border to `--primary-300` + `translateY(-2px)`.

### Badges (`.badge-deg`, `.badge-cat`, relationship badges)
- **Purpose:** status/category labels (PhD / MA, Supervised / Examined / Ongoing, Featured).
- **Structure:** small pill, Inter 700, 0.78rem. Tones: green tint (PhD), gold tint (MA), solid green (Supervised), neutral bordered (Examined), solid gold (Ongoing/Featured).
- **States:** static (non-interactive).
- **Hover:** none.

---

# Razor Pages Implementation Notes

### Implement in shared layouts
- **Header, footer, skip-link, back-to-top, theme + language switch** live in `Pages/Public/Shared/_PublicLayout.cshtml`. Editing this one file updates every public page. Keep label text bound to `SharedResource` / Header management — never hardcode strings.
- The `<html lang dir data-lang data-theme>` attributes are set here per culture/theme — keep that logic; only adjust markup/classes.

### Move to global CSS
- **All visual rules belong in `wwwroot/css/public.css`** (the canonical theme). Add/extend the token block in `:root`, the dark-mode and `[data-lang="ar"]` scopes, and the component classes there. Do **not** introduce per-page `<style>` blocks or inline styles. `site.css` is shared chrome; `admin.css` is out of scope — do not touch it.
- Prefer **CSS logical properties** for any new spacing/border so RTL mirrors for free.

### Reusable partials
- Promote repeated markup to partials under `Pages/Public/Shared/`:
  - `_BookCard.cshtml`, `_BookDetail.cshtml` (exist — restyle, keep model contracts).
  - Add `_PublicationItem.cshtml`, `_VideoCard.cshtml`, `_EventCard.cshtml`, `_SectionHead.cshtml` (eyebrow + title + lead), `_PageHero.cshtml` (breadcrumb + title + lead), `_StatCard.cshtml`, `_Chip.cshtml`.
- Each partial takes its existing view model / localized fields; the redesign changes the partial's **markup + classes only**.

### Leave untouched (to avoid breaking functionality)
- `PageModel` (`*.cshtml.cs`), controllers, Entity Framework, `app.db`, migrations, seeding.
- Localization pipeline: `SharedResource`, `Localized.Pick`, `.resx` files, culture routing (`/{culture}/...`).
- Upload/file handling, slug logic (`SlugHelper`), SEO/meta/sitemap, view/download/play counters, contact honeypot + rate limiting.
- Admin area (`Areas/Admin`, `admin.css`, `admin.js`) entirely.
- Existing JS behaviors in `public.js` (theme persistence, scroll header, counters, nav drawer) — extend, don't rewrite; reuse the hooks (`.scrolled`, `data-target`/`data-suffix`, `.open`).

---

# Implementation Priority

Phased so each phase ships independently and is verifiable before the next.

### Phase 1 — Header
- **Effort:** Medium (1–2 days).
- **Risks:** Nav reduction (14→7) must not orphan routes; ensure every nested page stays reachable and labels remain admin-editable/localized. Drawer + dropdown keyboard behavior.
- **Validation:** All 7 primary items + nested links reachable in AR/EN/FR; active state correct; drawer works ≤ 1199px; language switch preserves the current page; theme persists; focus visible; ≥ 44px targets; no header overflow at any width.

### Phase 2 — Footer
- **Effort:** Low (0.5 day).
- **Risks:** Quick-links must mirror the new IA; social links stay admin-bound.
- **Validation:** Footer links resolve; social icons (Facebook, WhatsApp, Email) correct; copyright/localized text renders per culture; RTL mirrored.

### Phase 3 — Homepage
- **Effort:** High (2–3 days).
- **Risks:** Hero portrait fallback; stat counter + scroll reveals must respect reduced-motion; section order is admin-controlled — keep it dynamic.
- **Validation:** Hero renders with and without a photo; counters animate then settle; each preview shows correct top/featured/recent items + working "View all →"; CTA band links to Contact; AA contrast; AR RTL correct.

### Phase 4 — Publications
- **Effort:** Medium (1–2 days).
- **Risks:** Filter + search are progressive enhancements — list must work without JS; result count accuracy; Theses filter-table parity.
- **Validation:** Decade filter + text search narrow correctly; "X of Y" count accurate; PDF/DOI actions appear only when sources exist; rows stack on mobile; year placement correct in RTL.

### Phase 5 — Videos
- **Effort:** Medium (1–1.5 days).
- **Risks:** Defer YouTube iframe until modal open (performance); Play counter fires once; thumbnail fallback.
- **Validation:** Grid + featured render; modal opens/closes via click, Close, Escape; player loads only on open; "Watch on YouTube" link correct; Play event increments; reduced-motion respected.

### Phase 6 — Books / Reading
- **Effort:** Medium (1–2 days).
- **Risks:** `_BookDetail` PDF preview must launch gracefully with no PDF/cover; View/Download counters; tab vs sibling-page decision.
- **Validation:** Grid reflows 1→4 cols; gradient-spine fallback shows when no cover; detail modal stacks under 560px; Read/Download increment counters; search filters; AR RTL correct.

### Phase 7 — Events
- **Effort:** Medium (1 day).
- **Risks:** Upcoming/past split by date; gallery empty state; date-chip localization (month abbreviations per culture).
- **Validation:** Upcoming sorted soonest-first; past separated; date chips correct in all cultures; gallery thumbnails link to full image; empty state shows when no images; hover/elevation correct.

### Phase 8 — Contact
- **Effort:** Low–Medium (0.5–1 day).
- **Risks:** Preserve honeypot + rate limit + server validation; success state must not lose the message-to-inbox flow.
- **Validation:** Two-column → stacked responsive; fields validate (required, email format); focus ring + ≥ 44px targets; success confirmation renders in-card; info values come from data; social actions correct; AR RTL mirrored.

---

## Cross-phase Definition of Done
For **every** phase: verified in **AR (RTL) / EN / FR**, in **light and dark mode**, at **mobile / tablet / desktop** widths; keyboard-navigable with visible focus; AA contrast; reduced-motion honored; no console errors; no change to data, routing, or localization behavior.
