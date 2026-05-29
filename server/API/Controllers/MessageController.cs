using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatingApp.Data;
using DatingApp.Models;
using DatingApp.DTOs;
using DatingApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hub;

        public MessagesController(
            AppDbContext context,
            IHubContext<ChatHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        [HttpPost]
        public async Task<IActionResult> Send(SendMessageDto dto)
        {
            var senderId =
                Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // 🔹 FIX: Chặn nhắn tin cho chính mình
            if (dto.ReceiverId == senderId)
                return BadRequest(new { message = "Không thể nhắn tin cho chính mình" });

            // 🔹 FIX: Kiểm tra người nhận có tồn tại không
            var receiverExists = await _context.Users
                .AnyAsync(u => u.Id == dto.ReceiverId);
            if (!receiverExists)
                return NotFound(new { message = "Người nhận không tồn tại" });

            // 🔹 FIX: Kiểm tra hai người đã match chưa — chỉ cho nhắn tin khi đã match
            var isMatched = await _context.Matches
                .AnyAsync(m =>
                    (m.User1Id == senderId && m.User2Id == dto.ReceiverId) ||
                    (m.User1Id == dto.ReceiverId && m.User2Id == senderId));
            if (!isMatched)
                return BadRequest(new { message = "Bạn chưa match với người này. Hãy match trước khi nhắn tin!" });

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                Content = dto.Content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _hub.Clients
                .Group(dto.ReceiverId.ToString())
                .SendAsync("ReceiveMessage", message);

            return Ok(message);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetChat(Guid userId)
        {
            var myId =
                Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // 🔹 FIX: Kiểm tra match trước khi cho xem lịch sử chat
            var isMatched = await _context.Matches
                .AnyAsync(m =>
                    (m.User1Id == myId && m.User2Id == userId) ||
                    (m.User1Id == userId && m.User2Id == myId));
            if (!isMatched)
                return BadRequest(new { message = "Bạn chưa match với người này" });

            var messages = await _context.Messages
                .Where(x =>
                    (x.SenderId == myId && x.ReceiverId == userId) ||
                    (x.SenderId == userId && x.ReceiverId == myId))
                .OrderBy(x => x.SentAt)
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPut("seen/{userId}")]
        public async Task<IActionResult> Seen(Guid userId)
        {
            var myId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var messages = await _context.Messages
                .Where(x =>
                    x.SenderId == userId &&
                    x.ReceiverId == myId &&
                    !x.IsSeen)
                .ToListAsync();

            foreach (var msg in messages)
            {
                msg.IsSeen = true;
                msg.SeenAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }

}