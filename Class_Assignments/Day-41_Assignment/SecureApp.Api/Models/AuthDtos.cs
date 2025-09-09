namespace SecureApp.Api.Models;

public sealed class RegisterRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Ssn { get; set; }
}

public sealed class LoginRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public sealed class AuthResponse
{
    public string Token { get; init; } = default!;
}
