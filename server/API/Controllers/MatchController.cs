using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DatingApp.Data;
using DatingApp.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MatchController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MatchController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyMatches()
        {
            var myId = GetUserId();
            if (myId == null) return Unauthorized();

            // 🔹 ĐÃ SỬA: Lệnh Include hướng vào đối tượng liên kết (UserOne/UserTwo) thay vì ID thô
            var matches = await _context.Matches
                .Where(x => x.User1Id == myId.Value || x.User2Id == myId.Value)
                .Include(x => x.UserOne)
                .Include(x => x.UserTwo)
                .Select(x => new
                {
                    x.Id,
                    x.CreatedAt,
                    // 🔹 ĐÃ SỬA: Trích xuất an toàn từ thực thể UserOne/UserTwo để lấy thông tin Partner
                    Partner = x.User1Id == myId.Value
                        ? new { x.UserTwo.Id, x.UserTwo.FullName, x.UserTwo.AvatarUrl, x.UserTwo.Bio }
                        : new { x.UserOne.Id, x.UserOne.FullName, x.UserOne.AvatarUrl, x.UserOne.Bio }
                })
                .ToListAsync();

            return Ok(matches);
        }

        private Guid? GetUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}