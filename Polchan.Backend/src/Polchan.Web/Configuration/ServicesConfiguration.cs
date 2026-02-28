using Polchan.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Auth;
using Polchan.Shared.Options;
using Polchan.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Polchan.Web.Middleware;
using Polchan.Application.Interfaces;
using Polchan.Infrastructure.Storage.Services;
using Polchan.Infrastructure.Auth.Services;
using Polchan.Core.Posts.Services;
using Polchan.Core.Interfaces;

namespace Polchan.Web.Configuration;

public static class ServicesConfiguration
{
    public static void AddServices(this IServiceCollection services, WebApplicationBuilder builder)
    {   
        if (builder.Environment.IsProduction())
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
        }
        
        services.AddValidation();
        services.AddOpenApi();
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddLogging();
        
        services.AddDbContext<PolchanDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("PolchanDB"));
        });

        services.AddMediatR(cfg =>
        {
            cfg.LicenseKey = builder.Configuration.GetRequiredSection("MediatR:LicenseKey").Value;
            cfg.RegisterServicesFromAssembly(typeof(LoginUserHandler).Assembly); // Application layer
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false; // Disable mapping newer JWT claims to older claim types

            var issuer = builder.Configuration.GetRequiredSection("Jwt:Issuer").Value;
            var audience = builder.Configuration.GetRequiredSection("Jwt:Audience").Value;
            var secret = builder.Configuration.GetRequiredSection("Jwt:Secret").Value;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!)),
            };
        });

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPolchanDbContext, PolchanDbContext>();
        services.AddScoped<IStorageService, LocalStorageService>();
        services.AddScoped<ITokensService, TokensService>();
        services.AddScoped<IUserAccessor, UserAccessor>();

        // Domain services
        services.AddScoped<PostReactionService>();
        services.AddScoped<CommentReactionService>();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog();

        services.Configure<JwtOptions>(builder.Configuration.GetRequiredSection("Jwt"));
        services.Configure<StorageOptions>(builder.Configuration.GetRequiredSection("Storage"));
    }
}
