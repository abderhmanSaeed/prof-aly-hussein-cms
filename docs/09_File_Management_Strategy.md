# 09 — File Management Strategy

Files are stored on the **local filesystem**; the database holds only metadata (`MediaFile`). Video is never stored (YouTube only). The storage layer is behind an `IFileStorage` abstraction so it can later move to object storage (Cloudflare R2 / Backblaze B2) without changing callers.

---

## 1. File Types Handled

| Kind | Allowed types | Max size | Purpose |
|---|---|---|---|
| **Image** | jpg, jpeg, png, webp | 5 MB | covers, profile photo, logo, custom video thumbnails |
| **Thumbnail** | generated jpg/webp | — | auto-derived from images |
| **PDF** | pdf | 25 MB | books, publications, research, theses, resources, enrichment |

> Word/PowerPoint were listed as "allowed" in the original requirements but are **out of scope for v1** to keep preview/serving simple. PDFs cover the academic need and preview natively in-browser. Office formats can be added later behind the same abstraction if required (download-only, no inline preview).

**Validation (every upload):**
- Check **extension allowlist** AND **content-type/magic-byte sniffing** (don't trust the client MIME).
- Enforce per-kind size limit (also enforce `MaxRequestBodySize`/multipart limits in Kestrel).
- Reject anything else, including HTML/SVG (SVG can carry script) and executables.
- Strip/ignore client path information; never use the original name as the stored path.

## 2. Storage Structure

```
/var/app/uploads/
  images/
    YYYY/MM/{guid}.{ext}
  thumbnails/
    YYYY/MM/{guid}_thumb.{ext}
  pdfs/
    YYYY/MM/{guid}.pdf
  _temp/            (in-progress uploads, swept periodically)
```

- **Date-partitioned** (`YYYY/MM`) to keep any single directory from growing unbounded.
- **Uploads root is outside the web app's content root** (or the folder is served through a controlled handler), so files cannot be executed as code and access can be governed.
- Permissions: the app's service account owns the folder; least privilege.

## 3. Naming Conventions

- **Stored name:** `{guid}.{ext}` (lowercase). GUID guarantees uniqueness, prevents collisions, and avoids leaking the original filename or enabling enumeration.
- **Original name:** preserved in `MediaFile.OriginalFileName` for display/download (sanitized: strip control chars, limit length, no directory separators).
- **Download served as** the original name via `Content-Disposition` while the physical file stays GUID-named.
- **Thumbnails:** `{sourceGuid}_thumb.{ext}`.
- **No PII or Arabic-only names on disk** — display names can be Arabic; physical names are GUIDs to avoid filesystem/encoding issues across OSes.

## 4. Image Handling

- On upload: validate, store original, generate a resized **thumbnail** and optionally a web-optimized variant (e.g. max 1600px, webp) to keep pages fast.
- Capture `Width`/`Height` for layout and responsive `srcset`.
- Require/encourage `AltText` for accessibility and SEO.

## 5. PDF Handling

- Stored as-is after validation; size-limited.
- **Preview:** rendered in-browser (PDF.js or native `<embed>`/`<iframe>`) inside the book/resource popup — no server-side conversion.
- **Download:** streamed with `Content-Disposition: attachment; filename="{original}"`; increments `DownloadCount`.
- **Read/inline:** `inline` disposition for the "Read" action.

## 6. Serving Strategy

- Public files served with long-lived cache headers (`Cache-Control: public, max-age=...`) since GUID names are immutable.
- Range requests supported for PDFs (smooth in-browser preview).
- Optional: front the uploads path with the reverse proxy for efficient static serving.
- Access control: covers/PDFs are public by design; there is no private-file requirement in scope.

## 7. Referential Integrity & Cleanup

- `MediaFile` is referenced by FK (`CoverImageId`, `PdfFileId`, `Profile.PhotoMediaId`, `SiteSettings.LogoMediaId`).
- **Deleting a referenced file** warns the admin and sets references to null (no orphaned content rows).
- **Orphan sweep:** a periodic job (or admin action) lists `MediaFile` rows/disk files with no references for safe cleanup.
- **Media Library** UI shows usage ("used by 2 items") before deletion.

## 8. Backup Implications

- The entire uploads tree is part of the backup set (see Backup & Restore). Because metadata lives in SQLite and bytes live on disk, a consistent backup = snapshot of `app.db` + `uploads/`. The manifest records the pairing so restore re-links cleanly.

## 9. Security Summary
- Allowlist + content sniffing; reject SVG/HTML/executables.
- GUID storage names; sanitized display names; no path traversal.
- Size limits at app and server level.
- Files stored outside executable web roots.
- Rich-text bodies (which may embed image URLs) are sanitized separately (see Entities).
