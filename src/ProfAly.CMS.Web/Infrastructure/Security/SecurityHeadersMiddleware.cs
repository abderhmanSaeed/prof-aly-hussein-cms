namespace ProfAly.CMS.Web.Infrastructure.Security;

/// <summary>
/// Adds the standard defensive HTTP response headers to every response, including a
/// Content-Security-Policy. The policy is intentionally compatible with the existing
/// UI (local Bootstrap/jQuery bundles, a small inline theme-bootstrap script, inline
/// styles, Google Fonts, and YouTube-nocookie video embeds) so hardening does not break
/// any current page. See <c>docs/90_Production_Hardening_Report.md</c> for the roadmap
/// to a nonce-based policy that drops <c>'unsafe-inline'</c> for scripts.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _csp;

    private static readonly string DefaultCsp = string.Join("; ", new[]
    {
        "default-src 'self'",
        "base-uri 'self'",
        "object-src 'none'",
        "frame-ancestors 'self'",
        "form-action 'self'",
        // Local bundles + a couple of tiny inline bootstrap scripts (theme, layout).
        "script-src 'self' 'unsafe-inline'",
        // Inline style attributes are used across admin/public views; Google Fonts CSS.
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
        "font-src 'self' https://fonts.gstatic.com",
        // Uploaded images are served from /uploads (same origin); data: for small inline assets.
        "img-src 'self' data:",
        "connect-src 'self'",
        // YouTube embeds on video/event detail pages use the privacy-enhanced host.
        "frame-src https://www.youtube-nocookie.com https://www.youtube.com",
        "media-src 'self'",
        "manifest-src 'self'",
        "upgrade-insecure-requests",
    });

    public SecurityHeadersMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _csp = configuration["Security:ContentSecurityPolicy"] ?? DefaultCsp;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Only set if absent so a downstream component can override intentionally.
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "SAMEORIGIN";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=(), interest-cohort=()";
        headers["Content-Security-Policy"] = _csp;
        headers["X-Permitted-Cross-Domain-Policies"] = "none";

        // Drop the X-Powered-By fingerprint if a downstream component set it. (The
        // Kestrel "Server" header is suppressed via KestrelServerOptions.AddServerHeader.)
        headers.Remove("X-Powered-By");

        return _next(context);
    }
}

/// <summary>Pipeline registration helper.</summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityHeadersMiddleware>();
}
