using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicStream.Application.Configuration;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MusicStream.Core.Entities;
using Microsoft.AspNetCore.Identity;
using MusicStream.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MusicStream.Application.Interfaces;
using MusicStream.Infrastructure.Identity;

namespace MusicStream.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddJwtAuthentication(configuration);
        services.AddIdentityServices();
        services.AddAuthorization();

        // Used to detect all use cases services
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(MusicStream.Application.UseCases.CreateUserCommandHandler).Assembly)
            );
    }

    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authSettings = configuration.GetSection("Authentication").Get<AuthenticationSettings>();

        if (authSettings is null)
        {
            throw new Exception("No auth settings were provided in \"appsettings.json\"");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(jwtOptions =>
        {
            jwtOptions.TokenValidationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authSettings.Issuer,
                ValidAudience = authSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(authSettings.TokenSecret)
                )
            };
        });

        services.AddSingleton<ITokenProvider, TokenProviderHelper>();
    }

    private static void AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;

            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();
    }
}

