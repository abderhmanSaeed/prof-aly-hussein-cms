# 56 — Videos Module Report (Stage 9)

**Date:** 2026-06-19
**Outcome:** ✅ Videos module implemented (admin + public), YouTube-hosted (URLs only — no uploads). Build 0/0, **tests 31/31**, verified end-to-end. No migration required.
**Boundaries:** no video files uploaded; videos remain on YouTube; only the URL/id is stored. Not committed/tagged/pushed.

---

## 1. Data model (reused — no schema change)

`Video : ContentItem` already existed (TPH, `ContentType.Video`, column `YouTubeVideoId` present in `InitialCreate`), so **no migration** was needed. Field mapping:

| Required field | Stored as |
|---|---|
| Title | `ContentItemTranslation.Title` (per culture) |
| Description | `ContentItemTranslation.Summary` (per culture) |
| YouTubeUrl | `ContentItem.ExternalUrl` (normalized watch URL) |
| YouTubeVideoId | `Video.YouTubeVideoId` (11 chars) |
| PublishDate | `ContentItem.EventDateUtc` |
| SortOrder / IsFeatured / IsPublished | `ContentItem.*` |
| Slug | `ContentItemTranslation.Slug` (auto, unique per culture) |
| SEO | `ContentItemTranslation.MetaTitle / MetaDescription / MetaKeywords` |

Thumbnail/embed URLs are **derived** at render time (never stored).

## 2. Admin pages (`Areas/Admin/Pages/Videos/`)

| Page | Features |
|---|---|
| **Index** | Videos list with thumbnail preview, title, featured/published badges, sort order; **reorder** (↑/↓), **delete** (confirm), **publish/unpublish** toggle, "Create" button |
| **Upsert** | Create/Edit: YouTube URL field (auto-extracts id), live **thumbnail preview**, PublishDate, **Featured** + **Published** flags, and a **3-tab translation editor (AR/EN/FR)** each with Title, Description, and **SEO** (Meta title/description/keywords) |

All under the existing `RequireSuperAdmin` admin shell, Bootstrap RTL/LTR, localized. Sidebar "Videos" entry enabled (Content — Collections).

Covers the required admin features: list, create, edit, delete, reorder, publish/unpublish, featured flag, SEO fields, AR/EN/FR translations.

## 3. Public pages

| Route | Page |
|---|---|
| `/{culture}/videos` | **Grid** of published videos (16:9 thumbnail cards with play overlay), **Featured videos** section (page 1), **pagination** (12/page), responsive, RTL/LTR |
| `/{culture}/videos/{slug}` | **Embedded YouTube player** (privacy-friendly `youtube-nocookie`), description, publish date, "Watch on YouTube" link, and a **Related videos** sidebar |

Both inherit the public layout (culture routing, SEO head with per-video `MetaTitle`/`MetaDescription`, hreflang, theme). "Videos" added to the public navigation.

## 4. YouTube integration

`ProfAly.CMS.Domain.Common.YouTube`:
- **`TryGetVideoId(input, out id)`** — extracts the 11-char id from `watch?v=`, `youtu.be/`, `embed/`, `shorts/`, `/v/` URLs (with extra query params) or a bare id; validates via `ContentRules.IsValidYouTubeId` (regex `^[A-Za-z0-9_-]{11}$`).
- **`ThumbnailUrl(id)`** → `https://img.youtube.com/vi/{id}/hqdefault.jpg`
- **`EmbedUrl(id)`** → `https://www.youtube-nocookie.com/embed/{id}`
- **`WatchUrl(id)`** → `https://www.youtube.com/watch?v={id}`

On save, the admin extracts the id from the pasted URL, stores `YouTubeVideoId` + a normalized `ExternalUrl`; thumbnails and the embed iframe are generated from the id. **No video files are stored.**

## 5. Validation summary

- **YouTube URL:** server-side `TryGetVideoId`; on failure → localized error `YouTube_Invalid` ("Enter a valid YouTube URL or 11-character video id").
- **Title (default culture):** required → localized `Field_Required`; non-default cultures optional (skipped if blank, with Arabic fallback on the public side).
- **Slug:** auto-generated and de-duplicated per culture (`…-2`, `…-3`).
- Standard antiforgery on all admin POSTs.

## 6. Localization summary

- All admin + public strings via `IStringLocalizer<SharedResource>` (the project's shared-resource pattern).
- **New keys (AR/EN/FR):** `Videos_Title`, `Videos_Featured`, `Videos_Related`, `Videos_WatchOnYouTube`, `F_YouTubeUrl`, `YouTube_Help`, `YouTube_Invalid`, `F_PublishDate`. Reused existing `F_Title`, `F_Description`, `F_Featured`, `F_Published`, `F_MetaTitle/Description/Keywords`, `Pub_NoItems`, `Pub_ViewAll`, etc.
- Content is trilingual (AR default/RTL, EN, FR with Arabic fallback).

## 7. Verification

- **Build:** 0 warnings / 0 errors (full Web project; built to a temp output folder because Visual Studio holds `Web/bin`).
- **Tests:** **31/31** — added `YouTubeHelperTests` (14 cases: extraction from all URL forms, invalid inputs, derived URLs).
- **End-to-end** (against a throwaway temp DB — real `App_Data` untouched):
  - Admin login → created a video from `https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=30s` → **id extracted `dQw4w9WgXcQ`**, `ExternalUrl` normalized, featured+published.
  - `/en/videos`, `/ar/videos`, `/fr/videos`, `/en/videos/{slug}` → all **200**.
  - List shows title + thumbnail + Featured section + Videos in nav; detail shows the **`youtube-nocookie` embed** + watch link; Arabic renders RTL.
  - Real `App_Data/app.db` confirmed unchanged; temp DB and build folder deleted; no stray processes.

## 8. Notes

1. **No migration** — `Video` was already in the schema since `InitialCreate`.
2. Arabic-only titles produce a fallback slug (`video`, `video-2`, …) because the slug helper is ASCII-based — consistent with existing content; English slugs are descriptive. Public lookup matches any culture's slug, so links resolve.
3. Excluded by instruction: no file uploads; videos stay on YouTube.

**⏸ Stage 9 (Videos Module) complete and verified. Not committed — awaiting approval.**
