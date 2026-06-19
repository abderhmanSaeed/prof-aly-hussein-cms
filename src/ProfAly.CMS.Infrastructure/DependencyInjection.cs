using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProfAly.CMS.Application.Abstractions;
using ProfAly.CMS.Infrastructure.Identity;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Infrastructure.Persistence.Interceptors;
using ProfAly.CMS.Infrastructure.Persistence.Seeding;
using ProfAly.CMS.Infrastructure.Storage;

namespace ProfAly.CMS.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer: EF Core (SQLite), ASP.NET Core
/// Identity (single Super Admin), and the file-storage provider.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddIdentity(services);
        AddFileStorage(services, configuration);
        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=App_Data/app.db";

        services.AddDbContext<AppDbContext>(options => options
            .UseSqlite(connectionString)
            .AddInterceptors(new SqlitePragmaInterceptor()));

        // Seed infrastructure (scaffolding only — no seeders registered yet).
        services.AddScoped<DatabaseInitializer>();
    }

    private static void AddIdentity(IServiceCollection services)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Strong password policy + lockout (doc 06 §7 / doc 13 Phase 2).
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Cookie endpoints (login/logout pages are built in Stage 4).
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/account/login";
            options.LogoutPath = "/account/logout";
            options.AccessDeniedPath = "/account/denied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });
    }

    private static void AddFileStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.AddSingleton<IFileStorage, LocalFileStorage>();
    }
}
