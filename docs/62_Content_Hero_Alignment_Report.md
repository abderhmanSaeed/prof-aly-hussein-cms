# 62 — Content Hero Alignment Report

**Date:** 2026-06-20
**Issue:** During the Static → Dynamic migration, interior page heroes lost the **introductory description** (`section-lead`) and used the bland nav label for both the eyebrow and the title — producing a "Breadcrumb / Title / duplicate Title" with no description.
**Source of truth:** `ProfAly.Static` — `assets/js/i18n.js` (the `*.title` / `*.lead` keys) + `*.html` hero markup.
**Fix:** restore the full hero structure (breadcrumb → eyebrow → title → **lead**) on every interior page, with the proper rich title and the original descriptions, stored as **localized resources** (AR/EN/FR).
**Constraints honored:** routing, DB integration, localization architecture, RTL/LTR all unchanged. No commit/tag/push.

---

## 1. Root Cause

The static hero markup (every interior page) is:

```html
<nav class="crumbs"> Home / <span aria-current>nav.X</span> </nav>
<span class="eyebrow" data-i18n="X.title"></span>
<h1 data-i18n="X.title"></h1>
<p class="section-lead" data-i18n="X.lead"></p>
```

i.e. the breadcrumb uses the **nav label** (`nav.X`), while the eyebrow + h1 use a **richer page title** (`X.title`), followed by the **lead** (`X.lead`). The dynamic migration collapsed all three to the single nav-label key (`X_Title`) and **omitted the lead entirely**. So `Home / Publications` → `Publications` → `Publications` with no description.

## 2. Fix Applied

- Added per-page **`Hero_{Page}_Title`** and **`Hero_{Page}_Lead`** keys to `SharedResource.{resx,ar,fr}` (the existing localization store — no architecture change).
- Pointed each hero's **eyebrow + h1 at `Hero_{Page}_Title`** and added **`<p class="section-lead">@L["Hero_{Page}_Lead"]</p>`**.
- Kept the **breadcrumb** on the existing short nav label (matches the static `nav.X` vs `X.title` separation).
- `ViewData["Title"]` (browser tab) now uses the rich hero title; `ViewData["Description"]` (meta) now uses the lead — improving SEO too.
- Values taken **exactly from the static `i18n.js`** for AR/EN; FR from the descriptions you provided (and translated for the two pages without a static FR string).

The eyebrow and h1 remain the **same text** on purpose — that is the static design (a small gold label above the large heading). The regression was the *missing lead* and the *bland nav-label title*, both now corrected.

## 3. Pages Reviewed & Corrected

| Page | Title (eyebrow + h1) restored | Lead (description) restored |
|---|---|---|
| **Experience** | Academic Experience / الخبرة الأكاديمية / Expérience académique | ✅ |
| **Activities / Projects** | Research & Professional Activities / المشروعات والأنشطة البحثية / Projets et activités de recherche | ✅ |
| **Publications** | Publications / الأبحاث المنشورة / Publications | ✅ |
| **Books** | Academic Books / المؤلَّفات الأكاديمية / Ouvrages académiques | ✅ |
| **Theses** | Supervised Theses / الرسائل العلمية / Thèses encadrées | ✅ |
| **Teaching** | Courses Taught / المقررات التدريسية / Cours enseignés | ✅ |
| **Research** | Research Papers / الأوراق البحثية / Articles de recherche | ✅ (CMS-only page; no static equivalent — sensible title/lead added) |
| **Contact** | Contact / تواصل معي / Contact | ✅ |
| **About** | *(unchanged — already complete & DB-driven)* | already present |

**About note:** the About hero already renders breadcrumb → eyebrow → **profile name as h1** → **academic title as lead** (all DB-driven). It was never missing a description, and the DB-driven hero is intentionally richer than repeating "About", so it was **left as-is** (consistent with "use database-driven content where appropriate").

**Out of scope:** the Home hero (DB-driven name/title/positioning) and the Videos page (Stage 9; no static counterpart) were not part of this task and are unchanged.

## 4. Missing Descriptions Restored (the leads)

| Page | EN (from static) | AR (from static) | FR |
|---|---|---|---|
| Experience | A career spanning school teaching to a full university professorship. | تدرّجٌ وظيفيٌّ ممتدٌّ من التدريس المدرسي إلى الأستاذية الجامعية. | Parcours académique s'étendant de l'enseignement scolaire au professorat universitaire. |
| Activities | International training, regional projects, and consulting in quality assurance and education development. | تدريبٌ دولي، ومشروعاتٌ إقليمية، واستشاراتٌ في ضمان الجودة وتطوير التعليم. | Formations internationales, projets régionaux et conseil en assurance qualité et développement de l'éducation. |
| Publications | Nine peer-reviewed studies in respected scholarly journals. | تسع دراسات محكَّمة في دوريات علمية مرموقة. | Neuf études évaluées par des pairs publiées dans des revues scientifiques reconnues. |
| Books | Fourteen books and texts on curriculum, teaching methods, and environmental education. | أربعة عشر كتابًا ومؤلَّفًا في المناهج وطرق التدريس والتربية البيئية. | Quatorze ouvrages académiques en curriculum, pédagogie et éducation environnementale. |
| Theses | Supervision and examination of more than fifty-seven master's and doctoral theses. | الإشراف والمناقشة لأكثر من سبعٍ وخمسين رسالة ماجستير ودكتوراه. | Encadrement et évaluation de plus de cinquante-sept mémoires de master et thèses de doctorat. |
| Teaching | Undergraduate and graduate courses in curriculum and teaching methods. | مقرراتٌ في المرحلة الجامعية والدراسات العليا في المناهج وطرق التدريس. | Cours de premier cycle et de troisième cycle en curriculum et méthodes d'enseignement. |
| Research | Selected research papers and scholarly contributions. | أوراقٌ بحثية ومساهماتٌ علمية مختارة. | Articles de recherche et contributions scientifiques sélectionnés. |
| Contact | I welcome your messages regarding academic collaboration and educational consulting. | يسعدني تلقّي رسائلكم بشأن التعاون العلمي والاستشارات التربوية. | Je serai ravi de recevoir vos messages concernant la collaboration scientifique et le conseil pédagogique. |

> Where the static had AR/EN strings, those were used **verbatim** (source of truth). FR uses the descriptions you supplied for the six listed pages; Research/Contact FR were translated to match the project's French style.

## 5. Localization Additions

- **16 new resource keys** (`Hero_{Experience|Activities|Publications|Books|Theses|Teaching|Research|Contact}_{Title|Lead}`) added to each of `SharedResource.resx` (EN/neutral), `SharedResource.ar.resx`, `SharedResource.fr.resx`.
- Stored as **localized resources** (the project's `IStringLocalizer<SharedResource>` architecture) — not hardcoded in views, not a DB/schema change.
- All three cultures populated, so FR now shows real French (no fallback needed); AR remains RTL.

## 6. Files Changed (11)

**Resources (3):** `SharedResource.resx`, `SharedResource.ar.resx`, `SharedResource.fr.resx` — +16 keys each.
**Public pages (8):** `Experience.cshtml`, `Activities.cshtml`, `Publications.cshtml`, `Books.cshtml`, `Theses.cshtml`, `Teaching.cshtml`, `Research.cshtml`, `Contact.cshtml` — eyebrow/h1 → `Hero_*_Title`, added `section-lead` = `Hero_*_Lead`, updated `ViewData` Title/Description.

No `.cs`, domain, persistence, routing, or admin files changed. Hero spacing/typography/colors are unchanged (existing `.page-hero`, `.eyebrow`, `h1`, `.section-lead` styles already match the static — see report 59/60).

## 7. Verification

```
dotnet build (Web) → Build succeeded, 0 errors / 0 warnings
dotnet test        → 31/31 passed
Heroes rendered (temp seeded DB) for all 8 pages × cultures:
  eyebrow == rich title, plus section-lead present, e.g.
  /en/books      → "Academic Books" + "Fourteen books and texts on curriculum…"
  /ar/publications → "الأبحاث المنشورة" + "تسع دراسات محكَّمة في دوريات علمية مرموقة."
  /fr/publications → "Publications" + "Neuf études évaluées par des pairs…"
Real App_Data untouched; temp DB/build deleted; no stray processes.
```

**Preserved:** routing (`/{culture}/…` unchanged), DB integration (page models untouched), localization architecture (resx + `IStringLocalizer`), RTL/LTR, admin. Original hero spacing, typography, colors, and hierarchy retained.

**⏸ Hero alignment complete (public layer only). No commit/tag/push — awaiting your review.**
