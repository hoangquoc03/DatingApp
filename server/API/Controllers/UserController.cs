using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DatingApp.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id.ToString() == userId);

            if (user == null)
                return NotFound();

            return Ok(user);
     
        }
        [HttpGet("discover")]
        public async Task<IActionResult> Discover()
        {
            var myId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var swipedIds = await _context.Swipes
                .Where(x => x.FromUserId == myId)
                .Select(x => x.ToUserId)
                .ToListAsync();

            var users = await _context.Users
                .Where(x => x.Id != myId && !swipedIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.Bio,
                    x.AvatarUrl,
                    x.Location
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}