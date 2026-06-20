# 64 — Breadcrumb Active-Item Alignment Report

**Date:** 2026-06-20
**Task:** Fix the breadcrumb **active (current-page) item color & style** on all public pages to match the original static design.
**Source of truth:** `ProfAly.Static/assets/css/style.css` → `.crumbs span[aria-current] { color: var(--primary); font-weight: 600; }`.
**Constraints honored:** RTL/LTR preserved, routing/DB/localization unchanged, all cultures. No commit/tag/push.

---

## 1. Problem

The dynamic breadcrumb rendered the **current-page item in muted gray** (it was bare text inheriting `.crumbs` color) instead of the static **green + bold**. Example: `الرئيسية / المقررات التدريسية` — the active "المقررات التدريسية" was gray (incorrect) rather than green (correct).

Root cause: the markup had no element to target for the active item (no `aria-current`), and the CSS had **no active-item rule**.

## 2. Fix

**CSS (`wwwroot/css/public.css`)** — aligned `.crumbs` with the static and added the active-item rule:

```css
.crumbs { font-size: .85rem; color: var(--muted); margin-block-end: 1.1rem; display: flex; gap: .5rem; flex-wrap: wrap; align-items: center; }
.crumbs a { color: var(--muted); }
.crumbs a:hover { color: var(--primary); }
.crumbs [aria-current] { color: var(--primary); font-weight: 600; }   /* active item: green + bold */
```

**Markup** — wrapped the current-page label in `<span aria-current="page">…</span>` and the separator in `<span aria-hidden="true">/</span>` (matches the static markup and improves accessibility), e.g.:

```html
<nav class="crumbs">
  <a asp-page="/Public/Home" …>@L["Nav_Home"]</a>
  <span aria-hidden="true">/</span>
  <span aria-current="page">@L["Teaching_Title"]</span>
</nav>
```

## 3. Color Values

| Item | Color | Token |
|---|---|---|
| **Active** (current page) | **`#0B5D3B`** + bold (600) | `var(--primary)` |
| Inactive (Home, parent links) | gray | `var(--muted)` (`#5B6472`) |
| Hover (links) | `#0B5D3B` | `var(--primary)` |

> The brief listed inactive ≈ `#687280`; I used the design-system token `var(--muted)` to **match the static site exactly** (its `.crumbs a` uses `--muted`) and stay token-consistent. The active green `#0B5D3B` is the key fix.

## 4. Pages Fixed (all public pages — 11)

About, Activities, Books, Contact, Experience, Publications, Research, Teaching, Theses, Videos (single-level) — current item wrapped in `aria-current`.
**VideoDetail** (multi-level `Home / Videos / {title}`) — parent links stay gray, the final title is `aria-current` (green).

## 5. Before / After

| | Before | After |
|---|---|---|
| Active crumb color | muted gray (`--muted`) | **green `#0B5D3B`** |
| Active crumb weight | normal | **bold (600)** |
| Markup | bare text node | `<span aria-current="page">` (a11y) |
| Separator | raw `/` | `<span aria-hidden="true">/</span>` |
| Inactive items | gray | gray (unchanged) |

## 6. Files Changed (12)

- `wwwroot/css/public.css` — `.crumbs` block + active-item rule.
- 11 public pages: `About`, `Activities`, `Books`, `Contact`, `Experience`, `Publications`, `Research`, `Teaching`, `Theses`, `Videos`, `VideoDetail` (`.cshtml`).

No `.cs`, schema, routing, localization-architecture, or admin changes.

## 7. Verification

```
dotnet build (Web) → Build succeeded, 0 errors / 0 warnings
Served HTML: every page's .crumbs contains exactly one <span aria-current="page"> (checked /ar, /en, /fr)
CSS served: .crumbs [aria-current] { color: var(--primary); font-weight: 600; } present
RTL (ar) + LTR (en/fr) both render the active item green + bold
Rendered against a throwaway temp DB; real App_Data untouched; temp deleted.
```

**⏸ Breadcrumb alignment complete (public layer only). No commit/tag/push — awaiting review.**
