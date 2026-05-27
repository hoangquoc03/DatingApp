using DatingApp.Data;
using DatingApp.DTo.cs;
using DatingApp.DTOs;
using DatingApp.Helpers;
using DatingApp.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;

        public AuthController(AppDbContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
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

            return Ok(new
            {
                token,
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

            return Ok(new
            {
                token,
                user = new { user.Id, user.Email, user.FullName, user.AvatarUrl }
            });
        }
    }

    // Lớp DTO nhận Token từ Google Component
    public class GoogleLoginRequestDto
    {
        public string Credential { get; set; } = string.Empty;
    }
}