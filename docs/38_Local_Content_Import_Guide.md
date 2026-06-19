# 38 — Local Content Import Guide

**Goal:** import the legacy `data.js` academic content into your local CMS database and verify it appears in the admin.
**Date:** 2026-06-19

> Verified on this machine: with the flag enabled, the importer logged *"Static content import complete"* and populated every module (samples in §4). The import is **idempotent** (only fills empty tables) and **config-gated** (off by default).

---

## 1. Where to set `Seed__ImportStaticContent=true`

The importer runs only when the config value **`Seed:ImportStaticContent`** is `true`. Pick **one** of these (ordered easiest → most explicit):

| # | Where | How | Committed to repo? |
|---|---|---|---|
| **A (recommended)** | **User secrets** (Web project) | `dotnet user-secrets set "Seed:ImportStaticContent" "true" --project src/ProfAly.CMS.Web` | No (local only) |
| B | **Environment variable** for one run | set `Seed__ImportStaticContent=true` (note the **double underscore**) before launching | No |
| C | **`appsettings.Development.json`** | add `"Seed": { "ImportStaticContent": true }` | Yes ⚠️ (don't commit if you want it local) |

> The default `appsettings.json` already has `"Seed": { "ImportStaticContent": false }`. Your password is already in user secrets (`AdminAccount:Password`), so option **A** keeps both local-only.

**PowerShell one-run (option B):**
```powershell
$env:Seed__ImportStaticContent="true"; dotnet run --project src/ProfAly.CMS.Web
```

---

## 2. Easiest way to run the import locally

The import runs **automatically at startup** (it's part of the database initializer) when the flag is on and the content tables are empty.

**Recommended steps (option A):**
```bash
# from the repo root: ProfAly.CMS/
dotnet user-secrets set "Seed:ImportStaticContent" "true" --project src/ProfAly.CMS.Web

# run (Visual Studio: just press F5 / run the https profile; or CLI:)
dotnet run --project src/ProfAly.CMS.Web
```
On startup the log shows:
```
Importing static content…
Imported profile.
Static content import complete.
```

Notes:
- You do **not** need to delete the database. The importer fills only empty tables, so it works on a fresh DB or one that only has the seeded admin/settings.
- It is **idempotent** — running again imports nothing new (no duplicates).
- After importing, you can turn it back off (optional): `dotnet user-secrets remove "Seed:ImportStaticContent" --project src/ProfAly.CMS.Web`.
- If you ever want to re-import from scratch: stop the app, delete `src/ProfAly.CMS.Web/App_Data/app.db` (+ `-wal`/`-shm`), then run again.

---

## 3. Verify in the admin

Sign in at **`https://localhost:7129/Admin`** (email `admin@aly-hussein.local`, password from user secrets — see `32_Admin_Login_Credentials.md`), then open each page from the sidebar and confirm the row counts:

| Admin page | URL | Expected |
|---|---|---|
| **Books** | `/Admin/Content?type=Book` | **14** (3 marked featured) |
| **Publications** | `/Admin/Content?type=Publication` | **9** |
| **Theses** | `/Admin/Theses` | **57** (filter: Supervised 22 · Examined 33 · In-progress 2) |
| **Experience** | `/Admin/Experience` | **8** |
| **Teaching** | `/Admin/Teaching` | **16** (8 undergraduate + 8 graduate) |
| **Activities** | `/Admin/Activities` | **5 groups** (26 items total) |

Also populated: Profile (with bio + DOB 24 Feb 1966), Qualifications (4), Skills (5), Memberships (10), Statistics (5), Credibility (5).

> Lists display the **Arabic** (default-culture) text. Switch the admin language (top bar) to English/French; English shows the imported English; French is empty (intentional — renders via fallback).

---

## 4. Verified sample data (what you should see)

Captured from an actual import on this machine (English titles shown):

**Books (14)**
- A New Model for Educational Objectives & Its Applied Reflections on the Curriculum System
- Environment & Environmental Education from an Islamic Perspective
- Social Studies Teaching Methods …

**Publications (9)**
- The Flipped Classroom Strategy and the Development of Teaching Skills … : An Experimental Study
- Effectiveness of Teaching Based on Research Projects and Discussion Seminars … at Al-Azhar …

**Theses (57)** — Researcher | Relationship | Degree | Year
- Sameh Gomaa | Supervised | PhD | 2013
- Sami Abdellatif | Supervised | Master | 2013
- Hamed Mostafa | Supervised | Master | 2015 …

**Experience (8)** — Role | Organization
- Professor of Curriculum & Instruction | Faculty of Education, Al-Azhar University, Cairo
- Associate Professor of Curriculum & Instruction | … Al-Azhar …

**Teaching (16)** — Level | Course
- Graduate | Microteaching
- Graduate | Curriculum (General Diploma) … (+ 8 Undergraduate)

**Activities (5 groups)**
- Training & Quality Assurance · International Projects & Studies · Consulting · Conferences & Talks · Curriculum Development

---

## 5. Troubleshooting

| Symptom | Cause / fix |
|---|---|
| Lists are empty after running | Flag not picked up. Confirm `dotnet user-secrets list --project src/ProfAly.CMS.Web` shows `Seed:ImportStaticContent = true`; ensure you ran in **Development** (the default for `dotnet run`). |
| "skipping Super Admin seeding" in log | `AdminAccount:Password` not set — see `32_Admin_Login_Credentials.md` (`dotnet user-secrets set "AdminAccount:Password" "…"`). |
| Re-ran but nothing changed | Expected — import is idempotent. To re-import, delete `App_Data/app.db` first. |
| `unable to open database file` | The app creates `App_Data` automatically now; ensure the working directory is the Web project (Visual Studio handles this). |
| Can't reach `https://localhost:7129` | Trust the dev cert: `dotnet dev-certs https --trust`, or use `http://localhost:5203`. |

**No database is committed** — the import populates your local `App_Data/app.db` only.
