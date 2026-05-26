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
        public async Task<IActionResult> Swipe([FromBody] SwipeDto dto)
        {
            var myId = GetUserId();
            if (myId == null) return Unauthorized();

            // 1. Tạo bản ghi lượt quẹt mới
            var swipe = new Swipe
            {
                Id = Guid.NewGuid(),
                FromUserId = myId.Value,
                ToUserId = dto.ToUserId,
                IsLike = dto.IsLike,
                CreatedAt = DateTime.UtcNow
            };
            _context.Swipes.Add(swipe);

            // 2. Logic so khớp tự động (Match Match)
            if (dto.IsLike)
            {
                // Kiểm tra xem đối phương trước đó đã từng quẹt THÍCH mình chưa
                var partnerLikedMe = await _context.Swipes
                    .AnyAsync(x => x.FromUserId == dto.ToUserId && x.ToUserId == myId.Value && x.IsLike);

                if (partnerLikedMe)
                {
                    // ĐÃ SỬA: Sửa từ UserOneId/UserTwoId thành User1Id/User2Id cho khớp với Entity Match.cs của bạn
                    var match = new Match
                    {
                        Id = Guid.NewGuid(),
                        User1Id = myId.Value,
                        User2Id = dto.ToUserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Matches.Add(match);

                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Swipe success", isMatch = true });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Swipe success", isMatch = false });
        }

        // 🔹 ĐÃ BỔ SUNG: Hàm Helper GetUserId để giải quyết lỗi "does not exist in the current context"
        private Guid? GetUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}