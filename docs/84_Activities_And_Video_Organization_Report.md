# 84 — Activities & Video Organization Report

> **Status:** Implemented and verified locally. **No commit, tag, or push** — awaiting review.
> Scope: **navigation/UX reorganization only.** No entities, routes, schema, or content logic
> changed. The existing YouTube-based `Video` module is reused for "Video Clips" — no second
> video system, no new entity, no duplicate code.

---

## 1. What was requested

- Make **فعاليات متنوعة (Diverse Activities)** a **parent** section containing **Events** +
  **Video Clips** (not a replacement of Events with videos).
- **Renames:** Videos → **Educational Videos**; Recommended Books → **Books Worth Reading**;
  Events → **Diverse Activities**.
- **Video Clips** reuses the existing `Video` content type and behaves exactly like the current
  Videos section (YouTube URL, thumbnail, listing, details, embedded player).
- **Events** unchanged (date, location, gallery, cover, external link). No schema changes.

**Clarification resolved:** you confirmed the Video module should appear in **both** locations —
as "Educational Videos" under Digital Resources **and** as "Video Clips" under Diverse Activities.
Both nav entries open the same `/videos` pages.

---

## 2. Final navigation structure

```
Digital Resources ▾
├─ Educational Videos    → /{c}/videos               (existing Video module)
├─ Enrichment Materials  → /{c}/enrichment
└─ Books Worth Reading   → /{c}/recommended-books     (renamed; route unchanged)

Diverse Activities ▾   (was the top-level "Events" item)
├─ Events                → /{c}/events                (unchanged)
└─ Video Clips           → /{c}/videos                (SAME existing Video module)
```

`Educational Videos` and `Video Clips` are two doorways to the **same** `/videos` listing and
detail pages. No code, entity, or route was duplicated to achieve this — only a second navigation
link was added.

---

## 3. Changes made (navigation & labels only)

**`Pages/Public/Shared/_PublicLayout.cshtml`** — the top-level `Events` leaf became a
`Diverse Activities` dropdown with two children: `Events` (`/Public/Events`) and `Video Clips`
(`/Public/Videos`). The Digital Resources dropdown is unchanged in structure. The footer
quick-links (which flatten dropdown children) now include Events and Video Clips automatically.

**Localization (AR / EN / FR)** — `SharedResource.resx`, `.ar.resx`, `.fr.resx`:
- **Renamed values (keys unchanged, so the new wording applies everywhere the key is used —
  public pages, breadcrumbs, homepage section, and admin labels):**
  - `Videos_Title`: Videos → **Educational Videos** / الفيديوهات التعليمية / Vidéos éducatives
  - `Nav_RecommendedBooks`, `RecommendedBooks_Title`, `Home_RecommendedBooks`:
    Recommended Books → **Books Worth Reading** / كتب تستحق القراءة / Des livres à lire
- **New keys:**
  - `Nav_DiverseActivities`: **Diverse Activities** / فعاليات متنوعة / Activités diverses
  - `Nav_VideoClips`: **Video Clips** / مقاطع فيديو / Clips vidéo

No other files changed. No `.cs`, no migration, no routes, no admin page logic.

---

## 4. Reuse confirmation (no duplication)

| Concern | Result |
|---|---|
| New Video entity? | **No** — reuses `Video : ContentItem` (ContentType.Video) |
| Second video system / pages? | **No** — Video Clips links to existing `/{c}/videos` + `/{c}/videos/{slug}` |
| Local video upload? | **No** — YouTube-only behavior unchanged |
| Events implementation? | **Unchanged** — date, location, gallery, cover, external link intact |
| Schema / migration? | **None** |
| Duplicate code? | **None** — one nav link added + label text updated |

---

## 5. Verification results

| Check | Result |
|---|---|
| **Build** | `dotnet build` → 0 warnings, 0 errors |
| **Tests** | `dotnet test` → **33/33** passed |
| **Digital Resources dropdown** | Educational Videos · Enrichment Materials · Books Worth Reading |
| **Diverse Activities dropdown** | Events (`/events`) · Video Clips (`/videos`) |
| **Both video links** | "Educational Videos" and "Video Clips" both resolve to `/{c}/videos` |
| **Video Clips behavior** | Opens the existing Videos page; a video detail still renders the YouTube embed (`youtube-nocookie.com/embed/…`) — unchanged |
| **Events** | `/{c}/events` → 200, unchanged |
| **Renamed page titles** | Videos page `<h1>` = "Educational Videos"; Recommended Books page `<h1>` = "Books Worth Reading"; homepage section = "Books Worth Reading" |
| **Localization** | Parent + child labels verified in EN, AR (RTL), FR |

(Verification was read-only — no content was created, modified, or deleted.)

---

## 6. Notes / known limitations

- **Two dropdowns highlight on `/videos`.** Because both "Educational Videos" (Digital Resources)
  and "Video Clips" (Diverse Activities) point to `/videos`, both parent dropdowns show the active
  state when viewing the videos pages. This is expected for a "both locations" design and is purely
  cosmetic.
- **Page title when arriving via "Video Clips".** The shared videos page is titled
  "Educational Videos", so clicking "Video Clips" lands on a page headed "Educational Videos"
  (same content, one canonical title). Left as-is to avoid duplicating the page; can be made
  context-aware later if desired.
- **Existing top-level "Activities" (Projects) is separate.** The site already has an "Activities"
  nav item (`Nav_Activities` → `/activities`, shown as "Projects"/«المشروعات»). It is distinct from
  the new "Diverse Activities" (فعاليات متنوعة) and was left untouched.
- **Admin sidebar** keeps its existing "Digital Resources" group (Videos, Enrichment, Recommended
  Books, Events). Its labels updated automatically via the shared resource keys
  (now "Educational Videos" and "Books Worth Reading"); the admin grouping itself was not
  reorganized, as the request targeted the public navigation/UX.

---

### Awaiting review. No commit, no tag, no push.
