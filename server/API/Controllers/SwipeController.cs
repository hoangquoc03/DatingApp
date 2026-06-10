using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DatingApp.DTOs;
using DatingApp.Services;
using System.Security.Claims;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SwipeController : ControllerBase
    {
        private readonly SwipeService _swipeService;

        public SwipeController(SwipeService swipeService)
        {
            _swipeService = swipeService;
        }

        [HttpPost]
        public async Task<IActionResult> Swipe([FromBody] SwipeDto dto)
        {
            var myId = GetUserId();
            if (myId == null) return Unauthorized();

            var result = await _swipeService.SwipeAsync(myId.Value, dto);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpGet("likes")]
        public async Task<IActionResult> GetLikesReceived()
        {
            var myId = GetUserId();
            if (myId == null) return Unauthorized();

            var result = await _swipeService.GetLikesReceivedAsync(myId.Value);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetSwipes()
        {
            var myId = GetUserId();
            if (myId == null) return Unauthorized();

            var result = await _swipeService.ResetSwipesAsync(myId.Value);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        private Guid? GetUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}