using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SwipeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SwipeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Swipe(SwipeDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (userId == dto.ToUserId)
                return BadRequest("Không thể swipe chính mình");

            var existed = await _context.Swipes
                .AnyAsync(s => s.FromUserId == userId && s.ToUserId == dto.ToUserId);

            if (existed)
                return BadRequest("Bạn đã swipe user này rồi");

            var swipe = new Swipe
            {
                FromUserId = userId,
                ToUserId = dto.ToUserId,
                IsLike = dto.IsLike
            };

            _context.Swipes.Add(swipe);

            bool isMatch = false;

            if (dto.IsLike)
            {
                var likedBack = await _context.Swipes
                    .AnyAsync(s =>
                        s.FromUserId == dto.ToUserId &&
                        s.ToUserId == userId &&
                        s.IsLike);

                if (likedBack)
                {
                    isMatch = true;

                    var match = new Match
                    {
                        User1Id = userId,
                        User2Id = dto.ToUserId
                    };

                    _context.Matches.Add(match);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = isMatch ? " It's a match!" : "Swiped successfully",
                isMatch
            });
        }
    }
}