# 35 — Content Migration Plan

**Goal:** import the **complete existing academic content** from the legacy static site into the CMS database automatically — not hand-entered into empty CRUD forms.
**Primary source:** `ProfAly.Static/assets/js/data.js` (the single source of truth for all CV content; bilingual AR/EN).
**Status:** plan + import infrastructure (this stage); the actual import runs on demand (config flag).

> Counts below are exact, computed by evaluating `data.js` with Node and tallying each dataset.

---

## 1. Source Datasets (`data.js` → `SITE_DATA`)

| Dataset | Shape | Count |
|---|---|---|
| `profile` | object (name, shortName, title, positioning, phone, email, location, born, nationality, marital, languages) — each text field `{ar,en}` | 1 |
| `bio` | `{ ar: string[3], en: string[3] }` (paragraphs) | 3 ×2 |
| `stats` | `[{ value, suffix, label{ar,en} }]` | 5 |
| `credibility` | `[{ ar, en }]` | 5 |
| `qualifications` | `[{ year, grade{ar,en}, degree{ar,en}, institution{ar,en} }]` | 4 |
| `career` | `[{ period{ar,en}, role{ar,en}, org{ar,en}, desc{ar,en} }]` | 8 |
| `skills` | `[{ ar, en }]` | 5 |
| `books` | `[{ year, featured, publisher{ar,en}, title{ar,en}, role{ar,en} }]` | 14 (3 featured) |
| `publications` | `[{ year, title{ar,en}, venue{ar,en} }]` | 9 |
| `theses` | `[{ n, category, degree, year, researcher{ar,en}, title{ar,en} }]` | 57 |
| `activities` | `[{ group{ar,en}, items: [{ar,en}] }]` | 5 groups / 26 items |
| `teaching` | `{ undergraduate: [{ar,en}], graduate: [{ar,en}] }` | 16 (8 + 8) |
| `memberships` | `{ societies: [{ar,en}], boards: [{ar,en}] }` | 10 (3 + 7) |

Theses breakdown: **supervised 22 · examined 33 · ongoing 2**; **MA 25 · PhD 32**.
Legacy images (`hero-portrait.jpeg`, `about-lecture.jpeg`, `contact-headshot.jpeg`) live as files, **not** in `data.js`.

---

## 2. Target Entities & Field Mappings

| Source | Target entity | Field mapping |
|---|---|---|
| `profile` | `Profile` (+`ProfileTranslation`) | base: `Email`←email, `Phone`←phone, `DateOfBirth`←parsed(born). translation(ar/en): `FullName`←name, `ShortName`←shortName, `Title`←title, `Positioning`←positioning, `Location`←location, `Nationality`←nationality, `MaritalStatus`←marital, `Languages`←languages |
| `bio` | `ProfileTranslation.FullBio` | join paragraphs with blank line, per culture |
| `stats` | `StatItem` (+T) | `Value`←value, `Suffix`←suffix; `Label`←label |
| `credibility` | `Credibility` (+T) | `Name`←text; `LogoMediaId`=null |
| `qualifications` | `Qualification` (+T) | `Year`←year; `Degree`←degree, `Institution`←institution, `Grade`←grade |
| `career` | `ExperienceEntry` (+T) | `PeriodLabel`←period; `Role`←role, `Organization`←org, `Description`←desc; `Start/EndDateUtc`=null (display via PeriodLabel) |
| `skills` | `Skill` (+T) | `Name`←text |
| `books` | `ContentItem`=`Book` (+T) | `PublicationYear`←year, `IsFeatured`←featured, `IsPublished`=true; `Title`←title, `Publisher`←publisher, `AuthorshipRole`←role, `Slug`←generated |
| `publications` | `ContentItem`=`Publication` (+T) | `PublicationYear`←year, `IsPublished`=true; `Title`←title, `Journal`←venue, `Slug`←generated; `Doi`/`Authors`=null |
| `theses` | `ContentItem`=`Thesis` (+T) | `PublicationYear`←year, `DegreeLevel`←(MA→Master, PhD→PhD), `RelationshipType`←(supervised/examined/ongoing), `IsPublished`=true; `Title`←title, `ResearcherName`←researcher, `Slug`←generated |
| `activities` | `ActivityGroup` (+T) → `Activity` (+T) | group: `Name`←group; item: `Text`←text, FK→group |
| `teaching` | `Course` (+T) | `Level`←(undergraduate→Undergraduate, graduate→Graduate); `CourseName`←text |
| `memberships` | `Membership` (+T) | `Kind`←(societies→Society, boards→Board); `Name`←text |

`SortOrder` for every list/collection = source array index (preserves original ordering).

---

## 3. Missing Fields (source has none → handled)

| Field | Handling |
|---|---|
| **French (fr) translations** | Left empty for all entities; public render falls back to default culture (doc 10 §5). |
| **Slugs** (content) | Generated from the title via `SlugHelper`, de-duplicated per culture. |
| **Cover images / PDFs** | None in source → `CoverImageId`/`PdfFileId`=null; admin uploads later (CSS book-cover fallback on the public site). |
| **DOI / Authors** (publications) | Not in source → null. |
| **ResearchPaper** content type | Source has **no** dataset distinct from `publications`; legacy `research.html` = Activities. So import creates **0 ResearchPaper** rows (the 9 papers map to `Publication`). |
| **Categories** | None in source → no `Category`/links created on import. |
| **Profile photo / CV** | Not in `data.js`; the 3 image files may be imported separately (optional, out of this JSON import). |
| **Experience dates** | Source `period` is free text → kept as `PeriodLabel`; structured `Start/EndDateUtc` left null. |

---

## 4. Data Cleanup Requirements

1. **Date of birth:** `born` is localised text (`"٢٤ فبراير ١٩٦٦"` / `"24 February 1966"`). Parse the **English** form → `1966-02-24` (`DateOfBirth`); render per culture afterward.
2. **Digits:** Arabic-Indic digits appear in some AR strings (e.g., periods). Per house style (doc 10 §8b) the app renders Western digits; `PeriodLabel`/text keep the author's wording as-is (display-only).
3. **Trim** all whitespace; treat empty EN as "no EN translation" (skip that row).
4. **Enum mapping:** degree `MA`→`Master`, `PhD`→`PhD`; category→`RelationshipType`; teaching key→`CourseLevel`; membership key→`MembershipKind`.
5. **Slug generation + per-culture uniqueness** (append `-2`, `-3`…).
6. **Publisher long-form text** (e.g., "within «Modern Trends…»") preserved verbatim in `Publisher`.

---

## 5. Seed / Import Strategy

- The JSON is generated from `data.js` (Node) and stored as **`Infrastructure/Persistence/Seeding/Data/static-content.json`**, embedded as a resource in the Infrastructure assembly.
- A **`StaticContentImporter : IDataSeeder`** (Order 100, after Roles/SuperAdmin/SiteSettings) reads the embedded JSON and maps it to entities.
- **Idempotent:** each dataset is imported only if its target table is empty (so partial re-runs fill gaps, and re-running never duplicates).
- **Gated by config:** runs only when **`Seed:ImportStaticContent = true`** (default false) — so normal dev runs don't import unexpectedly; flip the flag (env/user-secrets/appsettings) to import "immediately afterward".
- Imported items default to **Published** (the static site showed them publicly).

---

## 6. Migration Order (FK-safe)

```
1. Profile (+ translations, FullBio)        — singleton, no deps
2. StatItem, Credibility, Qualification,
   Skill, Membership, ExperienceEntry,
   Course                                    — independent lists
3. ActivityGroup  →  Activity                — Activity FK → ActivityGroup
4. ContentItem (Book, Publication, Thesis)
   + ContentItemTranslation                  — no deps (no categories/media)
```
(`SiteSettings`, roles, and the admin are already seeded by Stage 3.)

---

## 7. Estimated Record Counts

| Entity | Rows | Translation rows (ar+en) |
|---|---|---|
| Profile | 1 | 2 |
| StatItem | 5 | 10 |
| Credibility | 5 | 10 |
| Qualification | 4 | 8 |
| Skill | 5 | 10 |
| Membership | 10 | 20 |
| ExperienceEntry | 8 | 16 |
| Course | 16 | 32 |
| ActivityGroup | 5 | 10 |
| Activity | 26 | 52 |
| ContentItem — Book | 14 | 28 |
| ContentItem — Publication | 9 | 18 |
| ContentItem — Thesis | 57 | 114 |
| **Totals** | **165 base rows** | **~330 translation rows** |

MediaFiles created by the JSON import: **0** (no media in `data.js`). French translation rows: **0** (fallback). Categories/links: **0**.

---

## 8. Verification (after import)

- Row counts match the table above per entity.
- `Book` featured count = 3; thesis relationship split 22/33/2; degree split 25/32.
- Every imported `ContentItemTranslation` has a non-empty, unique (per culture) slug.
- Default-culture (Arabic) text present on every row; English present where the source had it.
- Re-running the importer makes **no** changes (idempotent).
