using SecureApp.Api.Configuration;
using Microsoft.OpenApi.Models; 

var builder = WebApplication.CreateBuilder(args);

// Do not bind or log secrets accidentally
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddAppServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SecureApp API v1");
        c.RoutePrefix = string.Empty; // Swagger at root (optional)
    });
}

app.UseHttpsRedirection();

// Content-Security-Policy for any future Razor/static responses
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["X-XSS-Protection"] = "0";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    ctx.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=()";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
