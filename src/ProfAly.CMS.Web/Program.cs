using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using ProfAly.CMS.Application;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Infrastructure;
using ProfAly.CMS.Infrastructure.Identity;
using ProfAly.CMS.Infrastructure.Persistence.Seeding;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Logging (doc 15, task 9). Providers (Console/Debug/EventSource) and filters
// come from the host defaults and the "Logging" section in appsettings.*.json.
// ---------------------------------------------------------------------------
builder.Logging.AddConsole();

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
    .AddDataAnnotationsLocalization()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AuthorizeAreaFolder("Admin", "/", Policies.RequireSuperAdmin);
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.RequireSuperAdmin, policy => policy.RequireRole(Roles.SuperAdmin));

// Health checks (the "database" check is registered in AddInfrastructure).

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
app.UseStaticFiles();

app.UseRouting();

app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHealthChecks("/health");

app.Run();
