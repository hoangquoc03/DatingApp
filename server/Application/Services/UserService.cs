using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Helpers;
using DatingApp.Models;
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
                    Photos = x.Photos.Select(p => new { p.Id, p.Url, p.IsMain }).ToList(),
                    x.Location,
                    x.Gender,
                    x.DateOfBirth,
                    x.IsVerified,
                    x.IsOnboarded,
                    x.CreatedAt,
                    // Nâng cao
                    x.Height,
                    x.Occupation,
                    x.Education,
                    x.Zodiac,
                    x.Mbti,
                    x.Smoking,
                    x.Drinking,
                    // Onboarding
                    x.LookingFor,
                    x.Lifestyle,
                    x.Interests,
                    x.Values,
                    x.Vibe,
                    x.MaxDistance
                })
                .FirstOrDefaultAsync();

            if (user == null) return ServiceResult.NotFound("User not found");

            return ServiceResult.Ok(user);
        }

        public async Task<ServiceResult> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return ServiceResult.NotFound("User not found");

            if (!string.IsNullOrWhiteSpace(dto.FullName)) user.FullName = dto.FullName.Trim();
            if (dto.Bio != null) user.Bio = dto.Bio.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Location)) user.Location = dto.Location.Trim();

            // Nâng cao
            if (dto.Height.HasValue) user.Height = dto.Height;
            if (dto.Occupation != null) user.Occupation = dto.Occupation.Trim();
            if (dto.Education != null) user.Education = dto.Education.Trim();

            // Tính cách
            if (dto.Zodiac != null) user.Zodiac = dto.Zodiac.Trim();
            if (dto.Mbti != null) user.Mbti = dto.Mbti.Trim();

            // Lối sống
            if (dto.Smoking != null) user.Smoking = dto.Smoking;
            if (dto.Drinking != null) user.Drinking = dto.Drinking;
            if (dto.LookingFor != null) user.LookingFor = dto.LookingFor;
            if (dto.Lifestyle != null) user.Lifestyle = dto.Lifestyle;
            if (dto.Interests != null) user.Interests = dto.Interests;

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
                user.IsOnboarded,
                user.Height,
                user.Occupation,
                user.Education,
                user.Zodiac,
                user.Mbti,
                user.Smoking,
                user.Drinking,
                user.LookingFor,
                user.Lifestyle,
                user.Interests
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

                var uploadResult = await _cloudinary.UploadImageAsync(file, $"avatars/{userId}");
                newAvatarUrl = uploadResult.Url;
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

        public async Task<ServiceResult> DiscoverAsync(
            Guid userId,
            int page,
            int pageSize,
            int? ageMin = null,
            int? ageMax = null,
            string? gender = null,
            int? maxDistance = null)
        {
            pageSize = Math.Clamp(pageSize, 1, 50);
            page = Math.Max(1, page);

            var swipedIds = await _context.Swipes
                .Where(x => x.FromUserId == userId)
                .Select(x => x.ToUserId)
                .ToListAsync();

            // DateOfBirth cưỡng từnh tuổi
            DateTime? dobMax = ageMin.HasValue ? DateTime.UtcNow.AddYears(-ageMin.Value) : null;
            DateTime? dobMin = ageMax.HasValue ? DateTime.UtcNow.AddYears(-ageMax.Value - 1) : null;

            var query = _context.Users
                .Where(x =>
                    x.Id != userId &&
                    !swipedIds.Contains(x.Id) &&
                    // Filter giới tính nếu có
                    (gender == null || x.Gender.ToString().ToLower() == gender.ToLower()) &&
                    // Filter tuổi nếu có
                    (!dobMax.HasValue || x.DateOfBirth <= dobMax) &&
                    (!dobMin.HasValue || x.DateOfBirth >= dobMin)
                )
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
                    Photos = x.Photos.Select(p => new { p.Id, p.Url, p.IsMain }).ToList(),
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
                },
                filters = new { ageMin, ageMax, gender, maxDistance }
            });
        }
        // ─── PHOTO GALLERY ────────────────────────────────────────────────────────

        public async Task<ServiceResult> AddPhotoAsync(Guid userId, IFormFile file)
        {
            var user = await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return ServiceResult.NotFound("User not found");

            if (user.Photos.Count >= 6)
                return ServiceResult.BadRequest("Bạn chỉ có thể upload tối đa 6 ảnh");

            try
            {
                var uploadResult = await _cloudinary.UploadImageAsync(file, $"gallery/{userId}");
                
                var photo = new Photo
                {
                    Url = uploadResult.Url,
                    PublicId = uploadResult.PublicId,
                    IsMain = user.Photos.Count == 0,
                    UserId = userId
                };

                if (photo.IsMain)
                {
                    user.AvatarUrl = photo.Url;
                }

                _context.Photos.Add(photo);
                await _context.SaveChangesAsync();

                return ServiceResult.Ok(new { photo.Id, photo.Url, photo.IsMain });
            }
            catch (Exception ex)
            {
                return ServiceResult.BadRequest(ex.Message);
            }
        }

        public async Task<ServiceResult> DeletePhotoAsync(Guid userId, int photoId)
        {
            var user = await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return ServiceResult.NotFound("User not found");

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null) return ServiceResult.NotFound("Không tìm thấy ảnh");

            if (photo.IsMain)
                return ServiceResult.BadRequest("Không thể xoá ảnh đại diện chính");

            if (!string.IsNullOrEmpty(photo.PublicId))
            {
                await _cloudinary.DeleteImageAsync(photo.PublicId);
            }

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return ServiceResult.Ok("Đã xoá ảnh");
        }

        public async Task<ServiceResult> SetMainPhotoAsync(Guid userId, int photoId)
        {
            var user = await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return ServiceResult.NotFound("User not found");

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null) return ServiceResult.NotFound("Không tìm thấy ảnh");

            if (photo.IsMain) return ServiceResult.BadRequest("Ảnh này đã là ảnh chính");

            var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);
            if (currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;
            user.AvatarUrl = photo.Url;

            await _context.SaveChangesAsync();

            return ServiceResult.Ok("Đã đổi ảnh đại diện");
        }
    }
}
