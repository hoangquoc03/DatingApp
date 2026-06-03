using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatingApp.Data;
using DatingApp.Models;
using DatingApp.Enums;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // --- Bí mật để tự thăng cấp thành Admin ---
        [AllowAnonymous]
        [HttpPost("secret-promote")]
        public async Task<IActionResult> PromoteToAdmin([FromQuery] string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound("Email không tồn tại.");

            user.Role = Role.Admin;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Thành công! {email} đã trở thành Admin. Vui lòng đăng xuất và đăng nhập lại để cập nhật quyền." });
        }

        // --- Quản trị viên mới được gọi các API dưới đây ---

        private bool IsAdmin()
        {
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            // Hiện tại chúng ta chưa đưa Role vào JWT, nên tôi sẽ query trực tiếp DB để check cho chắc chắn.
            // (Tuy nhiên cách tốt nhất là cập nhật AuthService đưa Role vào JWT)
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdStr, out var userId))
            {
                var user = _context.Users.Find(userId);
                return user?.Role == Role.Admin;
            }
            return false;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            if (!IsAdmin()) return Forbid();

            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalMatches = await _context.Matches.CountAsync();
            var totalReports = await _context.Reports.CountAsync();
            var unresolvedReports = await _context.Reports.CountAsync(r => r.Status == "pending");

            return Ok(new
            {
                totalUsers,
                activeUsers,
                totalMatches,
                totalReports,
                unresolvedReports
            });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            if (!IsAdmin()) return Forbid();

            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.AvatarUrl,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt
                })
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("users/{id}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive(Guid id)
        {
            if (!IsAdmin()) return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (user.Role == Role.Admin) return BadRequest("Không thể khóa tài khoản Admin khác.");

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Đã {(user.IsActive ? "mở khóa" : "khóa")} tài khoản thành công." });
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetReports()
        {
            if (!IsAdmin()) return Forbid();

            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Select(r => new
                {
                    r.Id,
                    r.Reason,
                    r.Description,
                    r.Status,
                    r.CreatedAt,
                    Reporter = new { r.Reporter.Id, r.Reporter.FullName, r.Reporter.AvatarUrl },
                    ReportedUser = new { r.ReportedUser.Id, r.ReportedUser.FullName, r.ReportedUser.AvatarUrl }
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(reports);
        }

        [HttpPost("reports/{id}/resolve")]
        public async Task<IActionResult> ResolveReport(Guid id)
        {
            if (!IsAdmin()) return Forbid();

            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            report.Status = "resolved";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã đánh dấu xử lý xong báo cáo." });
        }
    }
}
