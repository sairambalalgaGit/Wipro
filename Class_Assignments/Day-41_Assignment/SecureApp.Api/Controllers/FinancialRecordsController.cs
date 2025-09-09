using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureApp.Api.Data;
using SecureApp.Api.Security;
using SecureApp.Core.Entities;
using System.Security.Claims;

namespace SecureApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Enforce auth
public class FinancialRecordsController(AppDbContext db, IHmacService hmac, ILogger<FinancialRecordsController> logger) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "User,Admin")] // RBAC
    public async Task<IActionResult> Create([FromBody] CreateFinancialRecord dto, CancellationToken ct)
    {
        var email = User.FindFirstValue(ClaimTypes.Name)!;
        var user = await db.Users.SingleAsync(u => u.Email == email, ct);

        var entity = new FinancialRecord
        {
            UserId = user.Id,
            CardLast4 = dto.CardLast4,
            CardNumber = dto.CardNumber, // AE in DB handles encryption
            Amount = dto.Amount
        };
        entity.IntegrityTag = hmac.Compute(entity.UserId, entity.CardLast4, entity.Amount, entity.CreatedUtc);

        db.FinancialRecords.Add(entity);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("FinancialRecord created Id {Id} for UserId {UserId}", entity.Id, entity.UserId);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new { entity.Id });
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<ActionResult<FinancialRecordDto>> GetById(int id, CancellationToken ct)
    {
        var entity = await db.FinancialRecords.Include(f => f.User).FirstOrDefaultAsync(f => f.Id == id, ct);
        if (entity is null) return NotFound();

        var ok = hmac.Verify(entity.IntegrityTag, entity.UserId, entity.CardLast4, entity.Amount, entity.CreatedUtc);
        if (!ok) return StatusCode(409, "Integrity check failed");

        return new FinancialRecordDto
        {
            Id = entity.Id,
            Amount = entity.Amount,
            CardLast4 = entity.CardLast4,
            CreatedUtc = entity.CreatedUtc
            // CardNumber intentionally omitted from API response
        };
    }
}

public sealed class CreateFinancialRecord
{
    public string CardLast4 { get; set; } = default!;
    public string? CardNumber { get; set; }
    public decimal Amount { get; set; }
}

public sealed class FinancialRecordDto
{
    public int Id { get; set; }
    public string CardLast4 { get; set; } = default!;
    public decimal Amount { get; set; }
    public DateTime CreatedUtc { get; set; }
}
