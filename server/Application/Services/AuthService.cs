using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Helpers;
using DatingApp.Models;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DatingApp.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public AuthService(
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

        public async Task<ServiceResult> RegisterAsync(RegisterDto dto)
        {
            try
            {
                var email = dto.Email.Trim().ToLower();

                var exists = await _context.Users.AnyAsync(x => x.Email == email);
                if (exists) return ServiceResult.BadRequest("Email already exists");

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

                return ServiceResult.Ok(new { message = "Register success" });
            }
            catch (Exception ex)
            {
                return ServiceResult.Error(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<ServiceResult> LoginAsync(LoginDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                return ServiceResult.BadRequest("Email và mật khẩu không được để trống.");

            var emailClean = dto.Email.Trim().ToLower();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == emailClean);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return ServiceResult.Unauthorized("Email hoặc mật khẩu không chính xác.");

            if (!user.IsActive)
                return ServiceResult.Unauthorized("Tài khoản của bạn đã bị khóa.");

            var token = _jwt.GenerateToken(user.Id, user.Email, user.Role);
            var refreshToken = GenerateRefreshToken();
            SaveRefreshToken(user, refreshToken);
            await _context.SaveChangesAsync();

            return ServiceResult.Ok(new
            {
                token,
                accessToken = token,
                refreshToken,
                user = new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.AvatarUrl,
                    Role = (int)user.Role
                }
            });
        }

        public async Task<ServiceResult> GoogleLoginAsync(GoogleLoginRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.Credential))
                return ServiceResult.BadRequest("Google credential (ID Token) là bắt buộc.");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.Credential);
            }
            catch (InvalidJwtException)
            {
                return ServiceResult.Unauthorized("Chứng chỉ Google không hợp lệ hoặc đã hết hạn.");
            }
            catch (Exception)
            {
                return ServiceResult.Error("Xác thực danh tính qua Google thất bại.", 500);
            }

            if (string.IsNullOrEmpty(payload.Email))
                return ServiceResult.Unauthorized("Không thể truy cập thông tin email từ tài khoản Google này.");

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
                    PasswordHash = "OAUTH_EXTERNAL_ACCOUNT_NO_PASSWORD",
                    CreatedAt = DateTime.UtcNow,
                    Status = Enums.UserStatus.Active
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            if (!user.IsActive)
                return ServiceResult.Unauthorized("Tài khoản của bạn đã bị khóa.");

            var token = _jwt.GenerateToken(user.Id, user.Email, user.Role);
            var refreshToken = GenerateRefreshToken();
            SaveRefreshToken(user, refreshToken);
            await _context.SaveChangesAsync();

            return ServiceResult.Ok(new
            {
                token,
                accessToken = token,
                refreshToken,
                user = new { user.Id, user.Email, user.FullName, user.AvatarUrl, Role = (int)user.Role }
            });
        }

        public async Task<ServiceResult> RefreshAsync(RefreshTokenDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return ServiceResult.BadRequest("Refresh token is required.");

            var tokenHash = HashToken(dto.RefreshToken);
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.RefreshTokenHash == tokenHash &&
                x.RefreshTokenExpiresAt.HasValue &&
                x.RefreshTokenExpiresAt > DateTime.UtcNow);

            if (user == null)
                return ServiceResult.Unauthorized("Invalid or expired refresh token.");

            var accessToken = _jwt.GenerateToken(user.Id, user.Email, user.Role);
            var nextRefreshToken = GenerateRefreshToken();
            SaveRefreshToken(user, nextRefreshToken);
            await _context.SaveChangesAsync();

            return ServiceResult.Ok(new
            {
                token = accessToken,
                accessToken,
                refreshToken = nextRefreshToken
            });
        }

        public async Task<ServiceResult> LogoutAsync(RefreshTokenDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return ServiceResult.Ok(new { message = "Logged out" });

            var tokenHash = HashToken(dto.RefreshToken);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.RefreshTokenHash == tokenHash);
            if (user != null)
            {
                user.RefreshTokenHash = null;
                user.RefreshTokenExpiresAt = null;
                await _context.SaveChangesAsync();
            }

            return ServiceResult.Ok(new { message = "Logged out" });
        }

        public async Task<ServiceResult> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
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

            return ServiceResult.Ok(new { message = "If that email exists, a password reset link has been sent." });
        }

        public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var tokenHash = HashToken(dto.Token);
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.PasswordResetTokenHash == tokenHash &&
                x.PasswordResetTokenExpiresAt.HasValue &&
                x.PasswordResetTokenExpiresAt > DateTime.UtcNow);

            if (user == null)
                return ServiceResult.BadRequest("Token không hợp lệ hoặc đã hết hạn.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresAt = null;
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult.Ok(new { message = "Đặt lại mật khẩu thành công." });
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
}
