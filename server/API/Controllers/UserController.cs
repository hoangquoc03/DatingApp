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
    [Authorize] 
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinary;

        public UserController(AppDbContext context, CloudinaryService cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

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


        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();


            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName.Trim();

            if (dto.Bio != null) 
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

        [Authorize]
        [HttpGet("discover")]
        public async Task<IActionResult> Discover([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // Giới hạn số lượng bản ghi trên một trang để bảo vệ tài nguyên hệ thống
            pageSize = Math.Clamp(pageSize, 1, 50);
            page = Math.Max(1, page);

            // Lấy danh sách ID của những người dùng hiện tại đã tương tác (swipe)
            var swipedIds = await _context.Swipes
                .Where(x => x.FromUserId == userId.Value)
                .Select(x => x.ToUserId)
                .ToListAsync();

            // Sử dụng câu lệnh truy vấn ăn Index Primary Key (Guid so sánh trực tiếp với Guid)
            var query = _context.Users
                .Where(x => x.Id != userId.Value && !swipedIds.Contains(x.Id))
                .OrderBy(x => x.CreatedAt);

            var total = await query.CountAsync();

            // Thực hiện phân trang (Skip/Take) trực tiếp ở tầng Cơ sở dữ liệu PostgreSQL
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


        private Guid? GetUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}