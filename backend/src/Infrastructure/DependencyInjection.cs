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
using Amazon.S3;
using Amazon.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;
using MediatR;
using MusicStream.Application.UseCases;

namespace MusicStream.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthenticationSettings>(configuration.GetSection("Authentication"));
        services.Configure<S3Settings>(configuration.GetSection("AWS"));

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "MusicStream_";
        });

        services.AddJwtAuthentication(configuration);

        // Used to detect all use cases services
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(MusicStream.Application.UseCases.CreateUserCommandHandler).Assembly)
            );

        services.AddRepositoriesServices();

        services.AddAWSServices(configuration);
    }

    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authSettings = configuration.GetSection("Authentication").Get<AuthenticationSettings>();

        if (authSettings is null)
        {
            throw new Exception("No auth settings were provided in \"appsettings.json\"");
        }
        services.AddIdentityServices();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<AuthenticationSettings>>((jwtOptions, authOptions) =>
            {
                var settings = authOptions.Value;
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.TokenSecret))
                };
            });

        services.AddSingleton<ITokenProvider, TokenProviderHelper>();
        services.AddAuthorization();
    }

    private static void AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentityCore<User>(options =>
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

    private static void AddRepositoriesServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISongRepository, SongRepository>();
    }

    private static void AddAWSServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<S3Settings>(
            configuration.GetSection("AWS"));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var s3Settings = configuration.GetSection("AWS").Get<S3Settings>();
            if (s3Settings is null) throw new Exception("No aws settings were provided in \"appsettings.json\"");


            var credentials = new BasicAWSCredentials(s3Settings.AccessKey, s3Settings.SecretKey);
            var s3Config = new AmazonS3Config
            {
                ServiceURL = s3Settings.ServiceUrl,
                ForcePathStyle = s3Settings.ForcePathStyle
            };

            return new AmazonS3Client(credentials, s3Config);
        });
    }

    public static async Task EnsureS3BucketCreatedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();

        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

        var logger = loggerFactory.CreateLogger(nameof(DependencyInjection));

        var storageOptions = scope.ServiceProvider.GetRequiredService<IOptions<S3Settings>>().Value;
        var bucketName = storageOptions.BucketName;
        logger.LogInformation($"Check if the \"{bucketName}\" bucket exists");

        try
        {
            bool exists = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);

            if (!exists)
            {
                await s3Client.PutBucketAsync(bucketName);
                logger.LogInformation("Bucket-ul S3/MinIO '{BucketName}' created.", bucketName);
            }
            else logger.LogInformation($"The \"{bucketName}\" bucket exists");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while verifying or creating the MinIO/S3 bucket '{BucketName}'.", bucketName);
        }
    }

    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DatabaseSeeding");

        var request = new SeedFirstAdminCommand();
        var userCredentials = await mediator.Send(request);

        if (userCredentials is not null)
        {
            logger.LogWarning(
                "==========================================================\n" +
                "INITIAL ADMIN CREATED SUCCESSFULLY!\n" +
                "Username: {Username}\n" +
                "Temporary Password: {Password}\n" +
                "PLEASE CHANGE THIS PASSWORD IMMEDIATELY AFTER FIRST LOGIN!\n" +
                "==========================================================",
                userCredentials.Username,
                userCredentials.Password);
        }
    }
}

