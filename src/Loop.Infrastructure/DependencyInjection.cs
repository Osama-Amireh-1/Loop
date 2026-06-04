using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Communication;
using Loop.Application.Abstractions.Storage;
using Loop.Application.Interfaces;
using Loop.Infrastructure.Authentication;
using Loop.Infrastructure.Authorization;
using Loop.Infrastructure.Communication;
using Loop.Infrastructure.Database;
using Loop.Infrastructure.DomainEvents;
using Loop.Infrastructure.Repository;
using Loop.Infrastructure.Storage;
using Loop.Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Loop.SharedKernel;
using Loop.SharedKernel.UnitOfWork;

namespace Loop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        services.AddTransient<Application.Abstractions.Messaging.IDispatcher, Infrastructure.Messaging.Dispatcher>();

        services.AddHttpClient();
        services.AddScoped<Loop.Application.Abstractions.Ocr.IReceiptOcrProvider, Loop.Infrastructure.Ocr.MestalOcrProvider>();
        services.AddHttpClient<IReceiptFileStore, SupabaseReceiptFileStore>();

        services.AddHttpClient<IImageFileStore, SupabaseImageStore>();


        services.AddScoped<Loop.Application.Receipts.Services.IMerchantMatcher, Loop.Infrastructure.Receipts.MerchantMatcher.LocalMerchantMatcher>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<LoopContext>(
            options => options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<LoopContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IReadOnlyRepository<>), typeof(ReadOnlyRepository<>));

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks();

        string? connectionString = configuration.GetConnectionString("Database");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            healthChecks.AddNpgSql(connectionString);
        }

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IShopAdminContext, ShopAdminContext>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddSingleton<IStampRedemptionQrTokenProvider, StampRedemptionQrTokenProvider>();
        services.AddSingleton<IPointsRedemptionQrTokenProvider, PointsRedemptionQrTokenProvider>();
        services.AddSingleton<IOfferRedemptionQrTokenProvider, OfferRedemptionQrTokenProvider>();
        services.AddSingleton<IStampCollectionQrTokenProvider, StampCollectionQrTokenProvider>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ShopAdminOnly", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("shop_admin_id"));

            options.AddPolicy("UserOnly", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("user_id"));
        });

        services.AddScoped<PermissionProvider>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }
}


