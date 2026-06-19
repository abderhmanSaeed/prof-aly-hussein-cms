# 44 — CV → Database Mapping & Gap Analysis

**Date:** 2026-06-19
**Inputs compared:** CV PDF (report 43) · `static-content.json` (= imported `data.js`) · current database schema & seed data.

> **Key finding:** the database is a **faithful, loss-free import of `static-content.json`** (every JSON field is mapped; counts verified earlier), and `static-content.json` is a **faithful transcription of `data.js`**. So the question is really **CV vs. `data.js`**. For the *modeled* sections `data.js` matches the CV almost exactly; the gaps are CV sections that `data.js` never captured.

Legend: ✅ Present & complete · 🟡 Present but incomplete · ❌ Missing entirely · ↪ Mapped (to a different entity, by design) · 🔎 Needs manual review

---

## 1. Section-by-Section Mapping

| CV section | DB entity | CV | DB (before) | Status | Notes |
|---|---|---|---|---|---|
| **Profile / contact** | `Profile` (+Translation) | name, title, tel, email, DOB, nationality, marital | all present | ✅ | DOB 1966-02-24 parsed; phone/email present |
| **Biography** | `ProfileTranslation.FullBio` | summary bullets (not a narrative) | 3-para narrative (ar/en) | ✅ | DB bio is a curated narrative; complete |
| **Qualifications** | `Qualification` | 4 (w/ grades) | 4 (w/ grades) | ✅ | PhD *Très Honorable*, MA *Excellent*, etc. |
| **Professional positions** | `ExperienceEntry` | 8 | 8 | ✅ | Exact match 1990→present |
| **Published research** | `ContentItem=Publication` | 9 | 9 | ✅ | Exact (journal + issue + year) |
| **Theses** | `ContentItem=Thesis` | 57 (22/33/2) | 57 (22/33/2) | ✅ | **Exact** incl. relationship split |
| **Authored books** | `ContentItem=Book` | 14 | 14 | ✅ | Exact list 2006–2026; 3 featured |
| **Scientific societies** | `Membership(Society)` | 3 | 3 | ✅ | Exact |
| **Editorial / review boards** | `Membership(Board)` | 7 | 7 | ✅ | Exact |
| **Courses taught** | `Course` | ~16 | 16 (8+8) | ✅ | Count matches; minor UG/grad classification nuance (التدريس المصغر) |
| **Languages** | `ProfileTranslation.Languages` | Arabic, French | present | ✅ | — |
| **Personal skills** | `Skill` | 5 | 5 | ✅ | Exact (the 2 "languages" are stored in Profile, not Skills) |
| **Statistics** | `StatItem` | derived | 5 | ✅ | 30+ yrs · 57 theses · 14 books · 9 papers · 2 countries |
| **Credibility** | `Credibility` | affiliations | 5 | ✅ | Al-Azhar, Montpellier III, NAQAAE, UNESCO, ALECSO |
| **Awards / honors** | `Qualification.Grade` | distinctions | captured as grades | ↪ | *Très Honorable*, *1st of cohort* stored as grades; no separate Awards entity needed |
| **Conferences** | `Activity` (group: المؤتمرات والندوات) | several | 9 items | 🟡→✅ | Curated subset; see §2 |
| **International projects** | `Activity` (group: المشروعات والدراسات الدولية) | several | 5 items | 🟡→✅ | Curated subset; see §2 |
| **Training courses received** | — | **~20** | **0** | ❌→✅ | **Was entirely missing**; now added (see §2 & report 45) |
| **Lab / department projects** | — | ~8 | 0 | ❌→✅ | **Was missing**; now added (see §2 & report 45) |
| **Research papers (distinct type)** | `ContentItem=ResearchPaper` | none distinct | 0 | 🔎 | The CV's "research" = the 9 journal papers (→ Publications). The public **Research** page therefore renders empty and is reserved for future `ResearchPaper` items. Recommend either feeding Publications into it or hiding it until used. |
| **Interests / hobbies** | — | ~8 | 0 | 🔎 | Not modeled by any entity; low priority. Recommend mapping to a small "Interests" list or omitting. |
| **Co-authored Al-Bu'uth curriculum (2022)** | `ContentItem=Book` | 1 | not in 14 | 🔎 | Mentioned as a contribution, not in the main "authored books" list. Left out to keep the "14 books" statistic consistent; add manually if desired (and bump the stat to 15). |

---

## 2. Gaps Identified & Disposition

**❌ Missing entirely → populated (report 45):**
1. **Training courses received (~20)** — François-Rabelais (2001–2003), ICDL/Oracle/Intel (2007–2008), NAQAAE reviewer/TOT/IT courses (2008–2017), RemarkOffice (2022).
2. **Geography-Lab & department projects (~8)** — abstracts compilation, lab development, libraries/website, blog, wall-magazine, workshops, faculty-development & sustainable-PD projects.

Both were added as **new Activity groups** (existing `ActivityGroup`/`Activity` entities + existing importer — **no schema or feature change**), with hand-cleaned bilingual text.

**🟡 Curated subsets (acceptable):** the CV's ~40 granular activities were curated in `data.js` into thematic groups (training/QA, international projects, consulting, conferences, curriculum). The two missing clusters above are now added; the remaining items are representative summaries of the same events and are considered complete for the public site. Finer-grained items can be added later via the admin.

**🔎 Needs manual review (not auto-populated, to avoid low-confidence data):**
- Research-page semantics (`ResearchPaper` type empty by design).
- Interests/hobbies (no entity).
- The 2022 co-authored Al-Bu'uth curriculum as a 15th book.

**Mapped-incorrectly:** none found. All existing data maps to the correct entity and culture.

---

## 3. Conclusion

For every **modeled** CV section the database already matched the CV (often exactly). The only true content gaps were the **Training-Courses** section and the **Lab/Department-projects** cluster, both absent from the original `data.js`. These are now imported. Remaining items are explicitly flagged for manual review rather than populated from low-confidence/garbled extraction. Outcome and verification are in **45_Content_Completion_Report.md**.
