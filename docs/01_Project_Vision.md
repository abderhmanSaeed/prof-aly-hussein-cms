# 01 — Project Vision

**Project:** Dr. Aly Hussein — Academic Portfolio & Content Management Platform
**Type:** Dynamic replacement for an existing static academic portfolio website
**Owner:** Single Super Admin (the professor)
**Document status:** Planning baseline (v1.0)

---

## 1. Project Goals

The platform replaces a static, hard-to-update website with a self-managed, multilingual academic presence. The professor must be able to maintain every piece of content without developer involvement or code changes.

- **Eliminate developer dependency for content.** Every text block, image, document, link, and navigation label is editable from an admin dashboard.
- **Centralize the academic record.** Publications, research, books, theses, projects, teaching, videos, and enrichment material live in one structured, searchable system.
- **Be genuinely trilingual.** Arabic (primary, RTL), English, and French are first-class — both the interface and the content.
- **Be discoverable.** Strong, per-page, per-language SEO so the professor's work surfaces in search and academic indexing.
- **Stay tiny and durable.** A single deployable application, a file-based database, and a near-zero hosting cost that one person can maintain for years.

## 2. Business Objectives

| Objective | Measure of success |
|---|---|
| Self-service content management | Professor publishes/edits content with zero developer tickets |
| Trilingual reach | Every public page available and indexable in AR/EN/FR |
| Academic credibility & discoverability | Pages indexed with correct structured data; sitemap submitted |
| Low total cost of ownership | Hosting at or near $0/month; backup is a file copy |
| Long-term sustainability | No paid licenses; single LTS runtime; clear upgrade path |
| Content longevity | All assets (PDFs, images) and data recoverable from a single backup set |

## 3. Scope (In Scope)

**Public website**
- Home, About, Experience, Research, Publications, Projects, Theses, Teaching, Contact
- New sections: Videos, Enrichment Information, Resources
- Per-section "View All" entry points from the homepage
- Site-wide search across publications, books, research, videos, resources
- Category/topic filtering
- Contact form
- Footer social/contact links (Facebook, WhatsApp, Email)
- Trilingual UI with culture switching and full RTL support for Arabic

**Admin dashboard (single Super Admin)**
- Secure login (single account)
- Section-by-section content management mirroring the public site
- CRUD for Videos, Books, Publications, Research, Projects, Theses, Resources, Enrichment
- Categories management
- Image and PDF uploads with type/size enforcement
- YouTube video integration (link/ID, not hosting)
- Header and Footer management
- Profile and biography management (photo, name, title, bio)
- SEO settings per page and per content item
- Contact message inbox
- Basic statistics (views, downloads, video plays, top content)
- Backup export and restore

**Cross-cutting**
- Dynamic content (no code edits to change any displayed content)
- File management (images, PDFs) with naming conventions and storage rules
- Multilingual content model
- SEO (meta data, slugs, sitemap, structured data)
- Backup & restore

## 4. Out of Scope

These are deliberately excluded to protect simplicity, cost, and the single-admin design. Each can be revisited as a future phase.

- **Multi-user editing, roles, or workflow/approval.** One Super Admin only.
- **Self-hosted video streaming.** Video is referenced via YouTube; the platform never stores or streams video files.
- **Public user accounts, comments, forums, or social features.**
- **E-commerce, payments, paywalls, or subscriptions.**
- **Native mobile applications.** The site is responsive (Bootstrap 5) instead.
- **Real-time features** (chat, notifications, live updates).
- **Automatic machine translation.** Translations are authored by the admin; the system stores and serves them but does not generate them.
- **External CMS/headless integration, microservices, message queues, or container orchestration.** The system is a single monolith by design.
- **Advanced analytics / heatmaps / third-party trackers.** Statistics are lightweight and self-contained.

## 5. Guiding Principles

1. **Simplicity over flexibility we will not use.** Single admin, low traffic — the architecture matches that reality.
2. **Server-rendered first.** Razor Pages output complete HTML for SEO and accessibility without a client-side framework.
3. **One artifact, one file database, one uploads folder.** Deployment, backup, and reasoning stay trivial.
4. **Design once to avoid refactoring.** The multilingual model and content model are settled in this phase so later phases only add, never restructure.
5. **No paid dependencies on the critical path.** Free runtime, free database engine, free/low-cost hosting, free TLS.

## 6. Note on Inputs

The requirements document (`MASTER_PROMPT_Dr_Aly_Hussein_Website.md`) is the authoritative source for this plan. The **previous static website project** and the **existing content/structure** were referenced but not yet supplied. Providing them later will let us:
- Produce an exact content inventory (counts of publications, books, etc.) to validate the data model.
- Map legacy URLs to new slugs for redirects (preserves existing SEO).
- Pre-seed the database with migrated content during Phase 3.

None of this blocks the architecture; it refines the content layer.
