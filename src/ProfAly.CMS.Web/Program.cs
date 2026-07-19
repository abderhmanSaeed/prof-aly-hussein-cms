using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using ProfAly.CMS.Application;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Web;
using ProfAly.CMS.Infrastructure;
using ProfAly.CMS.Infrastructure.Identity;
using ProfAly.CMS.Infrastructure.Persistence.Seeding;
using ProfAly.CMS.Web.Infrastructure.Logging;
using ProfAly.CMS.Web.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Logging (doc 15, task 9). Providers (Console/Debug/EventSource) and filters
// come from the host defaults and the "Logging" section in appsettings.*.json.
// A rolling file provider writes production logs to Logs/yyyy-MM.log (never
// records passwords or secrets — only the messages the app chooses to emit).
// ---------------------------------------------------------------------------
builder.Logging.AddConsole();
builder.Logging.AddFileLogger(builder.Environment, builder.Configuration);

// ---------------------------------------------------------------------------
// Layer composition (DI — doc 15, task 8).
// ---------------------------------------------------------------------------
builder.Services.AddInfrastructure(builder.Configuration); // EF/SQLite + Identity + IFileStorage
builder.Services.AddApplication();                         // application services (added later)

// ---------------------------------------------------------------------------
// Localization infrastructure (AR/EN/FR — doc 10). Content localization uses
// translation tables (Stage 2+); this configures UI-string + request culture.
// ---------------------------------------------------------------------------
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Emit Arabic/non-Latin text as raw UTF-8 (not numeric HTML entities), matching the
// static site and improving SEO/readability.
builder.Services.AddSingleton<System.Text.Encodings.Web.HtmlEncoder>(
    System.Text.Encodings.Web.HtmlEncoder.Create(System.Text.Unicode.UnicodeRanges.All));

var supportedCultures = SupportedCultures.All
    .Select(c => new CultureInfo(c))
    .ToList();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(SupportedCultures.Default);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;

    // URL-segment ("/{culture}/...") routing is wired in a later stage; the
    // route-data provider is registered first so it takes effect automatically then.
    options.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider { RouteDataStringKey = "culture" });
});

// ---------------------------------------------------------------------------
// Razor Pages + Areas/Admin protection (doc 06 §7). The admin area is gated by
// a single policy at the folder level — no admin pages exist yet.
// ---------------------------------------------------------------------------
builder.Services.AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource)))
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AuthorizeAreaFolder("Admin", "/", Policies.RequireSuperAdmin);
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.RequireSuperAdmin, policy => policy.RequireRole(Roles.SuperAdmin));

// Health checks (database/uploads/backups checks are registered in AddInfrastructure).

// ---------------------------------------------------------------------------
// Production hardening infrastructure (Stage 90).
// ---------------------------------------------------------------------------
var isProduction = !builder.Environment.IsDevelopment();

// HSTS options (used only outside Development).
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

// Optional explicit HTTPS port for the redirect (e.g. behind a reverse proxy in
// production: set HttpsRedirection:HttpsPort=443). When unset, ASP.NET Core infers it
// from the request / server addresses / ASPNETCORE_HTTPS_PORT — preserving the existing
// dev launch behaviour.
var httpsPort = builder.Configuration.GetValue<int?>("HttpsRedirection:HttpsPort");
if (httpsPort is > 0)
{
    builder.Services.AddHttpsRedirection(options => options.HttpsPort = httpsPort.Value);
}

// Response compression (Brotli + Gzip) — enabled for HTTPS responses.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json", "image/svg+xml", "application/manifest+json",
    });
});
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);

// Secure cookie policy — HttpOnly + SameSite=Lax + Secure (Always in production).
var cookieSecurePolicy = isProduction
    ? Microsoft.AspNetCore.Http.CookieSecurePolicy.Always
    : Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;

builder.Services.Configure<Microsoft.AspNetCore.Builder.CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = cookieSecurePolicy;
});

// Harden the Identity authentication cookie (paths/expiry set in AddInfrastructure).
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "ProfAly.Admin.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
});

// Harden the anti-forgery cookie (anti-forgery itself is on by default for Razor Pages).
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "ProfAly.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
});

// Upload hardening — cap request body / multipart size just above the largest allowed
// document (FileStorage:MaxDocumentBytes = 50 MB) so oversized uploads are rejected early.
var maxUploadBytes = builder.Configuration.GetValue<long>("FileStorage:MaxDocumentBytes", 52_428_800L);
var maxRequestBytes = maxUploadBytes + 8L * 1024 * 1024; // headroom for multipart boundaries/other fields
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxRequestBytes;
});
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = maxRequestBytes;
    options.AddServerHeader = false; // don't advertise "Server: Kestrel"
});

// Rate limiting — a lenient global per-IP fixed window (defence against floods) plus a
// stricter "login" policy applied to the sign-in page (brute-force mitigation on top of
// the existing Identity account-lockout).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(context =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:GlobalPermitLimit", 240),
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));

    options.AddPolicy("login", context =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:LoginPermitLimit", 10),
                Window = TimeSpan.FromMinutes(5),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// Database initialization pipeline (Stage 3): ensure App_Data exists, apply
// migrations (create on first run), validate connectivity, run seeders.
// ---------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.RunAsync();
}

// ---------------------------------------------------------------------------
// HTTP pipeline.
// ---------------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Defensive response headers + CSP on every response (including static files below).
app.UseSecurityHeaders();

// Compress responses (Brotli/Gzip) before they are written by the static-file/endpoint
// middleware further down the pipeline.
app.UseResponseCompression();

// Cache-Control for static assets. asp-append-version fingerprints CSS/JS so a long
// immutable cache is safe; content served from wwwroot.
const string staticCacheControl = "public,max-age=604800"; // 7 days
app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
{
    OnPrepareResponse = ctx =>
        ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] = staticCacheControl,
});

// Serve uploaded media (images/PDFs) from the uploads root at /uploads (doc 09 §6).
// Only the uploads subtree is exposed — the SQLite DB in App_Data stays private.
var uploadsRoot = Path.GetFullPath(builder.Configuration["FileStorage:RootPath"] ?? "App_Data/uploads");
Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
        ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] = staticCacheControl,
});

app.UseRouting();

// Apply rate limiting to routed endpoints (static files above are not rate limited).
app.UseRateLimiter();

app.UseRequestLocalization();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Health endpoint: application running + SQLite + upload folder + backup folder. Emits a
// small JSON document listing each check so operators can see which subsystem failed.
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
            }),
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(payload));
    },
});

app.Run();
