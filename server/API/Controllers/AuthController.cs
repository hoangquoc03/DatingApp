using Microsoft.AspNetCore.Mvc;
using DatingApp.Data;
using DatingApp.Models;
using DatingApp.DTOs;
using DatingApp.Helpers;
using Microsoft.EntityFrameworkCore;
using DatingApp.DTo.cs;
using Google.Apis.Auth;

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

        // 🔹 REGISTER
                [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var email = dto.Email.Trim().ToLower();

                var exists = await _context.Users
                    .AnyAsync(x => x.Email.ToLower() == email);

                if (exists)
                    return BadRequest("Email already exists");

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    PasswordHash =
                        BCrypt.Net.BCrypt.HashPassword(dto.Password),

                    FullName = dto.FullName.Trim(),
                    Gender = dto.Gender,
                    DateOfBirth = dto.DateOfBirth.HasValue
                    ? DateTime.SpecifyKind(
                        dto.DateOfBirth.Value,
                        DateTimeKind.Utc
                    )
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

                return Ok(new
                {
                    message = "Register success"
                });
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

        // 🔹 LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

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
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin(GoogleLoginDto dto)
        {

            var payload = await GoogleJsonWebSignature.ValidateAsync(dto.Credential);

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == payload.Email);

            // nếu chưa có account
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = payload.Email,
                    FullName = payload.Name,
                    AvatarUrl = payload.Picture,
                    PasswordHash = "",
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
                user = new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.AvatarUrl
                }
            });
        }
    }
}