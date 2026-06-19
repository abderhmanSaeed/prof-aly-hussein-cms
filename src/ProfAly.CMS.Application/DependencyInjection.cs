using Microsoft.Extensions.DependencyInjection;

namespace ProfAly.CMS.Application;

/// <summary>
/// Composition root for the Application layer. Use-case services are registered
/// here from Stage 5; the skeleton wires the layer so the contract is in place.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application services (content, media, SEO, statistics, backup) are added in later stages.
        return services;
    }
}
