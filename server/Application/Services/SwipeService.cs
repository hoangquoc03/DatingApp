using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Helpers;
using DatingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Services
{
    public class SwipeService
    {
        private readonly AppDbContext _context;

        public SwipeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> SwipeAsync(Guid myId, SwipeDto dto)
        {
            if (dto.ToUserId == myId)
                return ServiceResult.BadRequest("Bạn không thể swipe chính mình");

            var targetExists = await _context.Users.AnyAsync(u => u.Id == dto.ToUserId);
            if (!targetExists)
                return ServiceResult.NotFound("Người dùng không tồn tại");

            var alreadySwiped = await _context.Swipes
                .AnyAsync(s => s.FromUserId == myId && s.ToUserId == dto.ToUserId);
            if (alreadySwiped)
                return ServiceResult.Error("Bạn đã swipe người này rồi", 409); // 409 Conflict

            var swipe = new Swipe
            {
                Id = Guid.NewGuid(),
                FromUserId = myId,
                ToUserId = dto.ToUserId,
                IsLike = dto.IsLike,
                CreatedAt = DateTime.UtcNow
            };
            _context.Swipes.Add(swipe);

            if (dto.IsLike)
            {
                var partnerLikedMe = await _context.Swipes
                    .AnyAsync(x => x.FromUserId == dto.ToUserId && x.ToUserId == myId && x.IsLike);

                if (partnerLikedMe)
                {
                    var matchExists = await _context.Matches
                        .AnyAsync(m =>
                            (m.User1Id == myId && m.User2Id == dto.ToUserId) ||
                            (m.User1Id == dto.ToUserId && m.User2Id == myId));

                    if (!matchExists)
                    {
                        var match = new Match
                        {
                            Id = Guid.NewGuid(),
                            User1Id = myId,
                            User2Id = dto.ToUserId,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Matches.Add(match);
                    }

                    await _context.SaveChangesAsync();
                    return ServiceResult.Ok(new { message = "Swipe success", isMatch = true });
                }
            }

            await _context.SaveChangesAsync();
            return ServiceResult.Ok(new { message = "Swipe success", isMatch = false });
        }
    }
}
