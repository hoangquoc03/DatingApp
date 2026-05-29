using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinary;

        public UserService(AppDbContext context, CloudinaryService cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<ServiceResult> GetProfileAsync(Guid userId)
        {
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
                    x.IsOnboarded,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null) return ServiceResult.NotFound("User not found");

            return ServiceResult.Ok(user);
        }

        public async Task<ServiceResult> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return ServiceResult.NotFound("User not found");

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName.Trim();

            if (dto.Bio != null)
                user.Bio = dto.Bio.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Location))
                user.Location = dto.Location.Trim();

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult.Ok(new
            {
                user.Id,
                user.FullName,
                user.Bio,
                user.Location,
                user.AvatarUrl,
                user.UpdatedAt,
                user.IsOnboarded
            });
        }

        public async Task<ServiceResult> UpdateOnboardingAsync(Guid userId, OnboardingDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return ServiceResult.NotFound("User not found");

            user.LookingFor = dto.LookingFor;
            if (dto.Interests != null) user.Interests = dto.Interests;
            user.Lifestyle = dto.Lifestyle;
            if (dto.Values != null) user.Values = dto.Values;
            if (dto.Distance.HasValue) user.MaxDistance = dto.Distance;
            user.Vibe = dto.Vibe;
            user.IsOnboarded = true;
            
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult.Ok(new
            {
                user.Id,
                user.IsOnboarded,
                user.LookingFor,
                user.Interests,
                user.Lifestyle,
                user.Values,
                user.MaxDistance,
                user.Vibe
            });
        }

        public async Task<ServiceResult> UploadAvatarAsync(Guid userId, IFormFile file)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return ServiceResult.NotFound("User not found");

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
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return ServiceResult.Error($"Upload thất bại: {ex.Message}");
            }

            user.AvatarUrl = newAvatarUrl;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult.Ok(new { avatarUrl = newAvatarUrl });
        }

        public async Task<ServiceResult> DiscoverAsync(Guid userId, int page, int pageSize)
        {
            pageSize = Math.Clamp(pageSize, 1, 50);
            page = Math.Max(1, page);

            var swipedIds = await _context.Swipes
                .Where(x => x.FromUserId == userId)
                .Select(x => x.ToUserId)
                .ToListAsync();

            var query = _context.Users
                .Where(x => x.Id != userId && !swipedIds.Contains(x.Id))
                .OrderBy(x => x.CreatedAt);

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

            return ServiceResult.Ok(new
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
    }
}
