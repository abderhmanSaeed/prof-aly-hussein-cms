# 90 — Event Video Support Report

> **Status:** Implemented and verified locally (build + tests + live render). **No commit,
> tag, or push** — awaiting review.
> Scope: add an **optional** YouTube video to Events, reusing the existing Videos module
> pattern. The image gallery is untouched — video and gallery coexist.

---

## 1. What was requested

- Events should support a video **exactly like** Digital Resources → Educational Videos
  (the existing proven YouTube integration).
- **Admin:** optional Event Video URL (`youtube.com/watch?v=…` or `youtu.be/…`), stored the
  same way as the Video module (parsed to the 11-char id).
- **Public event page:** if a video exists, show a new section ("فيديو الفعالية" / "Event
  Video") with a thumbnail + play button, responsive, RTL/LTR aware; clicking opens an
  embedded player (Option A).
- **Gallery rules:** images-only → gallery as today; video-only → video section; images+video
  → both. Never remove or replace the gallery.

---

## 2. Reuse-first audit (what already existed, and what was reused)

| Existing asset | Location | Reused for events? |
|----------------|----------|--------------------|
| YouTube URL/id helper (`TryGetVideoId`, `ThumbnailUrl`, `EmbedUrl`, `WatchUrl`) | `Domain/Common/YouTube.cs` | ✅ Used verbatim — no new parsing code |
| YouTube id validation rule | `Domain/Common/ContentRules.IsValidYouTubeId` | ✅ Via the helper |
| Field length for the id | `FieldLengths.YouTubeVideoId` (20) | ✅ Same constant |
| Video id storage pattern (id only, never uploaded) | `Video.YouTubeVideoId` | ✅ Mirrored on `Event` |
| Admin "YouTube URL" field + thumbnail preview UX | `Admin/Pages/Videos/Upsert.*` | ✅ Same markup pattern + `YouTube_Invalid` message |
| Embed iframe markup + `.video-embed` box | `Pages/Public/VideoDetail.cshtml`, `public.css` | ✅ Same player box / aspect ratio |
| Thumbnail + `.video-play` button (card pattern) | `Pages/Public/Videos.cshtml`, `public.css` | ✅ Same play-button visual |

**No new component, entity, page, route, or video system was created.** The only genuinely
new code is the small click-to-play facade handler in `public.js` and a few CSS rules that
re-point the *existing* `.video-embed` / `.video-play` styles at the facade.

---

## 3. Database changes

One **additive, nullable** column — no data migration, no change to existing columns.

```
Migration: 20260622194829_AddEventVideo
  + ContentItem.EventVideoYouTubeId   TEXT  maxLength 20  NULL
```

### Why a separate column (not the Video column)

The first attempt reused the existing `YouTubeVideoId` TPH column by adding a same-named
property to `Event`. Because the two sibling properties differ in nullability (Video = required,
Event = optional), EF Core did **not** share the column — it remapped `Video.YouTubeVideoId`
to a brand-new `Video_YouTubeVideoId` column, which would have **orphaned all existing video
ids**. That migration was reverted. The final design gives `Event` its own dedicated nullable
column (`EventVideoYouTubeId`), so the Video column is never touched and existing video data is
guaranteed intact. (`Event.ExternalUrl` is already used for the event's external link, so it
could not double as the video field.)

Auto-applied at startup by the existing `DatabaseInitializer`; also applied to the local
`App_Data/app.db` during verification.

---

## 4. Admin changes

**`Domain/Entities/Content/Event.cs`** — added optional `string? VideoYouTubeId` (id only,
doc-commented; same format as `Video.YouTubeVideoId`).

**`Infrastructure/.../ContentConfigurations.cs`** — added `EventConfiguration` mapping
`VideoYouTubeId` → column `EventVideoYouTubeId`, `HasMaxLength(FieldLengths.YouTubeVideoId)`.

**`Admin/Pages/Events/Upsert.cshtml.cs`**
- `InputModel.EventVideoUrl` (optional) + a `PreviewVideoId` for the thumbnail.
- GET: pre-fills the field from `YouTube.WatchUrl(entity.VideoYouTubeId)` when set.
- POST: if non-empty, parses with `YouTube.TryGetVideoId`; invalid → `ModelState` error using
  the existing `YouTube_Invalid` message; empty → clears the video. Stores the 11-char id into
  `entity.VideoYouTubeId` — exactly the Video module's flow.

**`Admin/Pages/Events/Upsert.cshtml`** — new "Event Video URL (optional)" input with the same
placeholder/help/preview treatment as the Videos admin form. Cover, gallery, categories, and
all other fields are unchanged.

---

## 5. Public page changes

**`Pages/Public/EventDetail.cshtml`** — a new section rendered **only** when the event has a
video, placed above the existing gallery:

```cshtml
@if (!string.IsNullOrWhiteSpace(Model.Event.VideoYouTubeId))
{
    <h2 class="panel-title mt-4">@L["Events_VideoSection"]</h2>
    <div class="video-embed video-facade" data-embed="@YouTube.EmbedUrl(Model.Event.VideoYouTubeId)"
         role="button" tabindex="0" aria-label="@L["Events_PlayVideo"]">
        <img src="@YouTube.ThumbnailUrl(Model.Event.VideoYouTubeId)" alt="@t?.Title" loading="lazy" />
        <span class="video-play" aria-hidden="true">▶</span>
    </div>
}
```

The gallery block is a **separate, independent** `@if (gallery.Count > 0)` — so the four gallery
rules hold by construction:

| Event content | Video section | Gallery |
|---------------|:-------------:|:-------:|
| images only | — | shown (unchanged) |
| video only | shown | — |
| images + video | shown | shown |
| neither | — | — |

**Interaction (Option A — embedded player).** The existing Videos module navigates to a detail
page that embeds an iframe; there is no separate "modal" component in the project to reuse.
Rather than open a new tab (Option B), this implements the same *embedded-player* experience
inline via a lightweight **click-to-play facade**: the thumbnail + play button (the proven
`.video-embed` box and `.video-play` button) is swapped for the `youtube-nocookie` iframe on
click — same visual language as Educational Videos, on the same page, no extra navigation.

- **`wwwroot/js/public.js`** — on click of `.video-facade`, replaces the thumbnail with the
  embed iframe (`?autoplay=1`, same `allow`/`referrerpolicy`/`allowfullscreen` attributes the
  Videos page uses). Keyboard-accessible: `role="button"` + `tabindex="0"`, activates on
  Enter/Space.
- **`wwwroot/css/public.css`** — `.video-facade` (cursor), `.video-embed > img` (fills the box),
  and extended the existing play-button selector to `.book-cover .video-play, .video-embed
  .video-play`. Responsive (16:9 box) and direction-agnostic (centered play button), so it is
  RTL/LTR-correct with no extra rules.

**Localization** — new keys in all three locales (`SharedResource{,.ar,.fr}.resx`):
`Events_VideoSection`, `Events_PlayVideo`, `F_EventVideoUrl`, `EventVideo_Help`. The validation
message reuses the existing `YouTube_Invalid`.

---

## 6. Verification results

### Automated
| Check | Result |
|-------|--------|
| Full solution build | ✅ `Build succeeded. 0 Error(s)` (C# + Razor) |
| Test suite | ✅ **35/35 passed** |
| New data-layer tests (`EventVideoTests`, run against a freshly **migrated** temp SQLite DB) | ✅ Event video id round-trips; **video coexists with a 2-image gallery**; `Event.VideoYouTubeId` and `Video.YouTubeVideoId` resolve to **independent columns** (no collision/data loss) |
| Migration shape | ✅ Single additive nullable column; Video column untouched (verified by inspecting the generated migration after reverting the column-sharing attempt) |

### Live render (app run locally; sample event = 2 gallery images + 1 YouTube video)
Created the requested sample event and fetched the public detail page in both languages:

| Page | Observed in rendered HTML |
|------|---------------------------|
| `/en/events/…` (HTTP 200) | "Event Video" heading, `video-facade`, `data-embed=https://www.youtube-nocookie.com/embed/…`, YouTube thumbnail, `video-play` — **and** "Gallery" with `gallery-grid` + 2 `gallery-item` (coexist) |
| `/ar/events/…` (HTTP 200) | "فيديو الفعالية" heading + facade/embed/thumbnail/play — **and** "معرض الصور" with 2 gallery items (coexist) |
| Arabic page direction | `<html lang="ar" dir="rtl">` — RTL confirmed; English renders LTR |
| Responsive | Player uses the existing 16:9 `.video-embed` box and `card`/`gallery` grids — same responsive behavior as the Videos and gallery sections already shipped |

### Manual checks recommended for the reviewer (need an admin login / a browser)
- Admin **create/edit**: paste a `watch?v=` and a `youtu.be/` URL → saved; clear the field →
  video removed; paste an invalid string → inline "Enter a valid YouTube URL…" error. (The
  code path mirrors the Videos admin, compiles, and the field/preview render; only the
  authenticated click-through is left to confirm visually.)
- Click the thumbnail on desktop and mobile widths → inline embedded player starts.

> **Note — sample data:** a published sample event **"Sample Event With Video"**
> (slugs `sample-event-with-video` / `sample-event-with-video-ar`, placeholder video id
> `dQw4w9WgXcQ`, reusing two existing uploaded images) was inserted into the local
> `App_Data/app.db` for the live render check. It is **local DB only** (not part of the code
> change). Delete it from Admin → Events if you don't want it. The temporary seeding helper used
> to create it was removed; the suite is back to 35 tests.

---

## 7. Files touched

```
src/ProfAly.CMS.Domain/Entities/Content/Event.cs                         (+ VideoYouTubeId)
src/ProfAly.CMS.Infrastructure/Persistence/Configurations/ContentConfigurations.cs (+ EventConfiguration)
src/ProfAly.CMS.Infrastructure/Persistence/Migrations/20260622194829_AddEventVideo.cs (+ Designer, + snapshot)
src/ProfAly.CMS.Web/Areas/Admin/Pages/Events/Upsert.cshtml.cs            (video URL: bind/parse/store)
src/ProfAly.CMS.Web/Areas/Admin/Pages/Events/Upsert.cshtml               (video URL field + preview)
src/ProfAly.CMS.Web/Pages/Public/EventDetail.cshtml                      (video section)
src/ProfAly.CMS.Web/wwwroot/css/public.css                              (facade styles)
src/ProfAly.CMS.Web/wwwroot/js/public.js                                (click/keyboard facade)
src/ProfAly.CMS.Web/Resources/SharedResource.resx / .ar.resx / .fr.resx (4 keys each)
tests/ProfAly.CMS.Tests/EventVideoTests.cs                              (new persistence tests)
```

No commit. No tag. No push. Awaiting review.
