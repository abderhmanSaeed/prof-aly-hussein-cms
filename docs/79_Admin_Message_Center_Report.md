# 79 — Admin Message Center Report

**Date:** 2026-06-20
**Scope:** Admin Contact Messages module (SuperAdmin-only). Uses the existing `ContactMessage` table — **no schema change**.
**Constraints:** existing admin shell, Bootstrap 5, current design language; responsive, RTL/LTR, accessible; localized AR/EN/FR. No commit/tag/push.

> Filename note: requested `75_…` is taken (`75_Public_Website_Fixes_Report.md`); this report is **79** (next free) to avoid overwriting.

---

## 1. Pages Implemented

| Route | Page | Purpose |
|---|---|---|
| `/Admin/Messages` | `Messages/Index` | Inbox-style list with status, sender, email, subject, received date, actions; filter tabs + search |
| `/Admin/Messages/View/{id}` | `Messages/View` | Full message detail; **auto-marks Read** on open |
| `/Admin` (dashboard) | `Index` | **Messages stats card** — Total, Unread badge, latest 5 |

All under the `Areas/Admin` folder, which is gated by the `RequireSuperAdmin` policy (set in `Program.cs`) — so the module is SuperAdmin-only with no extra code.

## 2. Messages List (`/Admin/Messages`)

- **Columns:** Status (Unread/Read badge), Sender Name (links to detail), Email (mailto, hidden on xs), Subject, Received (UTC, hidden < lg), Actions.
- **Sorting:** `CreatedUtc DESC` (newest first), tie-broken by Id.
- **Unread highlight:** unread rows get the `msg-unread` class + bold sender/subject and a red "Unread" badge; read rows render normally.
- **Filters:** All / Unread / Read (button-group tabs, active state highlighted) via `?filter=`.
- **Search:** by Name / Email / Subject via `?q=` (`EF.Functions.Like`, case-insensitive); preserved alongside the filter.
- **Row actions:** View · Mark read / Mark unread (toggles based on current state) · Delete (with `confirm()` dialog). Header badges show total + unread counts.

## 3. Message Details (`/Admin/Messages/View/{id}`)

- Shows Sender Name, Email (mailto), Received date (UTC), IP address (when present), Subject, and the full message (`white-space: pre-wrap`).
- **Auto-mark Read:** opening an unread message sets `IsRead = true` and saves (the documented read workflow).
- Actions: Reply by email (prefilled `mailto:` with `RE:` subject), Mark unread, Delete (confirm). Back link to the inbox.

## 4. Read / Unread Workflow

```
New submission           → IsRead = false (unread, highlighted in the list, counted in Unread)
Open detail page         → auto IsRead = true
List "Mark read/unread"  → toggles IsRead explicitly
Detail "Mark unread"     → IsRead = false (returns to inbox)
```

Counts (header badge + dashboard) reflect `IsRead` live.

## 5. Dashboard Integration

- The dashboard "Messages" tile is now **live** (was a "soon" placeholder): shows **Total** messages, an **Unread** badge when > 0, and the **latest 5** (name — subject — date, unread bolded). The whole tile links to `/Admin/Messages`.
- `IndexModel` now injects `AppDbContext` and computes `TotalMessages`, `UnreadMessages`, `LatestMessages` in `OnGetAsync`.

## 6. Navigation

- The sidebar **Communication → "Messages"** item (previously a disabled "soon" placeholder) is enabled → `/Messages/Index`, with active-state highlighting. (Suggested envelope icon: the admin sidebar is text-based; an icon can be added when the sidebar adopts icons globally.)

## 7. UX Decisions

- **Inbox metaphor:** unread = bold + red badge + row tint, mirroring familiar mail clients; read = muted/normal.
- **Non-destructive by default:** delete always behind a confirm dialog; mark-read/unread are reversible.
- **Responsive:** Bootstrap table; Email column hidden < md, Received hidden < lg, so phones show Status/Name/Subject/Actions. Filter tabs + search wrap.
- **Accessible:** semantic table with `<th>` headers; `role="search"`; `aria-label`s on filter group and search; actions are real buttons/links; status conveyed by text badge (not color alone).
- **RTL/LTR:** all via Bootstrap utilities + logical layout; email/date/IP forced `dir="ltr"`.

## 8. Future-Ready Architecture (prepared, not implemented)

- Read/unread state already modeled (`IsRead`); the resolver/handlers are small and isolated, so **Archive** (add a bool/enum), **Reply from dashboard** (the detail page already has a reply affordance), **Export** (the `Index` query is reusable), **Spam protection** (the public form already has a honeypot; a flag could be added), and **Email notifications** (hook on `ContactMessage` insert) can be layered without restructuring.
- No premature abstractions — plain PageModels + `AppDbContext`, consistent with the rest of the admin.

## 9. Files Changed (3 added pages + 4 modified)

**Added:** `Areas/Admin/Pages/Messages/Index.cshtml(.cs)`, `View.cshtml(.cs)`.
**Modified:** `Areas/Admin/Pages/Index.cshtml(.cs)` (dashboard card), `Areas/Admin/Pages/Shared/_Sidebar.cshtml` (enable Messages), `Resources/SharedResource.{resx,ar,fr}` (15 keys each).

No domain/persistence/migration changes (`ContactMessage` DbSet already existed).

## 10. Verification

```
dotnet build → 0 errors / 0 warnings ; dotnet test → 31/31
Seeded 3 messages via the public contact form (all unread):
  /Admin/Messages → 200; sidebar Messages link present; dashboard shows Total=3 + latest 5
  list: 3 unread rows; filter=unread→3, filter=read→0; search "Visitor 2" → match
  open View/{id} → auto IsRead=1; unread count 3 → 2
  delete → 302; messages 3 → 2
Verified against a throwaway temp DB; real App_Data untouched; temp deleted; no stray processes.
```

**⏸ Admin Message Center complete. No commit/tag/push — awaiting your review.**
