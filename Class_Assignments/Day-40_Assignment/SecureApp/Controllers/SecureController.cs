using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SecureApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecureController : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnly()
        {
            return Ok(new { message = "Hello Admin! You have full access." });
        }

        [Authorize(Roles = "User")]
        [HttpGet("user")]
        public IActionResult UserOnly()
        {
            return Ok(new { message = "Hello User! You have limited access." });
        }

        [Authorize]
        [HttpGet("general")]
        public IActionResult AnyAuthenticated()
        {
            var name = User.Identity?.Name ?? "unknown";
            return Ok(new { message = $"Hello {name}, you are authenticated." });
        }
    }
}
