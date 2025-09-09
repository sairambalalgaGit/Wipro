namespace SecureApp.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FullName { get; set; } = default!; // AE in DB
    public string? Ssn { get; set; } // AE in DB
    public string Role { get; set; } = "User"; // RBAC
}
