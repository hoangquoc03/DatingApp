using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DatingApp.Services;
using System.Security.Claims;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _notificationService.GetNotificationsAsync(userId.Value);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _notificationService.MarkAsReadAsync(userId.Value, id);
            return result.Success ? Ok() : StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _notificationService.MarkAllAsReadAsync(userId.Value);
            return result.Success ? Ok() : StatusCode(result.StatusCode, new { message = result.Message });
        }

        private Guid? GetUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
