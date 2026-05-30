using DatingApp.Data;
using DatingApp.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> GetNotificationsAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(30)
                .Select(n => new
                {
                    n.Id,
                    n.Content,
                    n.Type,
                    n.RelatedUserId,
                    n.IsRead,
                    n.CreatedAt,
                    // Lấy thêm thông tin người liên quan nếu có
                    RelatedUser = n.RelatedUserId.HasValue 
                        ? _context.Users
                            .Where(u => u.Id == n.RelatedUserId.Value)
                            .Select(u => new { u.Id, u.FullName, u.AvatarUrl })
                            .FirstOrDefault()
                        : null
                })
                .ToListAsync();

            var unreadCount = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return ServiceResult.Ok(new { notifications, unreadCount });
        }

        public async Task<ServiceResult> MarkAsReadAsync(Guid userId, int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null) return ServiceResult.NotFound("Không tìm thấy thông báo");

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> MarkAllAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
            }

            if (unreadNotifications.Any())
            {
                await _context.SaveChangesAsync();
            }

            return ServiceResult.Ok();
        }
    }
}
