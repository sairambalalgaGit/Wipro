using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureApp.Api.Data;
using SecureApp.Api.Filters;
using SecureApp.Api.Security;
using SecureApp.Api.Services;
using SecureApp.Api.Validators; // contains RegisterRequestValidator
using SecureApp.Core.Entities;
using System.Text;

namespace SecureApp.Api.Configuration;

public static class ServiceExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddControllers(o => o.Filters.Add<NoSensitiveLoggingFilter>())
                .AddJsonOptions(o => { /* default encoder already safe */ });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SecureApp API",
                Version = "v1"
            });
         });


        // EF Core + SQL Server (TLS/AE is in the connection string)
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(cfg.GetConnectionString("AppDb")));

        // Identity hasher for PBKDF2
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        // ✅ FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        // HMAC integrity
        services.AddSingleton<IHmacService, HmacService>();

        // JWT auth
        var jwtKey = cfg["Security:JwtKey"] ?? throw new InvalidOperationException("Missing JWT key");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "SecureApp",
                    ValidAudience = "SecureApp",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("CanViewSensitive", p => p.RequireRole("Admin"));
        });

        services.AddDataProtection(); // Persist keys out of container for prod

        // Security headers (basic)
        services.AddHsts(o =>
        {
            o.MaxAge = TimeSpan.FromDays(365);
            o.IncludeSubDomains = true;
            o.Preload = true;
        });

        return services;
    }
}
