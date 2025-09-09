using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Configure strongly-typed jwt settings convenience
var jwtSection = configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key");
var jwtIssuer = jwtSection.GetValue<string>("Issuer");
var jwtAudience = jwtSection.GetValue<string>("Audience");
var jwtExpireMinutes = jwtSection.GetValue<int>("ExpireMinutes");

// Add services
builder.Services.AddControllers();

// Authentication: JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Validate signature, issuer, audience and lifetime
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30) // small clock skew
    };

    // Optional: return more meaningful messages
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            // for debugging, do not leak sensitive info in production
            return Task.CompletedTask;
        }
    };
});

// Authorization
builder.Services.AddAuthorization();

// Cookie policy (ensures cookies are Secure and HttpOnly)
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

var app = builder.Build();

// Force HTTPS redirection
app.UseHttpsRedirection();

// Use cookie policy
app.UseCookiePolicy();

// Enable authentication & authorization middlewares (order matters)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
