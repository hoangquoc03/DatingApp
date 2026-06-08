using DatingApp.DTOs;
using DatingApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(dto);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
        {
            var result = await _authService.GoogleLoginAsync(dto);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            var result = await _authService.RefreshAsync(dto);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
        {
            var result = await _authService.LogoutAsync(dto);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ForgotPasswordAsync(dto);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(dto);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyEmailAsync(dto);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }
    }
}