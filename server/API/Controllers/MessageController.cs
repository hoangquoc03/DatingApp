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