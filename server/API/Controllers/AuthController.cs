using DatingApp.Data;
using DatingApp.DTo.cs;
using DatingApp.DTOs;
using DatingApp.Helpers;
using DatingApp.Models;
using DatingApp.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public AuthController(
            AppDbContext context,
            JwtService jwt,
            EmailService emailService,
            IConfiguration config)
        {
            _context = context;
            _jwt = jwt;
            _emailService = emailService;
            _config = config;
        }

        // ── REGISTER ──────────────────────────────────────────────────────────────────
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var email = dto.Email.Trim().ToLower();

                // TỐI ƯU: Đưa biểu thức về dạng so sánh đồng bộ Index trực tiếp trong Database
                var exists = await _context.Users
                    .AnyAsync(x => x.Email == email);

                if (exists)
                    return BadRequest("Email already exists");

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    FullName = dto.FullName.Trim(),
                    Gender = dto.Gender,
                    DateOfBirth = dto.DateOfBirth.HasValue
                        ? DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc)
                        : null,
                    Bio = "",
                    AvatarUrl = "",
                    Location = "",
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    Status = Enums.UserStatus.Active
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Register success" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.InnerException?.Message ?? ex.Message,
                    detail = ex.Message
                });
            }
        }

        // ── LOGIN THƯỜNG ─────────────────────────────────────────────────────────────
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                return BadRequest("Email và mật khẩu không được để trống.");

            var emailClean = dto.Email.Trim().ToLower();

            // TỐI ƯU: Lọc tìm dữ liệu dựa trên chuỗi email đã dọn dẹp khoảng trắng và chữ thường
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == emailClean);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password");

            var token = _jwt.GenerateToken(user.Id, user.Email);
            var refreshToken = GenerateRefreshToken();
            SaveRefreshToken(user, refreshToken);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token,
                accessToken = token,
                refreshToken,
                user = new
                {
                    user.Id,
                    user.Email,
                    user.FullName
                }
            });
        }

        // ── GOOGLE LOGIN (CREDENTIAL FLOW) ───────────────────────────────────────────
        // 💡 ĐÃ FIX: Loại bỏ thuộc tính định tuyến bị khai báo lặp làm sập Swagger 500
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.Credential))
                return BadRequest("Google credential (ID Token) là bắt buộc.");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                // Xác thực chữ ký số ID Token an toàn từ Google nội bộ
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.Credential);
            }
            catch (InvalidJwtException)
            {
                return Unauthorized("Chứng chỉ Google không hợp lệ hoặc đã hết hạn.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Xác thực danh tính qua Google thất bại.");
            }

            if (string.IsNullOrEmpty(payload.Email))
                return Unauthorized("Không thể truy cập thông tin email từ tài khoản Google này.");

            var emailClean = payload.Email.Trim().ToLower();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == emailClean);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = emailClean,
                    FullName = payload.Name ?? emailClean,
                    AvatarUrl = payload.Picture ?? "",
                    PasswordHash = "OAUTH_EXTERNAL_ACCOUNT_NO_PASSWORD", // Khóa bảo mật tài khoản ngoài
                    CreatedAt = DateTime.UtcNow,
                    Status = Enums.UserStatus.Active
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var token = _jwt.GenerateToken(user.Id, user.Email);
            var refreshToken = GenerateRefreshToken();
            SaveRefreshToken(user, refreshToken);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token,
                accessToken = token,
                refreshToken,
                user = new { user.Id, user.Email, user.FullName, user.AvatarUrl }
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return BadRequest("Refresh token is required.");

            var tokenHash = HashToken(dto.RefreshToken);
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.RefreshTokenHash == tokenHash &&
                x.RefreshTokenExpiresAt.HasValue &&
                x.RefreshTokenExpiresAt > DateTime.UtcNow);

            if (user == null)
                return Unauthorized("Invalid or expired refresh token.");

            var accessToken = _jwt.GenerateToken(user.Id, user.Email);
            var nextRefreshToken = GenerateRefreshToken();
            SaveRefreshToken(user, nextRefreshToken);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = accessToken,
                accessToken,
                refreshToken = nextRefreshToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return Ok(new { message = "Logged out" });

            var tokenHash = HashToken(dto.RefreshToken);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.RefreshTokenHash == tokenHash);
            if (user != null)
            {
                user.RefreshTokenHash = null;
                user.RefreshTokenExpiresAt = null;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Logged out" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user != null)
            {
                var rawToken = GenerateResetToken();
                user.PasswordResetTokenHash = HashToken(rawToken);
                user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
                await _context.SaveChangesAsync();

                var frontendBaseUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
                var resetLink = $"{frontendBaseUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";
                await _emailService.SendPasswordResetAsync(user.Email, resetLink);
            }

            return Ok(new
            {
                message = "If that email exists, a password reset link has been sent."
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tokenHash = HashToken(dto.Token);
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.PasswordResetTokenHash == tokenHash &&
                x.PasswordResetTokenExpiresAt.HasValue &&
                x.PasswordResetTokenExpiresAt > DateTime.UtcNow);

            if (user == null)
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresAt = null;
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đặt lại mật khẩu thành công." });
        }

        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        private static string GenerateResetToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        }

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }

        private static void SaveRefreshToken(User user, string refreshToken)
        {
            user.RefreshTokenHash = HashToken(refreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(30);
        }
    }

    // Lớp DTO nhận Token từ Google Component
    public class GoogleLoginRequestDto
    {
        public string Credential { get; set; } = string.Empty;
    }
}