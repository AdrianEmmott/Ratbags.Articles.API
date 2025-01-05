using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ratbags.Articles.API.Models;
using System.Text;

namespace Ratbags.Articles.API.ServiceExtensions;

public static class AuthenticationServiceExtension
{
    public static IServiceCollection AddAuthenticationServiceExtension(this IServiceCollection services, AppSettings settings)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = settings.JWT.Issuer,
                ValidAudience = settings.JWT.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JWT.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
