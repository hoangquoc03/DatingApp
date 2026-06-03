using DatingApp.Data;
using DatingApp.Helpers;
using DatingApp.Hubs;
using DatingApp.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Services
{
    public class MatchService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public MatchService(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // ─── Lấy danh sách Match (cải thiện: kèm lastMessage + online status) ────
        public async Task<ServiceResult> GetMatchesAsync(Guid userId)
        {
            var blockedIds = await _context.Blocks
                .Where(b => b.BlockerId == userId || b.BlockedUserId == userId)
                .Select(b => b.BlockerId == userId ? b.BlockedUserId : b.BlockerId)
                .ToListAsync();

            var matches = await _context.Matches
                .Where(x => (x.User1Id == userId || x.User2Id == userId)
                    && !blockedIds.Contains(x.User1Id == userId ? x.User2Id : x.User1Id))
                .Include(x => x.UserOne)
                .Include(x => x.UserTwo)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var result = new List<object>();

            foreach (var m in matches)
            {
                var partner = m.User1Id == userId ? m.UserTwo : m.UserOne;

                var lastMessage = await _context.Messages
                    .Where(msg =>
                        (msg.SenderId == userId && msg.ReceiverId == partner.Id) ||
                        (msg.SenderId == partner.Id && msg.ReceiverId == userId))
                    .OrderByDescending(msg => msg.SentAt)
                    .Select(msg => new
                    {
                        msg.Content,
                        msg.SentAt,
                        IsMine = msg.SenderId == userId,
                        msg.IsSeen
                    })
                    .FirstOrDefaultAsync();

                var unreadCount = await _context.Messages
                    .CountAsync(msg =>
                        msg.SenderId == partner.Id &&
                        msg.ReceiverId == userId &&
                        !msg.IsSeen);

                result.Add(new
                {
                    m.Id,
                    m.CreatedAt,
                    Partner = new
                    {
                        partner.Id,
                        partner.FullName,
                        partner.AvatarUrl,
                        partner.Bio,
                        IsOnline = ChatHub.IsUserOnline(partner.Id.ToString())
                    },
                    LastMessage = lastMessage,
                    UnreadCount = unreadCount
                });
            }

            return ServiceResult.Ok(result);
        }

        // ─── Unmatch ─────────────────────────────────────────────────────────────
        public async Task<ServiceResult> UnmatchAsync(Guid userId, Guid matchId)
        {
            var match = await _context.Matches
                .FirstOrDefaultAsync(m => m.Id == matchId &&
                    (m.User1Id == userId || m.User2Id == userId));

            if (match == null)
                return ServiceResult.NotFound("Không tìm thấy tương hợp này");

            var partnerId = match.User1Id == userId ? match.User2Id : match.User1Id;

            _context.Matches.Remove(match);

            var messages = await _context.Messages
                .Where(m =>
                    (m.SenderId == userId && m.ReceiverId == partnerId) ||
                    (m.SenderId == partnerId && m.ReceiverId == userId))
                .ToListAsync();
            _context.Messages.RemoveRange(messages);

            var notifications = await _context.Notifications
                .Where(n =>
                    (n.UserId == userId && n.RelatedUserId == partnerId && n.Type == "NewMatch") ||
                    (n.UserId == partnerId && n.RelatedUserId == userId && n.Type == "NewMatch"))
                .ToListAsync();
            _context.Notifications.RemoveRange(notifications);

            await _context.SaveChangesAsync();

            return ServiceResult.Ok("Đã huỷ tương hợp");
        }

        // ─── Block User ──────────────────────────────────────────────────────────
        public async Task<ServiceResult> BlockUserAsync(Guid userId, Guid targetUserId)
        {
            if (userId == targetUserId)
                return ServiceResult.BadRequest("Bạn không thể chặn chính mình");

            var targetExists = await _context.Users.AnyAsync(u => u.Id == targetUserId);
            if (!targetExists)
                return ServiceResult.NotFound("Người dùng không tồn tại");

            var alreadyBlocked = await _context.Blocks
                .AnyAsync(b => b.BlockerId == userId && b.BlockedUserId == targetUserId);
            if (alreadyBlocked)
                return ServiceResult.BadRequest("Bạn đã chặn người này rồi");

            _context.Blocks.Add(new Block
            {
                BlockerId = userId,
                BlockedUserId = targetUserId
            });

            var match = await _context.Matches
                .FirstOrDefaultAsync(m =>
                    (m.User1Id == userId && m.User2Id == targetUserId) ||
                    (m.User1Id == targetUserId && m.User2Id == userId));

            if (match != null)
            {
                _context.Matches.Remove(match);

                var messages = await _context.Messages
                    .Where(m =>
                        (m.SenderId == userId && m.ReceiverId == targetUserId) ||
                        (m.SenderId == targetUserId && m.ReceiverId == userId))
                    .ToListAsync();
                _context.Messages.RemoveRange(messages);
            }

            await _context.SaveChangesAsync();

            return ServiceResult.Ok("Đã chặn người dùng");
        }

        // ─── Unblock User ────────────────────────────────────────────────────────
        public async Task<ServiceResult> UnblockUserAsync(Guid userId, Guid targetUserId)
        {
            var block = await _context.Blocks
                .FirstOrDefaultAsync(b => b.BlockerId == userId && b.BlockedUserId == targetUserId);

            if (block == null)
                return ServiceResult.NotFound("Bạn chưa chặn người này");

            _context.Blocks.Remove(block);
            await _context.SaveChangesAsync();

            return ServiceResult.Ok("Đã bỏ chặn");
        }

        // ─── Lấy danh sách đã chặn ──────────────────────────────────────────────
        public async Task<ServiceResult> GetBlockedUsersAsync(Guid userId)
        {
            var blocked = await _context.Blocks
                .Where(b => b.BlockerId == userId)
                .Include(b => b.BlockedUser)
                .Select(b => new
                {
                    b.Id,
                    b.CreatedAt,
                    User = new
                    {
                        b.BlockedUser.Id,
                        b.BlockedUser.FullName,
                        b.BlockedUser.AvatarUrl
                    }
                })
                .ToListAsync();

            return ServiceResult.Ok(blocked);
        }

        // ─── Report User ─────────────────────────────────────────────────────────
        public async Task<ServiceResult> ReportUserAsync(Guid reporterId, Guid targetUserId, string reason, string? description)
        {
            if (reporterId == targetUserId)
                return ServiceResult.BadRequest("Bạn không thể báo cáo chính mình");

            var targetExists = await _context.Users.AnyAsync(u => u.Id == targetUserId);
            if (!targetExists)
                return ServiceResult.NotFound("Người dùng không tồn tại");

            var alreadyReported = await _context.Reports
                .AnyAsync(r => r.ReporterId == reporterId && r.ReportedUserId == targetUserId && r.Status == "pending");
            if (alreadyReported)
                return ServiceResult.BadRequest("Bạn đã báo cáo người này rồi, đang chờ xử lý");

            _context.Reports.Add(new Report
            {
                ReporterId = reporterId,
                ReportedUserId = targetUserId,
                Reason = reason,
                Description = description
            });

            await _context.SaveChangesAsync();

            return ServiceResult.Ok("Cảm ơn bạn đã báo cáo. Chúng tôi sẽ xem xét sớm nhất.");
        }
    }
}
