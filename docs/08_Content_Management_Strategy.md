# 08 — Content Management Strategy

All academic content shares one model (`ContentItem` + `ContentType` discriminator) with translation rows for text. This unifies the editing experience and the storage model while still letting each type expose its own fields. Below is how each type behaves.

---

## 1. Shared Lifecycle (applies to every content type)

1. **Create as draft** → add the default-culture translation (Title, Slug, Summary).
2. **Translate** → fill AR/EN/FR tabs (system flags missing languages).
3. **Attach media** → cover image, PDF, or YouTube ID as the type allows.
4. **Categorize** → assign one or more topical `Category` values.
5. **Order** → set `SortOrder` (or drag in the list).
6. **Publish** → becomes visible on the public site (requires default-culture translation).
7. **Track** → `ViewCount` / `DownloadCount` / play events accrue automatically.

Common fields for all: Title, Slug, Summary, Body, MetaTitle/Description/Keywords (per culture); CoverImage, Categories, IsPublished, SortOrder (shared).

---

## 2. Books
- **Fields:** cover image (recommended), PDF (optional), summary, body, categories, publication year.
- **Public behavior:** shown as covers; clicking opens a **popup** with cover, summary, inline PDF preview, and **Read/Download** buttons.
- **Metrics:** open → View; download → Download.
- **Notes:** if a book has no PDF, the popup omits preview/download and shows summary only.

## 3. Publications (المؤلفات)
- **Fields:** journal/venue, authors, DOI, publication year, PDF (optional), `ExternalUrl` (DOI/landing).
- **Public behavior:** list with citation-style metadata; detail page with abstract and link/download.
- **Notes:** DOI and ExternalUrl render as outbound links; PDF for open-access copies.

## 4. Research (الأبحاث)
- **Fields:** same academic metadata as Publications (journal, authors, DOI, year), abstract in Body, PDF.
- **Public behavior:** list + detail; download/track.
- **Distinction from Publications:** purely a `ContentType` filter — the professor decides what is "Research" vs "Publication"; the system treats them identically apart from the section they appear in.

## 5. Videos
- **Fields:** YouTube video ID (required), title, description, optional custom thumbnail, categories, sort order.
- **Public behavior:** grid of thumbnails; click → embedded `youtube-nocookie` player (modal or detail). **No video files are stored or streamed.**
- **Metrics:** play event recorded on user interaction.
- **Validation:** YouTube ID is parsed from a pasted URL or entered directly; format-validated.

## 6. Resources (المصادر)
- **Fields:** either a downloadable file (PDF) **or** an external link (`ExternalUrl`), summary, `ResourceType` label, categories.
- **Public behavior:** list/cards; file resources open the popup (preview/download); link resources open externally.

## 7. Enrichment Information (المعلومات الإثرائية)
- **Fields:** like Resources — articles/material that may be file-backed or link-backed, with richer `Body` for in-page reading.
- **Public behavior:** readable list/detail; supports the "enrichment content" tab and homepage preview.

## 8. Theses (الرسائل)
- **Fields:** degree level (Master/PhD), supervisor/role, year, abstract (Body), PDF (optional).
- **Public behavior:** list with degree/year metadata; detail with abstract and optional download.

## 9. Projects (المشروعات)
- **Fields:** status (Ongoing/Completed), role, dates (`EventDateUtc`), `ExternalUrl`, cover image, body.
- **Public behavior:** list/cards; detail page; outbound project link if present.

---

## 10. Categories (cross-cutting taxonomy)
- **Purpose:** topical grouping *across* content types (e.g. "Linguistics", "History"), distinct from `ContentType`.
- **Model:** `Category` + trilingual `CategoryTranslation`; many-to-many with content via `ContentItemCategory`.
- **Public behavior:** category pages (`/{c}/category/{slug}`) list all published items of any type in that topic; category filters appear on list pages.
- **Admin:** create/rename/reorder; assign during content editing.
- **Guidance:** keep the category set small and curated; it is a navigation aid, not a tagging free-for-all.

## 11. Page Sections, Experience, Teaching
These are managed content but not part of the `ContentItem` collection:
- **PageSection** — editable blocks for Home/About/Contact (hero text, intros).
- **ExperienceEntry** — chronological roles/positions.
- **Course** — teaching record.
All are trilingual via their translation tables and ordered by `SortOrder`.

---

## 12. Editorial Principles
- **Draft-first, publish-deliberately.** Nothing appears publicly until published.
- **Translation completeness is surfaced, not forced.** The admin sees which languages are missing and can publish with the default culture while completing others.
- **Consistency by reuse.** Because every content type uses the same screen and model, adding content is the same motion regardless of type, which keeps the single admin efficient.
- **Migration-friendly.** When the previous site's content is supplied, it maps onto this model by assigning each existing item a `ContentType` and importing its text as translations — no schema change needed.
