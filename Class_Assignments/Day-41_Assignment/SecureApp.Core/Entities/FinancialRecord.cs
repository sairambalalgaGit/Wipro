namespace SecureApp.Core.Entities;

public class FinancialRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public string CardLast4 { get; set; } = default!;
    public string? CardNumber { get; set; } // AE in DB
    public decimal Amount { get; set; }

    // Integrity (HMAC) over (UserId|CardLast4|Amount|CreatedUtc)
    public byte[] IntegrityTag { get; set; } = Array.Empty<byte>();
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
