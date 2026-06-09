using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatingApp.Data;
using DatingApp.Models;
using DatingApp.DTOs;
using DatingApp.Hubs;
using DatingApp.Services;
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
        private readonly CloudinaryService _cloudinary;

        public MessagesController(
            AppDbContext context,
            IHubContext<ChatHub> hub,
            CloudinaryService cloudinary)
        {
            _context = context;
            _hub = hub;
            _cloudinary = cloudinary;
        }

        // ── Gửi tin nhắn văn bản ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
        {
            var senderId = GetUserId();
            if (senderId == null) return Unauthorized();

            if (dto.ReceiverId == senderId)
                return BadRequest(new { message = "Không thể nhắn tin cho chính mình" });

            var receiverExists = await _context.Users.AnyAsync(u => u.Id == dto.ReceiverId);
            if (!receiverExists)
                return NotFound(new { message = "Người nhận không tồn tại" });

            var isMatched = await CheckMatch(senderId.Value, dto.ReceiverId);
            if (!isMatched)
                return BadRequest(new { message = "Bạn chưa match với người này. Hãy match trước khi nhắn tin!" });

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = senderId.Value,
                ReceiverId = dto.ReceiverId,
                Content = dto.Content?.Trim() ?? "",
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Broadcast realtime cho người nhận
            var msgPayload = BuildMessagePayload(message);
            await _hub.Clients
                .Group(dto.ReceiverId.ToString())
                .SendAsync("ReceiveMessage", msgPayload);

            return Ok(msgPayload);
        }

        // ── Gửi tin nhắn ảnh ─────────────────────────────────────────────────
        [HttpPost("image")]
        public async Task<IActionResult> SendImage([FromForm] Guid receiverId, IFormFile file)
        {
            var senderId = GetUserId();
            if (senderId == null) return Unauthorized();

            if (receiverId == senderId)
                return BadRequest(new { message = "Không thể nhắn tin cho chính mình" });

            var receiverExists = await _context.Users.AnyAsync(u => u.Id == receiverId);
            if (!receiverExists)
                return NotFound(new { message = "Người nhận không tồn tại" });

            var isMatched = await CheckMatch(senderId.Value, receiverId);
            if (!isMatched)
                return BadRequest(new { message = "Bạn chưa match với người này" });

            // Upload ảnh lên Cloudinary
            string imageUrl;
            try
            {
                var folderName = senderId.Value.CompareTo(receiverId) < 0 
                    ? $"{senderId.Value}_{receiverId}" 
                    : $"{receiverId}_{senderId.Value}";

                var uploadResult = await _cloudinary.UploadImageAsync(file, $"chat_images/{folderName}");
                imageUrl = uploadResult.Url;
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Upload thất bại: {ex.Message}" });
            }

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = senderId.Value,
                ReceiverId = receiverId,
                Content = "",      // Tin nhắn ảnh không có text
                ImageUrl = imageUrl,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var msgPayload = BuildMessagePayload(message);
            await _hub.Clients
                .Group(receiverId.ToString())
                .SendAsync("ReceiveMessage", msgPayload);

            return Ok(msgPayload);
        }

        // ── Lấy lịch sử chat ─────────────────────────────────────────────────
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetChat(Guid userId)
        {
            var myId = GetUserId();
            if (myId == null) return Unauthorized();

            var isMatched = await CheckMatch(myId.Value, userId);
            if (!isMatched)
                return BadRequest(new { message = "Bạn chưa match với người này" });

            var messages = await _context.Messages
                .Where(x =>
                    (x.SenderId == myId && x.ReceiverId == userId) ||
                    (x.SenderId == userId && x.ReceiverId == myId))
                .OrderBy(x => x.SentAt)
                .Select(x => new
                {
                    x.Id,
                    x.SenderId,
                    x.ReceiverId,
                    x.Content,
                    x.ImageUrl,
                    x.IsSeen,
                    x.SeenAt,
                    x.SentAt
                })
                .ToListAsync();

            return Ok(messages);
        }

        // ── Đánh dấu đã đọc + Broadcast realtime ─────────────────────────────
        [HttpPut("seen/{userId}")]
        public async Task<IActionResult> Seen(Guid userId)
        {
            var myId = GetUserId();
            if (myId == null) return Unauthorized();

            var messages = await _context.Messages
                .Where(x =>
                    x.SenderId == userId &&
                    x.ReceiverId == myId &&
                    !x.IsSeen)
                .ToListAsync();

            if (messages.Count > 0)
            {
                var now = DateTime.UtcNow;
                foreach (var msg in messages)
                {
                    msg.IsSeen = true;
                    msg.SeenAt = now;
                }
                await _context.SaveChangesAsync();

                // ✅ Broadcast realtime cho người gửi biết tin nhắn đã được đọc
                await _hub.Clients
                    .Group(userId.ToString())
                    .SendAsync("MessagesSeen", new
                    {
                        byUserId = myId.Value.ToString(),
                        seenAt = now
                    });
            }

            return Ok(new { seenCount = messages.Count });
        }

        [HttpDelete("{messageId}")]
        public async Task<IActionResult> Delete(Guid messageId)
        {
            var myId = GetUserId();
            if (myId == null) return Unauthorized();

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null) return NotFound(new { message = "Tin nhắn không tồn tại" });

            if (message.SenderId != myId)
                return Forbid(); // Chỉ người gửi mới được thu hồi

            // Soft delete tin nhắn
            message.Content = "Tin nhắn đã bị thu hồi";
            message.ImageUrl = null;
            await _context.SaveChangesAsync();

            // Broadcast realtime cho cả 2 phía
            var partnerId = message.SenderId == myId ? message.ReceiverId : message.SenderId;
            var payload = new { messageId = message.Id };
            
            await _hub.Clients.Group(partnerId.ToString()).SendAsync("MessageDeleted", payload);
            await _hub.Clients.Group(myId.ToString()).SendAsync("MessageDeleted", payload);

            return Ok(payload);
        }

        [HttpPut("{messageId}")]
        public async Task<IActionResult> Edit(Guid messageId, [FromBody] EditMessageDto dto)
        {
            var myId = GetUserId();
            if (myId == null) return Unauthorized();

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null) return NotFound(new { message = "Tin nhắn không tồn tại" });

            if (message.SenderId != myId)
                return Forbid(); // Chỉ người gửi mới được chỉnh sửa

            if (!string.IsNullOrEmpty(message.ImageUrl))
                return BadRequest(new { message = "Không thể chỉnh sửa tin nhắn ảnh" });

            message.Content = dto.Content?.Trim() ?? "";
            await _context.SaveChangesAsync();

            // Broadcast realtime cho cả 2 phía
            var partnerId = message.SenderId == myId ? message.ReceiverId : message.SenderId;
            var payload = new { messageId = message.Id, content = message.Content };
            
            await _hub.Clients.Group(partnerId.ToString()).SendAsync("MessageEdited", payload);
            await _hub.Clients.Group(myId.ToString()).SendAsync("MessageEdited", payload);

            return Ok(payload);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private Guid? GetUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }

        private async Task<bool> CheckMatch(Guid userId1, Guid userId2)
        {
            return await _context.Matches
                .AnyAsync(m =>
                    (m.User1Id == userId1 && m.User2Id == userId2) ||
                    (m.User1Id == userId2 && m.User2Id == userId1));
        }

        private static object BuildMessagePayload(Message msg) => new
        {
            msg.Id,
            senderId = msg.SenderId,
            receiverId = msg.ReceiverId,
            msg.Content,
            msg.ImageUrl,
            msg.IsSeen,
            msg.SeenAt,
            msg.SentAt
        };
    }

    public class EditMessageDto
    {
        public string Content { get; set; } = string.Empty;
    }
}