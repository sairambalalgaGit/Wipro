using Microsoft.EntityFrameworkCore;
using SecureApp.Core.Entities;

namespace SecureApp.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<FinancialRecord> FinancialRecords => Set<FinancialRecord>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.Property(p => p.Email).HasMaxLength(256).IsRequired();
            e.HasIndex(p => p.Email).IsUnique();
            e.Property(p => p.PasswordHash).IsRequired().HasMaxLength(512);
            e.Property(p => p.FullName).IsRequired(); // AE at DB level
            e.Property(p => p.Ssn);
            e.Property(p => p.Role).HasMaxLength(64).HasDefaultValue("User");
        });

        b.Entity<FinancialRecord>(e =>
        {
            e.ToTable("FinancialRecords");
            e.Property(p => p.CardLast4).HasMaxLength(4).IsRequired();
            e.Property(p => p.CardNumber); // AE at DB level
            e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            e.Property(p => p.IntegrityTag).IsRequired().HasColumnType("varbinary(64)");
            e.Property(p => p.CreatedUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        base.OnModelCreating(b);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Minimal EF audit log (no sensitive data)
        var changes = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => new { Entity = e.Entity.GetType().Name, State = e.State.ToString(), Time = DateTime.UtcNow })
            .ToList();

        // TODO: persist to an AuditLogs table if desired (avoid sensitive fields)
        return await base.SaveChangesAsync(ct);
    }
}
