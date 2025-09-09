using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SecureApp.Api.Models;
using SecureApp.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecureApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUserService users, IConfiguration cfg, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest dto, CancellationToken ct)
    {
        // Validate via FluentValidation (auto-registered)
        var existing = await users.GetByEmailAsync(dto.Email, ct);
        if (existing is not null) return Conflict("Email already registered");

        var id = await users.CreateAsync(dto.Email, dto.Password, dto.FullName, dto.Ssn, ct);
        logger.LogInformation("Registered user {UserId}", id);
        return CreatedAtAction(nameof(Register), new { id }, new { id });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest dto, CancellationToken ct)
    {
        if (!await users.ValidateCredentialsAsync(dto.Email, dto.Password, ct))
            return Unauthorized();

        var token = IssueJwt(dto.Email);
        return Ok(new AuthResponse { Token = token });
    }

    private string IssueJwt(string email)
    {
        var key = cfg["Security:JwtKey"] ?? throw new InvalidOperationException("Missing JWT key");
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(ClaimTypes.Name, email), new Claim(ClaimTypes.Role, "User") };

        var jwt = new JwtSecurityToken(
            issuer: "SecureApp",
            audience: "SecureApp",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
