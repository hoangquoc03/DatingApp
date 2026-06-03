using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DatingApp.Services;
using System.Security.Claims;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MatchController : ControllerBase
    {
        private readonly MatchService _matchService;

        public MatchController(MatchService matchService)
        {
            _matchService = matchService;
        }

        // ─── Lấy danh sách Match (kèm lastMessage + online) ─────────────────────
        [HttpGet]
        public async Task<IActionResult> GetMyMatches()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _matchService.GetMatchesAsync(userId.Value);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // ─── Unmatch ─────────────────────────────────────────────────────────────
        [HttpDelete("{matchId}")]
        public async Task<IActionResult> Unmatch(Guid matchId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _matchService.UnmatchAsync(userId.Value, matchId);
            return result.Success ? Ok(new { message = result.Message }) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // ─── Block User ──────────────────────────────────────────────────────────
        [HttpPost("block/{targetUserId}")]
        public async Task<IActionResult> BlockUser(Guid targetUserId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _matchService.BlockUserAsync(userId.Value, targetUserId);
            return result.Success ? Ok(new { message = result.Message }) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // ─── Unblock User ────────────────────────────────────────────────────────
        [HttpDelete("block/{targetUserId}")]
        public async Task<IActionResult> UnblockUser(Guid targetUserId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _matchService.UnblockUserAsync(userId.Value, targetUserId);
            return result.Success ? Ok(new { message = result.Message }) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // ─── Danh sách đã chặn ──────────────────────────────────────────────────
        [HttpGet("blocked")]
        public async Task<IActionResult> GetBlockedUsers()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _matchService.GetBlockedUsersAsync(userId.Value);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // ─── Report User ─────────────────────────────────────────────────────────
        [HttpPost("report/{targetUserId}")]
        public async Task<IActionResult> ReportUser(Guid targetUserId, [FromBody] ReportDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _matchService.ReportUserAsync(userId.Value, targetUserId, dto.Reason, dto.Description);
            return result.Success ? Ok(new { message = result.Message }) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        private Guid? GetUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public class ReportDto
    {
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}