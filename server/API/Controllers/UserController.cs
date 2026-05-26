using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // T?t c? endpoints ð?u c?n JWT
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinary;

        public UserController(AppDbContext context, CloudinaryService cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        // GET api/user/profile - l?y profile c?a m?nh
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Where(x => x.Id == userId)
                .Select(x => new
                {
                    x.Id,
                    x.Email,
                    x.FullName,
                    x.Bio,
                    x.AvatarUrl,
                    x.Location,
                    x.Gender,
                    x.DateOfBirth,
                    x.IsVerified,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            return Ok(user);
        }

        // PUT api/user/profile - c?p nh?t thông tin cõ b?n
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Ch? c?p nh?t nh?ng field ðý?c g?i lên (không null)
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName.Trim();

            if (dto.Bio != null) // Cho phép set bio r?ng
                user.Bio = dto.Bio.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Location))
                user.Location = dto.Location.Trim();

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Bio,
                user.Location,
                user.AvatarUrl,
                user.UpdatedAt
            });
        }

        // POST api/user/avatar - upload ?nh ð?i di?n
        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            string newAvatarUrl;
            try
            {
                // Xóa ?nh c? trên Cloudinary (n?u có)
                var oldPublicId = CloudinaryService.ExtractPublicId(user.AvatarUrl);
                if (!string.IsNullOrEmpty(oldPublicId))
                    await _cloudinary.DeleteImageAsync(oldPublicId);

                newAvatarUrl = await _cloudinary.UploadImageAsync(file, "avatars");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Upload th?t b?i: {ex.Message}");
            }

            user.AvatarUrl = newAvatarUrl;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { avatarUrl = newAvatarUrl });
        }

        // GET api/user/discover?page=1&pageSize=10 - khám phá ngý?i dùng m?i
        [HttpGet("discover")]
        public async Task<IActionResult> Discover([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // Gi?i h?n pageSize t?i ða 50
            pageSize = Math.Clamp(pageSize, 1, 50);
            page = Math.Max(1, page);

            // L?y danh sách ð? swipe
            var swipedIds = await _context.Swipes
                .Where(x => x.FromUserId == userId)
                .Select(x => x.ToUserId)
                .ToListAsync();

            var query = _context.Users
                .Where(x => x.Id != userId && !swipedIds.Contains(x.Id))
                .OrderBy(x => x.CreatedAt); // ?n ð?nh th? t? phân trang

            var total = await query.CountAsync();

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.Bio,
                    x.AvatarUrl,
                    x.Location,
                    x.Gender,
                    Age = x.DateOfBirth.HasValue
                        ? (int)((DateTime.UtcNow - x.DateOfBirth.Value).TotalDays / 365.25)
                        : (int?)null
                })
                .ToListAsync();

            return Ok(new
            {
                data = users,
                pagination = new
                {
                    page,
                    pageSize,
                    total,
                    totalPages = (int)Math.Ceiling((double)total / pageSize),
                    hasNext = page * pageSize < total,
                    hasPrev = page > 1
                }
            });
        }

        // --- Helper ---
        private Guid? GetUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}