# 91 — Release v1.0 Report

> **Official stable release checkpoint.** Created, committed, tagged, and pushed after full
> pre-release verification. This report is written *after* the push, so it is intentionally
> **not** part of the `v1.0` commit.

---

## Release identifiers

| Item | Value |
|------|-------|
| **Commit SHA** | `56ef3d7009468682bca6a209c02065804a77fdb3` (`56ef3d7`) |
| **Commit message** | `Release v1.0 - Dynamic CMS Platform` |
| **Annotated tag** | `v1.0` → tag object `3f43b1c`, points at commit `56ef3d7` |
| **Branch** | `main` |
| **Remote** | `origin` → https://github.com/abderhmanSaeed/prof-aly-hussein-cms.git |
| **Previous main** | `b406ca0` |

---

## 1. Pre-release verification summary

| # | Check | Result |
|---|-------|--------|
| 1 | Build solution | ✅ **Release build: 0 Warnings, 0 Errors** (`dotnet build -c Release`) |
| 2 | Run all tests | ✅ **35 passed** / 0 failed / 0 skipped |
| 3 | No failing tests | ✅ None |
| 4 | Database NOT deleted | ✅ `App_Data/app.db` present (≈643 KB), untouched |
| 5 | `app.db` gitignored | ✅ Ignored via `.gitignore:35 App_Data/` |
| 6 | `App_Data/Backups` gitignored | ✅ Covered by the same `App_Data/` rule |
| 7 | No secrets tracked | ✅ No `secrets.json` / `.pfx` / `.pem` / `.key` tracked; connection string is file-based SQLite (no password); admin password externalized to env/user-secrets |
| 8 | user-secrets outside repository | ✅ `UserSecretsId` resolves to `~/.microsoft/usersecrets/…` (outside the repo tree) |
| 9 | Content renders correctly | ✅ Live instance returned HTTP 200 with content: `/` (home, AR), `/en/events`, `/ar/videos` |
| 10 | Working tree status | ✅ Reviewed; one stray artifact (`src/ProfAly.CMS.Web/.dotnet/` — CLI first-use sentinels from a prior tooling run) **removed** before commit |

> **Note on the Debug build:** a `dotnet build -c Debug` reports only `MSB3021/MSB3027` file-lock
> errors because the running Visual Studio / app instance holds the `bin/Debug` DLLs. These are
> environmental, not compilation errors. The authoritative **Release** build (separate output
> path) is clean: 0 warnings, 0 errors. Tests (which recompile all libraries) also pass.

---

## 2. Git review — what is in this release

This release bundles the work since `b406ca0`: the navigation simplification (doc 89), event
video support (doc 90), and the accompanying checkpoint/feature reports.

**Files committed (18):**

```
A  docs/88_Stable_Checkpoint_Report.md
A  docs/89_Navigation_Simplification_Report.md
A  docs/90_Event_Video_Support_Report.md
M  src/ProfAly.CMS.Domain/Entities/Content/Event.cs
M  src/ProfAly.CMS.Infrastructure/Persistence/Configurations/ContentConfigurations.cs
A  src/ProfAly.CMS.Infrastructure/Persistence/Migrations/20260622194829_AddEventVideo.cs
A  src/ProfAly.CMS.Infrastructure/Persistence/Migrations/20260622194829_AddEventVideo.Designer.cs
M  src/ProfAly.CMS.Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs
M  src/ProfAly.CMS.Web/Areas/Admin/Pages/Events/Upsert.cshtml
M  src/ProfAly.CMS.Web/Areas/Admin/Pages/Events/Upsert.cshtml.cs
M  src/ProfAly.CMS.Web/Pages/Public/EventDetail.cshtml
M  src/ProfAly.CMS.Web/Pages/Public/Shared/_PublicLayout.cshtml
M  src/ProfAly.CMS.Web/Resources/SharedResource.resx
M  src/ProfAly.CMS.Web/Resources/SharedResource.ar.resx
M  src/ProfAly.CMS.Web/Resources/SharedResource.fr.resx
M  src/ProfAly.CMS.Web/wwwroot/css/public.css
M  src/ProfAly.CMS.Web/wwwroot/js/public.js
A  tests/ProfAly.CMS.Tests/EventVideoTests.cs
```

(18 files changed, +2816 / −12.)

### Categorized summary

- **Features** — Optional YouTube video on Events, reusing the existing Videos module
  (`YouTube` helper, id-only storage, embed pattern). No new entity/page/route/video system.
- **UI/UX** — Public event detail gains a click-to-play video facade (thumbnail + play button →
  inline embedded player), reusing the proven `.video-embed` / `.video-play` styles; responsive
  16:9, keyboard-accessible. "Diverse Activities" is now a plain top-level nav link (dropdown
  removed).
- **Localization** — New AR/EN/FR keys (`Events_VideoSection`, `Events_PlayVideo`,
  `F_EventVideoUrl`, `EventVideo_Help`); validation reuses `YouTube_Invalid`. Removed the now-unused
  `Nav_VideoClips` key. RTL/LTR verified.
- **Content** — No content/data deleted; existing pages render unchanged.
- **Admin** — Events upsert form adds an optional "Event Video URL" field with a thumbnail
  preview and parse/validate/store wired exactly like the Videos admin.
- **Digital Resources** — Video content remains a single source under Digital Resources →
  Educational Videos (the duplicate "Video Clips" nav entry was removed in doc 89).
- **Events** — Events now support a video alongside the image gallery; the two coexist (video
  section and gallery are independent render blocks). New EF migration `AddEventVideo` adds one
  additive nullable column `EventVideoYouTubeId`, leaving the existing Video column untouched.
- **Documentation** — Reports 88 (stable checkpoint), 89 (navigation simplification), 90 (event
  video support) included.
- **Database Safety Layer** — Unchanged and intact; its tests pass. The live database was not
  deleted and remains gitignored along with its `Backups`.

### Suspicious files flagged

- `src/ProfAly.CMS.Web/.dotnet/` — stray .NET CLI first-use sentinel files from an earlier
  tooling command. **Not a project artifact → removed before commit.** Nothing else flagged;
  no DB, `bin/`, `obj/`, or secret files were staged.

---

## 3. Results

| Stage | Result |
|-------|--------|
| **Build** | ✅ Release: 0 Warnings, 0 Errors |
| **Tests** | ✅ 35 passed, 0 failed, 0 skipped |
| **Commit** | ✅ `56ef3d7` on `main` (18 files) |
| **Tag** | ✅ `v1.0` (annotated) → `56ef3d7` |
| **Push (branch)** | ✅ `b406ca0..56ef3d7  main -> main` |
| **Push (tag)** | ✅ `[new tag] v1.0 -> v1.0` |

### Push verification

| Check | Result |
|-------|--------|
| `origin/main` updated | ✅ `origin/main` = `56ef3d7` |
| Tag exists remotely | ✅ `refs/tags/v1.0` present on origin |
| Local HEAD == remote HEAD | ✅ both `56ef3d7` — in sync |
| Working tree clean | ✅ Clean (no pending changes at push time) |

### Git status (at push verification)

```
## main...origin/main      (no ahead/behind)
nothing to commit, working tree clean
```

---

## 4. Release summary

**ProfAly CMS Platform v1.0 — First stable release.** Tag message:

> First stable release of ProfAly CMS Platform. Includes: Dynamic CMS, Public Website,
> Localization (AR/EN/FR), Admin Dashboard, Academic Content, Digital Resources, Events,
> Contact Center, Database Safety Layer.

No features were modified, no code refactored, and no new stage was started — this checkpoint
only packaged, tagged, and published the approved work.

---

_Generated after push verification. This report file is untracked by design and not part of the
`v1.0` commit._
