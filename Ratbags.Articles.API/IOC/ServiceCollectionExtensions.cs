using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Repositories;
using Ratbags.Articles.API.Services;

namespace Ratbags.Articles.API.IOC;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IArticlesService, ArticlesService>();
        services.AddScoped<IArticlesRepository, ArticlesRepository>();

        return services;
    }
}
