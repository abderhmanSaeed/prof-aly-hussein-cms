# 05 — Entities, Properties & Validation Rules (FINAL)

**Status:** **FINAL — single source of truth (v2.0).** Implementation-ready entity catalogue after the migration analysis (docs 16/17). Types are expressed neutrally (C#-leaning). "Required" means non-null and non-empty after trim. All timestamps are UTC ISO-8601 stored as TEXT in SQLite.

Common conventions:
- `Culture` ∈ {`ar`, `en`, `fr`} — **all three first-class; French content may be empty (fallback applies).**
- Surrogate integer `Id` PKs unless noted.
- `CreatedUtc` set on insert; `ModifiedUtc` set on every update.
- Every `*Translation` table is unique on `(ParentId, Culture)` with `CHECK (Culture IN ('ar','en','fr'))`.

> **v2.0 change summary:** added `Qualification`, `Skill`, `Membership`, `StatItem`, `Credibility`, `ActivityGroup`+`Activity`; extended `Profile`, `ContentItem`/translation, `Course`, `ExperienceEntry`; removed Thesis `Supervisor` (→ `RelationshipType` + `ResearcherName`); added `Redirect`; added `SiteSettings.DefaultTheme`.

---

## 1. SiteSettings (singleton)
| Property | Type | Validation |
|---|---|---|
| Id | int | fixed = 1 |
| DefaultCulture | string(2) | required; in {ar,en,fr} |
| DefaultTheme | string(10) | in {Light,Dark}; default Light |
| LogoMediaId | int? | FK→MediaFile (Image) |
| FacebookUrl | string(300)? | valid absolute URL if present |
| WhatsAppNumber | string(30)? | digits/`+` only if present |
| ContactEmail | string(254) | required; valid email |
| PrimaryColor / SecondaryColor | string(9)? | hex color if present |

### SiteSettingsTranslation `(SiteSettingsId, Culture)`
`SiteTitle` (req, 150), `FooterText` (500?), `Tagline` (200?).

## 2. Profile (singleton)
| Property | Type | Validation |
|---|---|---|
| Id | int | fixed = 1 |
| PhotoMediaId | int? | FK→MediaFile (Image) |
| DateOfBirth | date? | optional; rendered per culture (avoids stored-digit/locale issues) |
| Email | string(254)? | valid email if present |
| Phone | string(30)? | optional |

### ProfileTranslation `(ProfileId, Culture)`
| Property | Type | Validation |
|---|---|---|
| FullName | string(150) | required |
| ShortName | string(80)? | brand/nav short form |
| Title | string(200) | required |
| Positioning | string(300)? | hero one-line positioning |
| ShortBio | string(500)? | optional |
| FullBio | string (long)? | optional, rich text |
| Nationality | string(100)? | optional |
| MaritalStatus | string(100)? | optional |
| Location | string(150)? | optional |
| Languages | string(250)? | optional |
| CvFileId | int? | FK→MediaFile (Pdf); per-culture CV download |

## 3. ContentItem (TPH base)
| Property | Type | Validation |
|---|---|---|
| Id | int | PK |
| ContentType | enum/string | required; one of the 8 subtypes |
| CoverImageId | int? | FK→MediaFile (Image) |
| PdfFileId | int? | FK→MediaFile (Pdf); null when ContentType = Video |
| ExternalUrl | string(500)? | valid URL if present |
| YouTubeVideoId | string(20)? | required when Video; 11-char YouTube ID pattern |
| PublicationYear | int? | 1900..currentYear+1 if present |
| EventDateUtc | datetime? | optional |
| Doi | string(100)? | Publication/Research; DOI pattern if present |
| DegreeLevel | string(10)? | Thesis only; in {Master, PhD} |
| RelationshipType | string(12)? | **Thesis only; in {Supervised, Examined, Ongoing}** |
| ProjectStatus | string(12)? | Project only; in {Ongoing, Completed} |
| Role | string(150)? | Project only (the professor's role) |
| ResourceType | string(50)? | Resource/Enrichment only |
| IsPublished | bool | default false |
| IsFeatured | bool | default false (homepage featuring; primarily Book) |
| SortOrder | int | default 0 |
| ViewCount / DownloadCount | int | default 0; non-negative |

**Cross-field rules**
- `Video` ⇒ `YouTubeVideoId` required, `PdfFileId` null.
- `Book` ⇒ `CoverImageId` recommended; `PdfFileId` optional (enables preview/download popup).
- `Thesis` ⇒ `RelationshipType` required; `DegreeLevel` required; `ResearcherName` (translation) required.
- At least the default-culture translation required before an item can be published.

### ContentItemTranslation `(ContentItemId, Culture)`
| Property | Type | Validation |
|---|---|---|
| Title | string(250) | required |
| Slug | string(250) | required; lowercase, url-safe; unique per (ContentType, Culture) |
| Summary | string(600)? | shown on cards/popup |
| Body | string (long)? | rich text |
| Journal | string(250)? | Publication/Research (translatable venue/journal+issue) |
| Authors | string(500)? | Publication/Research |
| Publisher | string(250)? | Book (e.g. "Dar Al-Masirah, Jordan") |
| AuthorshipRole | string(150)? | Book (e.g. "Sole author", "With dept. members") |
| ResearcherName | string(200)? | Thesis (the student/researcher) |
| MetaTitle | string(70)? | SEO; defaults to Title |
| MetaDescription | string(160)? | SEO |
| MetaKeywords | string(250)? | SEO |

**Slug rules:** auto-generated from Title (transliteration-aware for Arabic), editable, validated against the uniqueness index, stable-by-default once published (changing it offers a `Redirect`).

## 4. Category
`Category`: `Id`, `SortOrder`.
`CategoryTranslation` `(CategoryId, Culture)`: `Name` (req, 120), `Slug` (req, 120, unique per Culture).
`ContentItemCategory`: composite PK `(ContentItemId, CategoryId)`.

## 5. Qualification (NEW)
`Qualification`: `Id`, `Year` (int?, 1900..currentYear), `SortOrder`.
`QualificationTranslation` `(QualificationId, Culture)`: `Degree` (req, 200), `Institution` (req, 200), `Grade` (150?).

## 6. Skill (NEW)
`Skill`: `Id`, `SortOrder`.
`SkillTranslation` `(SkillId, Culture)`: `Name` (req, 200).

## 7. Membership (NEW)
`Membership`: `Id`, `Kind` (req; in {Society, Board}), `SortOrder`.
`MembershipTranslation` `(MembershipId, Culture)`: `Name` (req, 250).

## 8. StatItem (NEW)
`StatItem`: `Id`, `Value` (int, ≥ 0), `Suffix` (string(5)?, e.g. `+`), `SortOrder`.
`StatItemTranslation` `(StatItemId, Culture)`: `Label` (req, 150).

## 9. Credibility (NEW)
`Credibility`: `Id`, `LogoMediaId` (int? FK→MediaFile Image), `SortOrder`.
`CredibilityTranslation` `(CredibilityId, Culture)`: `Name` (req, 150).

## 10. ActivityGroup + Activity (NEW)
`ActivityGroup`: `Id`, `SortOrder`.
`ActivityGroupTranslation` `(ActivityGroupId, Culture)`: `Name` (req, 200).
`Activity`: `Id`, `ActivityGroupId` (FK, req), `SortOrder`.
`ActivityTranslation` `(ActivityId, Culture)`: `Text` (req, 1000).

## 11. PageSection
`PageSection`: `Id`, `PageKey` (req, unique, 50; e.g. home/about/contact), `SortOrder`.
`PageSectionTranslation` `(PageSectionId, Culture)`: `Heading` (200?), `Body` (long?, rich text).

## 12. ExperienceEntry
`ExperienceEntry`: `Id`, `StartDateUtc` (date?, optional), `EndDateUtc` (date?, null = present; if both present, ≥ StartDate), `SortOrder`.
`ExperienceEntryTranslation` `(ExperienceEntryId, Culture)`: `Role` (req, 200), `Organization` (req, 200), `Description` (long?), `PeriodLabel` (120?, free-text display e.g. "Jul 2018 – present").

> Ordering is by `SortOrder` (admin-controlled). Dates are optional structured metadata; `PeriodLabel` preserves the exact legacy wording without lossy parsing.

## 13. Course (Teaching)
`Course`: `Id`, `Level` (req; in {Undergraduate, Graduate}), `Period` (50?, e.g. "2023–2024"), `SortOrder`.
`CourseTranslation` `(CourseId, Culture)`: `CourseName` (req, 200), `Institution` (200?), `Description` (long?).

## 14. MediaFile
| Property | Type | Validation |
|---|---|---|
| Id | int | PK |
| StoredFileName | string(120) | required; unique; GUID-based |
| OriginalFileName | string(260) | required; sanitized |
| RelativePath | string(300) | required |
| ContentType | string(120) | required; in allowed MIME set |
| MediaKind | enum | Image / Pdf / Thumbnail |
| SizeBytes | long | > 0; ≤ kind-specific max |
| Width / Height | int? | images only |
| AltText | string(250)? | optional accessibility text |

**Allowed types & limits:** images jpg/png/webp ≤ 5 MB; PDFs ≤ 25 MB. Reject by both extension and content sniffing. **SVG/HTML/executables rejected** (the site favicon ships as a fixed app asset, not through the Media Library).

## 15. ContactMessage
`Id`, `Name` (req, 150), `Email` (req, 254, valid), `Subject` (200?), `Message` (req, 4000, min 5), `IsRead` (bool, default false), `IpAddress` (45?), `CreatedUtc`.
**Anti-abuse:** server-side validation, honeypot field, rate limiting per IP.

## 16. Redirect (NEW)
| Property | Type | Validation |
|---|---|---|
| Id | int | PK |
| FromPath | string(400) | required; unique; e.g. `/about.html` |
| ToPath | string(400) | required; e.g. `/ar/about` |
| StatusCode | int | in {301, 302}; default 301 |
| IsActive | bool | default true |
| Notes | string(250)? | optional |

## 17. ContentEvent / PageView
- **ContentEvent:** `ContentItemId` (FK), `EventType` ∈ {View, Download, Play}, `Culture`, `CreatedUtc`.
- **PageView:** `Path` (string 300), `Culture`, `CreatedUtc`.
- Append-only; pruned/aggregated on a schedule.

## 18. PageSeo
`(PageKey, Culture)` unique → `MetaTitle` (70?), `MetaDescription` (160?), `MetaKeywords` (250?). Falls back to sensible defaults when empty.

## 19. Identity (ASP.NET Core Identity)
Single Super Admin user; one role `SuperAdmin`; no self-registration; seeded at deploy.

---

## Validation Strategy (cross-cutting)
- **Server-side is authoritative** (data annotations + service rules in PageModels/services).
- **Culture completeness:** items may be saved as drafts with partial translations; publishing requires the default-culture translation; the UI flags missing AR/EN/**FR** per item.
- **Referential safety:** deleting a referenced `MediaFile` warns and sets references null (covers `CoverImageId`, `PdfFileId`, `Profile.PhotoMediaId`, `ProfileTranslation.CvFileId`, `SiteSettings.LogoMediaId`, `Credibility.LogoMediaId`).
- **Sanitization:** rich-text bodies (`FullBio`, `Body`, `PageSection.Body`) are sanitized (tag allowlist) to prevent stored XSS.
- **Enums** validated in app + DB CHECK where listed in doc 03 §4.
