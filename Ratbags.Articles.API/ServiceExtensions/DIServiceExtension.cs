﻿using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Repositories;
using Ratbags.Articles.API.Services;

namespace Ratbags.Articles.API.ServiceExtensions;

public static class DIServiceExtension
{
    public static IServiceCollection AddDIServiceExtension(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IArticlesService, ArticlesService>();
        services.AddScoped<IArticlesRepository, ArticlesRepository>();

        return services;
    }
}
