using Polchan.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Auth;
using Polchan.Shared.Options;
using Polchan.Application.Auth.Services;
using Polchan.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Polchan.Web.Middleware;

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
            cfg.LicenseKey = builder.Configuration.GetRequiredSection("MediatR").GetRequiredSection("LicenseKey").Value;
            cfg.RegisterServicesFromAssembly(typeof(LoginUserHandler).Assembly); // Application layer
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false; // Disable mapping newer JWT claims to older claim types

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            };
        });

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokensService, TokensService>();
        services.AddScoped<IUserAccessor, UserAccessor>();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog();

        services.Configure<JwtOptions>(builder.Configuration.GetRequiredSection("Jwt"));
    }
}
