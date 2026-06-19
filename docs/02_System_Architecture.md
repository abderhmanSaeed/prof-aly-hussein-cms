# 02 — System Architecture

## 1. High-Level Architecture

A single **ASP.NET Core Razor Pages (.NET 8 LTS)** application, server-rendered, backed by **SQLite via EF Core**, with uploaded files on the **local filesystem**. There is no separate API, no SPA, and no database server. Public traffic and the admin dashboard are served by the same process but separated by route area and authorization.

```
                         ┌─────────────────────────────────────────┐
   Public visitors  ───▶ │                                         │
   (no login)            │   ASP.NET Core Razor Pages (.NET 8)      │
                         │   - Kestrel web server                  │
   Super Admin      ───▶ │   - Public Pages  (/{culture}/...)      │ ──▶  SQLite file  (app.db, WAL)
   (cookie auth)         │   - Admin Area    (/admin/...)          │ ──▶  Uploads folder (images, pdfs)
                         │   - Localization middleware             │
                         │   - EF Core (SQLite provider)           │
                         └─────────────────────────────────────────┘
                                          │
                                          ▼
                         Reverse proxy (Caddy or Nginx) → automatic HTTPS
```

**Why this shape:** one writer (the admin) and read-mostly public traffic, low volume, minimal budget, and a single maintainer. A server-rendered monolith gives native SEO, the simplest authentication model, and a backup that is a file copy. A SPA + API split would add CORS, token lifecycle, server-side rendering for SEO, and a second build/deploy pipeline — cost with no return at this scale.

## 2. Razor Pages Architecture

- **Public pages** live under `Pages/` and are routed with a culture segment: `/{culture}/...` (e.g. `/en/research`, `/ar/books`). The default culture redirects appropriately.
- **Admin pages** live in a dedicated **Area** (`Areas/Admin/Pages/`) protected by a single authorization policy (`RequireSuperAdmin`). The area is invisible to anonymous users.
- **PageModels** are thin: they validate input and call **application services**; they contain no data-access or business logic directly.
- **Partial views / view components** render shared building blocks (header, footer, language switcher, content cards, the book popup, pagination, breadcrumb, SEO `<head>` block).
- **Tag helpers** handle culture-aware links, slugs, and `hreflang` emission.

## 3. Layer Structure

A pragmatic clean-ish layering inside a single project (or a small solution of 3–4 projects). For a system this size, **a single web project with clearly separated folders** is acceptable and easiest to maintain; the optional multi-project split is noted for teams that prefer hard boundaries.

| Layer | Responsibility | Contents |
|---|---|---|
| **Presentation** | HTTP, rendering, validation, auth | Razor Pages, PageModels, view components, tag helpers, partials, middleware |
| **Application** | Use cases, orchestration, DTOs | Content services, media service, localization service, SEO service, statistics service, backup service, mapping |
| **Domain** | Entities, enums, invariants | `ContentItem` hierarchy, translations, `Category`, `Profile`, `MediaFile`, `ContactMessage`, value rules |
| **Infrastructure** | Persistence, IO, integrations | `AppDbContext` (EF Core + SQLite), repositories (optional), file storage provider, email sender (SMTP), backup/export implementation |

**Recommended physical structure (single solution):**

```
src/
  AcademicPortfolio.Web/            (Razor Pages, Areas/Admin, wwwroot, partials)
  AcademicPortfolio.Application/    (services, DTOs, interfaces)
  AcademicPortfolio.Domain/         (entities, enums)
  AcademicPortfolio.Infrastructure/ (EF Core, storage, email, backup)
tests/
  AcademicPortfolio.Tests/
```

> If the professor's site is unlikely to ever grow a team, collapsing Application/Domain/Infrastructure into folders inside the Web project is fully acceptable and reduces ceremony. The plan keeps the boundaries logical either way so the choice is reversible.

## 4. Key Cross-Cutting Components

- **Localization middleware** — `RequestLocalization` resolves culture from the URL segment, sets `CultureInfo`, and drives RTL/LTR layout.
- **Authentication** — cookie-based, ASP.NET Core Identity scoped to one Super Admin account and one role.
- **File storage abstraction** — `IFileStorage` so the filesystem implementation can later be swapped for object storage (R2/B2) without touching callers.
- **SEO service** — builds titles, meta tags, canonical URLs, `hreflang`, and JSON-LD per page/culture.
- **Statistics** — lightweight middleware/event recording for page views, downloads, and video plays.
- **Backup service** — produces and restores a single archive (database + uploads + metadata manifest).

## 5. Deployment Architecture

A single Linux VM is the target. The application runs as a self-contained or framework-dependent publish, managed by `systemd`, behind a reverse proxy that terminates TLS.

```
[ Internet ] → [ Caddy / Nginx : 80,443 (auto HTTPS via Let's Encrypt) ]
                       │  reverse proxy to 127.0.0.1:5000
                       ▼
              [ Kestrel : ASP.NET Core app (systemd service) ]
                       │
          ┌────────────┴────────────┐
          ▼                         ▼
   /var/app/app.db (SQLite)   /var/app/uploads (images, pdfs)
                       │
                       ▼  nightly cron
        [ off-site backup → Cloudflare R2 / Backblaze B2 ]
```

- **Process:** `dotnet publish -c Release` → copy artifact to VM → `systemd` unit runs Kestrel on a loopback port.
- **TLS:** Caddy is recommended for zero-config automatic HTTPS; Nginx + Certbot is the alternative.
- **Static/uploads:** served by the app (or proxied static path) with correct content types and caching headers.
- **Windows/IIS alternative:** the same app runs unchanged on Windows under IIS with the ASP.NET Core Hosting Bundle (in-process). Use this only if Windows hosting is already paid for; Linux is cheaper and lower-touch.

## 6. Hosting Recommendations

| Option | Cost | Notes |
|---|---|---|
| **Oracle Cloud Always Free (Ampere A1 VM)** | $0/month | Free forever; generous storage and egress. **As of June 15, 2026 the free Ampere A1 allowance was reduced to 2 OCPU / 12 GB RAM total** — still ample for this workload. Capacity can be hard to provision in busy regions; choose a region with availability. |
| **Hetzner ARM VPS (CAX11 class)** | ~€4/month | No free tier, but predictable, fast NVMe, large traffic allowance, snapshots/backups. Best when you want "set and forget" reliability over a $0 line item. |
| **Azure App Service Free (F1)** | $0 (limited) | Convenient if staying in Azure, but constrained CPU/quota and storage handling makes the filesystem-uploads + SQLite model more awkward than on a VM. |

**Recommendation:** a single small Linux VM (Oracle Always Free if you accept its capacity/policy variability; Hetzner ~€4/mo for predictability), Kestrel behind Caddy, with nightly off-site backup to R2/B2. This keeps cost near zero, deployment to one artifact, and maintenance to one process plus one backup set.

## 7. Scalability & Reversibility Notes

- **Vertical headroom first.** A single VM handles this traffic profile comfortably; scaling means a bigger VM, not new architecture.
- **Database upgrade path.** EF Core makes a future move to PostgreSQL a provider swap plus regenerated migrations — chosen deliberately so SQLite-now is not a dead end.
- **Storage upgrade path.** The `IFileStorage` abstraction lets uploads move to object storage without code changes in services.
- **No premature distribution.** Caching, CDN in front of static assets, or a managed DB are *future, optional* steps — none are required to satisfy the requirements.
