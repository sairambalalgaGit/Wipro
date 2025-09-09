using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecureApp.Api.Data;
using SecureApp.Core.Entities;

namespace SecureApp.Api.Services;

public interface IUserService
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<int> CreateAsync(string email, string password, string fullName, string? ssn, CancellationToken ct = default);
    Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);
}

public class UserService(AppDbContext db, IPasswordHasher<User> hasher, ILogger<UserService> logger) : IUserService
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<int> CreateAsync(string email, string password, string fullName, string? ssn, CancellationToken ct = default)
    {
        var user = new User { Email = email, FullName = fullName, Ssn = ssn };
        user.PasswordHash = hasher.HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("User created with Id {UserId}", user.Id); // DO NOT log email/PII
        return user.Id;
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null) return false;
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
