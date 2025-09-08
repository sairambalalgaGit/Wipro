using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAPlaceAPI.Data;
using RentAPlaceAPI.Models;
using System.Security.Claims;
using RentAPlaceAPI.Services;

namespace RentAPlaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservationsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Reserve a property
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Reserve(
            [FromBody] ReservationDto dto,
            [FromServices] EmailService emailService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (userId == 0)
                    return Unauthorized(new { message = "User not logged in" });

                var property = await _context.Properties
                    .Include(p => p.Owner)
                    .FirstOrDefaultAsync(p => p.PropertyId == dto.PropertyId);

                if (property == null)
                    return BadRequest(new { message = "Invalid PropertyId" });

                if (dto.CheckIn >= dto.CheckOut)
                    return BadRequest(new { message = "Check-out date must be after Check-in" });

                var reservation = new Reservation
                {
                    UserId = userId,
                    PropertyId = dto.PropertyId,
                    CheckIn = dto.CheckIn,
                    CheckOut = dto.CheckOut,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                // ✅ Email to property owner
                if (!string.IsNullOrEmpty(property.Owner.Email))
                {
                    string subject = $"New Reservation Request for {property.Title}";
                    string body = $@"
                        <h3>Hello {property.Owner.Name},</h3>
                        <p>A new reservation has been made for your property <b>{property.Title}</b>.</p>
                        <p><b>Check-In:</b> {dto.CheckIn:yyyy-MM-dd}</p>
                        <p><b>Check-Out:</b> {dto.CheckOut:yyyy-MM-dd}</p>
                        <p>Status: Pending</p>
                        <br>
                        <p>Regards,<br/>RentAPlace Team</p>";

                    await emailService.SendEmailAsync(property.Owner.Email, subject, body);
                }

                return Ok(new { message = "Reservation created successfully!", reservation });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Reservation Save Error: " + ex);
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        // ✅ Get logged-in user's reservations (with paging)
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> MyReservations(int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (userId == 0)
                    return Unauthorized(new { message = "User not logged in" });

                var reservations = await _context.Reservations
                    .Include(r => r.Property)
                    .Where(r => r.UserId == userId)
                    .OrderBy(r => r.CreatedAt)                // ✅ fixes EF warning
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        // ✅ Get reservations for owner (with paging)
        [HttpGet("owner")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetReservationsForOwner(int page = 1, int pageSize = 10)
        {
            var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var reservations = await _context.Reservations
                .Include(r => r.Property)
                .Include(r => r.User)
                .Where(r => r.Property.OwnerId == ownerId)
                .OrderBy(r => r.CreatedAt)                  // ✅ fixes EF warning
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(reservations);
        }

        // ✅ Update reservation status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> UpdateReservationStatus(int id, [FromBody] string newStatus)
        {
            var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var reservation = await _context.Reservations
                .Include(r => r.Property)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            if (reservation.Property.OwnerId != ownerId && !User.IsInRole("Admin"))
                return Forbid();

            reservation.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reservation status updated", reservation });
        }
    }
}
