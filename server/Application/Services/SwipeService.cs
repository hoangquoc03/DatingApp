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
                IsSuperLike = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Swipes.Add(swipe);

            if (dto.IsLike)
            {
                var partnerLikedMe = await _context.Swipes
                    .AnyAsync(x => x.FromUserId == dto.ToUserId && x.ToUserId == myId && x.IsLike);

                var user1 = await _context.Users.FirstOrDefaultAsync(u => u.Id == myId);
                var user2 = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.ToUserId);

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
                            notif1.Id, 
                            notif1.Content, 
                            notif1.Type, 
                            notif1.RelatedUserId, 
                            notif1.CreatedAt, 
                            notif1.IsRead,
                            RelatedUser = user2 != null ? new { user2.Id, user2.FullName, user2.AvatarUrl } : null
                        });
                        await _hubContext.Clients.Group(dto.ToUserId.ToString()).SendAsync("ReceiveNotification", new {
                            notif2.Id, 
                            notif2.Content, 
                            notif2.Type, 
                            notif2.RelatedUserId, 
                            notif2.CreatedAt, 
                            notif2.IsRead,
                            RelatedUser = user1 != null ? new { user1.Id, user1.FullName, user1.AvatarUrl } : null
                        });
                    }
                    else
                    {
                        await _context.SaveChangesAsync();
                    }
                    return ServiceResult.Ok(new { message = "Swipe success", isMatch = true });
                }
                else
                {
                    // Tạo thông báo NewLike
                    var notifContent = $"{user1?.FullName ?? "Ai đó"} đã thích bạn! 😍";

                    var notif = new Notification 
                    { 
                        UserId = dto.ToUserId, 
                        Content = notifContent, 
                        Type = "NewLike", 
                        RelatedUserId = myId 
                    };
                    _context.Notifications.Add(notif);
                    await _context.SaveChangesAsync();

                    // Realtime push
                    await _hubContext.Clients.Group(dto.ToUserId.ToString()).SendAsync("ReceiveNotification", new {
                        notif.Id, 
                        notif.Content, 
                        notif.Type, 
                        notif.RelatedUserId, 
                        notif.CreatedAt, 
                        notif.IsRead,
                        RelatedUser = user1 != null ? new { user1.Id, user1.FullName, user1.AvatarUrl } : null
                    });

                    return ServiceResult.Ok(new { message = "Swipe success", isMatch = false });
                }
            }

            await _context.SaveChangesAsync();
            return ServiceResult.Ok(new { message = "Swipe success", isMatch = false });
        }

        public async Task<ServiceResult> GetLikesReceivedAsync(Guid myId)
        {
            var swipedUserIds = await _context.Swipes
                .Where(s => s.FromUserId == myId)
                .Select(s => s.ToUserId)
                .ToListAsync();

            var likes = await _context.Swipes
                .Include(s => s.FromUser)
                .Where(s => s.ToUserId == myId && s.IsLike && !swipedUserIds.Contains(s.FromUserId))
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => s.FromUser)
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.Bio,
                    x.AvatarUrl,
                    x.Location,
                    x.Zodiac,
                    x.Mbti,
                    x.Interests,
                    x.IsVerified,
                    x.Height,
                    x.Occupation,
                    x.Education,
                    x.Smoking,
                    x.Drinking,
                    Age = x.DateOfBirth.HasValue
                        ? (int)((DateTime.UtcNow - x.DateOfBirth.Value).TotalDays / 365.25)
                        : (int?)null
                })
                .ToListAsync();

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == myId);
            if (currentUser == null) return ServiceResult.NotFound("User not found");

            var result = likes.Select(x => new
            {
                x.Id,
                x.FullName,
                x.Bio,
                x.AvatarUrl,
                x.Location,
                x.IsVerified,
                x.Zodiac,
                x.Mbti,
                x.Interests,
                x.Age,
                x.Height,
                x.Occupation,
                x.Education,
                x.Smoking,
                x.Drinking,
                CompatibilityScore = UserService.CalculateCompatibility(currentUser, x.Interests, x.Zodiac, x.Mbti, x.Age)
            }).ToList();

            return ServiceResult.Ok(result);
        }



        public async Task<ServiceResult> ResetSwipesAsync(Guid myId)

        {
            // Xóa swipes từ mình
            var swipes = await _context.Swipes
                .Where(s => s.FromUserId == myId)
                .ToListAsync();
            _context.Swipes.RemoveRange(swipes);

            // Xóa matches liên quan đến mình
            var matches = await _context.Matches
                .Where(m => m.User1Id == myId || m.User2Id == myId)
                .ToListAsync();
            _context.Matches.RemoveRange(matches);

            // Xóa tin nhắn liên quan đến mình
            var messages = await _context.Messages
                .Where(m => m.SenderId == myId || m.ReceiverId == myId)
                .ToListAsync();
            _context.Messages.RemoveRange(messages);

            // Xóa thông báo liên quan đến mình
            var notifications = await _context.Notifications
                .Where(n => n.UserId == myId || n.RelatedUserId == myId)
                .ToListAsync();
            _context.Notifications.RemoveRange(notifications);

            await _context.SaveChangesAsync();
            return ServiceResult.Ok(new { message = "Reset swipes thành công." });
        }
    }
}
