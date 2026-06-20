# 59 — UI Difference Report (Static → Dynamic)

**Date:** 2026-06-20
**Source of truth (design):** `ProfAly.Static` — `assets/css/style.css` (752 lines) + the provided screenshots (`UI UX/*.png`).
**Subject:** the dynamic CMS public site (`Pages/Public/*`, `wwwroot/css/public.css`).
**Method:** compared the static stylesheet (exact values) and screenshots against the dynamic implementation, component by component.

**Good news:** the design **tokens are already identical** (palette, fonts, radii, shadows, `--maxw`, `--ease` all match). The regressions are in **component styling** — the dynamic site ported the tokens but simplified several components. No architecture/schema/logic/route changes are needed to fix them; almost all are CSS, with three tiny markup-class swaps.

---

## 1. Page-by-Page Findings

### Header / Navigation (visible on every page — `research.png`)
| # | Finding | Static (truth) | Dynamic (now) | Severity |
|---|---|---|---|---|
| H1 | **Active nav item** | gold **underline** (`::after` bar, `scaleX`), text `--primary`, weight 600 | filled `--primary-050` **pill** background | **High** |
| H2 | Brand mark | 44px, green **gradient**, radius 12 | 38px, **solid**, radius 10 | Medium |
| H3 | Nav height | `--nav-h: 76px` | 72px | Low |
| H4 | Desktop menu breakpoint | xl **1200px** + Bootstrap offcanvas drawer | 992px + custom dropdown | Medium (functional) |

### Experience timeline (`experience.png`)
| E1 | **Timeline marker** | 18px **hollow** circle, white fill, **3px green ring**, light-green halo; line at `inset-inline-start:8px`, gradient accent→line | 12px **gold filled** dot at `-7px`; plain 2px line | **High** |
| E2 | Item spacing | `padding-inline-start: 2.4rem`, `padding-block-end: 1.8rem` | 1.75rem / 1.5rem | Medium |

### Memberships (`experience 2.png`)
| M1 | **Membership items** | bordered **rows** (course-item style) each with a **gold bullet dot**, inside panels | bare `<li>` text in a `chip-set` column | **High** |
| M2 | **Panel title** | **4px gold bar** (`::before`) + 1.3rem | no bar, 1.15rem | **High** |

### Publications / Research (`publications.png`)
| P1 | **Card accent** | **4px gold `border-inline-start`** bar on each card | full border, no edge accent | **High** |
| P2 | **Year** | `--primary` (**green**), 1.5rem, head font | `--accent-ink` (gold) | **High** |
| P3 | Filter toolbar | count + year `<select>` + search field | absent | Medium (search is out of scope) |

### Theses (`theses.png`)
| T1 | **Table header** | **dark-green `--primary` background, white text** | light `--surface-2` background | **High** |
| T2 | **Relationship cell** | colored **badges**: supervised (green fill), examined (outline), ongoing (gold) | plain text | **High** |
| T3 | **Filter tabs** | grouped in **one rounded `--surface-2` container** (pill of pills) | separate detached pills | **High** |
| T4 | Row striping | even rows `--surface-2`, hover `--primary-050` | none | Medium |
| T5 | Toolbar (count / year-range / degree / search) | present | absent | Medium (search out of scope) |

### Activities accordion (`research 2.png`)
| A1 | **Count badge** | **dark-green filled circle** (2rem), on the start side | small **gold** badge before the name | **High** |
| A2 | Header open state | `--primary-050` bg, `--primary-700` text | similar (ok) | Low |
| A3 | Body list bullets | gold `::marker` dots, indented | plain `<ul>` | Medium |

### Books (`books.png`)
| B1 | Book cover | gradient + **spine bar** + radial gold glow + 3.2rem initial | gradient + initial (simpler) | Low |
| B2 | Search field | present ("apply search") | absent | Medium (search out of scope) |

### Teaching
| TE1 | **Course items** | dedicated **course-item** bordered rows with a gold dot (`::before`) + hover slide | reuses **`.pub-item`** (publication component) | **High** |

### About / Qualifications
| Q1 | Qualifications | **qual-item** cards (`qual-year` + `qual-grade` pill) | reuses **`.timeline`** | Medium |

### Statistics (home)
| S1 | Stat card | **top gradient bar** (primary→accent), min-height 118 | plain card | Medium |

### Contact
| C1 | Layout | `contact-grid` 2-col + photo | form + info panel | Low |

### Footer
| F1 | Footer headings | uppercase, letter-spaced, `--accent-600` | similar (ok) | Low |

### Responsive / RTL
| R1 | RTL logical props | both use `inset-inline`/`margin-inline`; ✅ already correct | — | OK |
| R2 | Nav breakpoint | xl(1200) vs 992 — both work, slightly different collapse point | — | Medium |

---

## 2. Severity Summary

- **Critical:** none — the dynamic site is fully functional; differences are visual.
- **High (implement now):** H1 (nav underline), E1 (timeline hollow-green dots), M1 (membership rows), M2 (panel-title bar), P1+P2 (publication gold edge + green year), T1+T2+T3 (theses green header + relationship badges + grouped tabs), A1 (accordion green count), TE1 (teaching course rows). Plus low-risk ride-alongs already in the same CSS edits: T4 (row striping), H2 (brand mark), H3 (nav height), S1 (stat bar), A3 (accordion bullets), E2 (timeline spacing).
- **Medium / Low (documented, deferred):** P3/T5/B2 search-and-filter toolbars (**Search is explicitly out of scope** from earlier stages — not implemented), Q1 (qualifications cards), B1 (book cover spine/glow), C1 (contact 2-col), H4/R2 (nav breakpoint), page-hero radial gradient.

---

## 3. Implementation Plan (High only — exact)

All changes are confined to the **public layer** (`public.css` + 3 Razor markup-class swaps). No architecture/schema/logic/URL/CMS changes; localization, routing, DB, and admin untouched.

**`wwwroot/css/public.css` (port static component styles):**
1. Nav: `.nav-menu a` → underline `::after` (accent, scaleX) for `:hover`/`.active`; remove pill background; active `color:--primary; font-weight:600`. Brand mark 44px gradient radius 12; `--nav-h` 76px.
2. Timeline: add `.timeline::before` line (gradient, `inset-inline-start:8px`); restyle `.tl-item` (`padding-inline-start:2.4rem`) and `.tl-item::before` → 18px hollow circle, white fill, 3px `--primary` ring, `0 0 0 4px --primary-050` halo at `inset-inline-start:0`.
3. `.panel-title` → flex + `::before` 4px gold bar, size 1.3rem.
4. `.pub-item` → `border-inline-start:4px solid --accent`; `.pub-year` → `--primary`.
5. `.filter-tabs` → `--surface-2` container, `.35rem` padding, pill; `.filter-tab.active` green fill (already), hover primary.
6. `.theses-table thead th` → `--primary` bg, white; `tbody tr:nth-child(even)` stripe, hover `--primary-050`; add `.badge-supervised/examined/ongoing`, `.th-year` green.
7. `.acc-count` → `--primary` filled circle, 2rem; accordion body `ul` → gold markers.
8. `.stat-card::before` → top gradient bar.
9. `.member-list`/`.course-item` → bordered rows with gold dot `::before` (shared by memberships + teaching).

**Markup class swaps (no logic change):**
- `Pages/Public/Experience.cshtml` — memberships: `chip-set` `<li>` → `<ul class="course-list"><li class="course-item">`.
- `Pages/Public/Teaching.cshtml` — courses: `.pub-item` → `.course-item` (in `.course-list`).
- `Pages/Public/Theses.cshtml` — relationship cell: wrap `@CatLabel` in `<span class="badge-cat badge-@Cat(...)">`.

Verification: build, tests, and render every page at 1366/768/375 px in AR(RTL)/EN(LTR), confirming no functional/localization/routing regressions. Results in **60_UI_Alignment_Implementation_Report.md**.
