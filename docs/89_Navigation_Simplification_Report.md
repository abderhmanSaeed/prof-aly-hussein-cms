# 89 — Navigation Simplification Report

> **Status:** Implemented and verified locally. **No commit, tag, or push** — awaiting review.
> Scope: **navigation/UX only.** No entities, routes, schema, or content logic changed.
> This report supersedes the navigation decision in
> [84_Activities_And_Video_Organization_Report.md](84_Activities_And_Video_Organization_Report.md).

---

## 1. What was requested

- **فعاليات متنوعة (Diverse Activities)** must become a **normal top-level navigation item** —
  **no dropdown, no submenu**.
- Clicking **فعاليات متنوعة** must navigate directly to **`/{culture}/events`**
  (e.g. `/ar/events`, `/en/events`, `/fr/events`).
- **Remove مقاطع فيديو (Video Clips)** from the menu **completely** — video content already lives
  under **المصادر الرقمية (Digital Resources) → فيديوهات تعليمية (Educational Videos)** and must
  not appear in two places.

This reverses the earlier doc-84 decision that exposed the Video module a second time as
"Video Clips" under a "Diverse Activities" dropdown.

---

## 2. Final navigation structure

```
المصادر الرقمية (Digital Resources) ▾
├─ فيديوهات تعليمية (Educational Videos)  → /{c}/videos              (existing Video module)
├─ مواد إثرائية   (Enrichment Materials)  → /{c}/enrichment
└─ كتب جديرة بالقراءة (Books Worth Reading) → /{c}/recommended-books

فعاليات متنوعة (Diverse Activities)        → /{c}/events             (plain top-level link, no dropdown)
```

Video content now has exactly **one** doorway in the navigation: *Educational Videos* under
*Digital Resources*. The `/{c}/videos` listing and detail pages are unchanged.

---

## 3. Changes made

### 3.1 `src/ProfAly.CMS.Web/Pages/Public/Shared/_PublicLayout.cshtml`

The `Nav_DiverseActivities` entry was changed from a dropdown (own `Page = null`, two children)
into a plain top-level link to the Events page:

**Before**
```csharp
("Nav_DiverseActivities", null, new[]
{
    ("Nav_Events", "/Public/Events"),
    ("Nav_VideoClips", "/Public/Videos"),
}),
```

**After**
```csharp
("Nav_DiverseActivities", "/Public/Events", null),
```

Because the item now has a non-null `Page` and `Children = null`, the existing layout loop renders
it through the **plain-link branch** (`<li><a …>`), not the dropdown branch — so no dropdown markup,
no toggle button, and no submenu `<ul>` are emitted for it. The explanatory comment block above the
`nav` array was updated to reflect the single-doorway-for-video rule.

The label `Nav_DiverseActivities` ("فعاليات متنوعة" / "Diverse Activities" / "Activités diverses")
is preserved, so the clickable text remains **فعاليات متنوعة** as required.

### 3.2 Resource files — removed the now-unused `Nav_VideoClips` key

Removed `Nav_VideoClips` from all three resource files (the "مقاطع فيديو" string is no longer
referenced anywhere):

| File | Removed value |
|------|---------------|
| `Resources/SharedResource.ar.resx` | `مقاطع فيديو` |
| `Resources/SharedResource.resx`    | `Video Clips` |
| `Resources/SharedResource.fr.resx` | `Clips vidéo` |

`Nav_Events` ("الفعاليات" / "Events" / "Événements") was left in place — it is a harmless, valid
label and its removal was not requested.

### 3.3 Route mapping

`/Public/Events` is the Razor Page that declares `@page "/{culture:regex(^(ar|en|fr)$)}/events"`,
so `asp-page="/Public/Events" asp-route-culture="@culture"` produces exactly `/ar/events`,
`/en/events`, and `/fr/events`. No route, page, or controller was added or changed.

---

## 4. Verification

| Check | Result |
|-------|--------|
| **فعاليات متنوعة renders as a plain link (no caret/toggle)** | ✅ Item has non-null `Page`, so the layout's plain-link branch is used; no `.nav-dropdown` / `.nav-dropdown-toggle` markup emitted. |
| **Click target = `/{culture}/events`** | ✅ `asp-page="/Public/Events"` + culture route → `/ar/events`, `/en/events`, `/fr/events`. |
| **مقاطع فيديو removed from menu** | ✅ Child entry deleted from nav array; `Nav_VideoClips` resource keys removed from all 3 locales. |
| **No empty dropdown remains** | ✅ Only one dropdown remains (Digital Resources) and it still has its three children. The Diverse Activities item no longer produces a `<ul class="nav-dropdown-menu">`. |
| **No orphan route** | ✅ No routes were added or removed. `/{c}/videos` still serves Educational Videos; `/{c}/events` still serves Events. |
| **Existing Events page still works** | ✅ `Events.cshtml` / `Events.cshtml.cs` and the `/{culture}/events` route are untouched. |
| **Educational Videos remain under Digital Resources only** | ✅ The only nav link to `/Public/Videos` is now `Nav_EducationalVideos` under Digital Resources. |
| **Desktop** | ✅ Plain `<li><a>` link with active-state styling (`active` when `currentPage == "/Public/Events"`). |
| **Mobile / burger** | ✅ `wwwroot/js/public.js` is generic — it only acts on `.nav-dropdown` / `.nav-dropdown-toggle` selectors. The plain link has neither class, so it behaves as an ordinary tappable link inside the mobile menu; no JS change needed. |
| **RTL (ar)** | ✅ Inherits the existing RTL layout (`dir="rtl"`, Bootstrap RTL stylesheet); a plain link has no direction-specific dropdown chrome to misalign. |
| **LTR (en/fr)** | ✅ Same plain-link rendering; labels resolve to "Diverse Activities" / "Activités diverses". |
| **Footer quick-links** | ✅ The footer flattens the nav array; Diverse Activities now contributes a single link (فعاليات متنوعة → `/Public/Events`) instead of two (Events + Video Clips). No dangling Video Clips link. |
| **No dangling references** | ✅ Grep over `*.cs` / `*.cshtml` for `Nav_VideoClips` and `Nav_Events` returns no usages; no test source references the nav structure. |

### Build note

A local `dotnet build` of the Web project reached the output-copy step and failed **only** with
file-lock errors (`MSB3021` / `MSB3026` / `MSB3027`) because the application was running in Visual
Studio at the time, holding the output DLLs. These are **not** compilation errors — C# compilation,
Razor, and resource (`.resx`) generation all succeeded. Re-running the build after stopping the app
will copy outputs cleanly.

---

## 5. Files touched

```
src/ProfAly.CMS.Web/Pages/Public/Shared/_PublicLayout.cshtml   (nav array + comment)
src/ProfAly.CMS.Web/Resources/SharedResource.ar.resx           (removed Nav_VideoClips)
src/ProfAly.CMS.Web/Resources/SharedResource.resx              (removed Nav_VideoClips)
src/ProfAly.CMS.Web/Resources/SharedResource.fr.resx           (removed Nav_VideoClips)
docs/89_Navigation_Simplification_Report.md                    (this report)
```

No commit. No tag. No push. Awaiting review.
