using System.Security.Cryptography;
using System.Text;

namespace SecureApp.Api.Security;

public interface IHmacService
{
    byte[] Compute(int userId, string cardLast4, decimal amount, DateTime createdUtc);
    bool Verify(byte[] tag, int userId, string cardLast4, decimal amount, DateTime createdUtc);
}

public class HmacService : IHmacService
{
    private readonly byte[] _key;

    public HmacService(IConfiguration cfg)
    {
        // Load HMAC key from Key Vault or environment/secret store
        var base64 = cfg["Security:HmacKey"];
        if (string.IsNullOrWhiteSpace(base64))
            throw new InvalidOperationException("Missing HMAC key.");
        _key = Convert.FromBase64String(base64);
    }

    public byte[] Compute(int userId, string cardLast4, decimal amount, DateTime createdUtc)
    {
        var payload = $"{userId}|{cardLast4}|{amount}|{createdUtc:O}";
        using var hmac = new HMACSHA256(_key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
    }

    public bool Verify(byte[] tag, int userId, string cardLast4, decimal amount, DateTime createdUtc)
    {
        var computed = Compute(userId, cardLast4, amount, createdUtc);
        return CryptographicOperations.FixedTimeEquals(computed, tag);
    }
}
