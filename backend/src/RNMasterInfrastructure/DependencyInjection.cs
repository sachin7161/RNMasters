
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using RNMasterApplication.Authentication.Interfaces;
using RNMasterInfrastructure.Identity;
using RNMasterInfrastructure.Persistence;
using RNMasterInfrastructure.Services;

namespace RNMasterInfrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // --------------------------------------------
        // Database Connection
        // --------------------------------------------

        var connectionString =
            Environment.GetEnvironmentVariable("DB_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "DB_CONNECTION is not configured.");
        }

        services.AddDbContext<RNMasterDbContext>(options =>
        {
            options.UseSqlServer(connectionString);

            options.ConfigureWarnings(warnings =>
                warnings.Ignore(
                    RelationalEventId.PendingModelChangesWarning));
        });

        // --------------------------------------------
        // ASP.NET Core Identity
        // --------------------------------------------

        services
            .AddIdentityCore<AppUser>()
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<RNMasterDbContext>();

        // --------------------------------------------
        // Authentication Services
        // --------------------------------------------

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
       services.AddScoped<DatabaseScriptService>();

        // --------------------------------------------
        // Database Services
        // --------------------------------------------

        services.AddScoped<DatabaseScriptService>();

        // --------------------------------------------
        // JWT Configuration
        // --------------------------------------------

        var jwtSecret =
            Environment.GetEnvironmentVariable("JWT_SECRET");

        var jwtIssuer =
            Environment.GetEnvironmentVariable("JWT_ISSUER");

        var jwtAudience =
            Environment.GetEnvironmentVariable("JWT_AUDIENCE");

        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            throw new InvalidOperationException(
                "JWT_SECRET is not configured.");
        }

        if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
        {
            throw new InvalidOperationException(
                "JWT_SECRET must be at least 32 bytes long.");
        }

        if (string.IsNullOrWhiteSpace(jwtIssuer) ||
            string.IsNullOrWhiteSpace(jwtAudience))
        {
            throw new InvalidOperationException(
                "JWT_ISSUER and JWT_AUDIENCE must be configured.");
        }

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwtSecret)),

                        ClockSkew = TimeSpan.Zero
                    };
            });

        services.AddAuthorization();

        return services;
    }
}
