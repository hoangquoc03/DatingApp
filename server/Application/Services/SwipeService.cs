using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Helpers;
using DatingApp.Models;
using DatingApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Services
{
    public class SwipeService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public SwipeService(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
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

                        // Tạo thông báo cho cả 2 người
                        var notif1 = new Notification { UserId = myId, Content = "Bạn có một tương hợp mới! 🎉", Type = "NewMatch", RelatedUserId = dto.ToUserId };
                        var notif2 = new Notification { UserId = dto.ToUserId, Content = "Bạn có một tương hợp mới! 🎉", Type = "NewMatch", RelatedUserId = myId };
                        
                        _context.Notifications.AddRange(notif1, notif2);
                        await _context.SaveChangesAsync();

                        // Realtime push
                        await _hubContext.Clients.Group(myId.ToString()).SendAsync("ReceiveNotification", new {
                            notif1.Id, notif1.Content, notif1.Type, notif1.RelatedUserId, notif1.CreatedAt, notif1.IsRead
                        });
                        await _hubContext.Clients.Group(dto.ToUserId.ToString()).SendAsync("ReceiveNotification", new {
                            notif2.Id, notif2.Content, notif2.Type, notif2.RelatedUserId, notif2.CreatedAt, notif2.IsRead
                        });
                    }
                    else
                    {
                        await _context.SaveChangesAsync();
                    }
                    return ServiceResult.Ok(new { message = "Swipe success", isMatch = true });
                }
            }

            await _context.SaveChangesAsync();
            return ServiceResult.Ok(new { message = "Swipe success", isMatch = false });
        }
    }
}
