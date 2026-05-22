using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatingApp.Data;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MatchesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MatchesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMatches()
        {
            var myId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var myLikes = await _context.Swipes
                .Where(x => x.FromUserId == myId && x.IsLike)
                .Select(x => x.ToUserId)
                .ToListAsync();

            var likedMe = await _context.Swipes
                .Where(x => x.ToUserId == myId && x.IsLike)
                .Select(x => x.FromUserId)
                .ToListAsync();

            var matchIds = myLikes.Intersect(likedMe).ToList();

            var matches = await _context.Users
                .Where(x => matchIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.AvatarUrl,
                    x.Location
                })
                .ToListAsync();

            return Ok(matches);
        }
    }
}