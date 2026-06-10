using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Helpers;
using DatingApp.Models;
using DatingApp.Enums;
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
                    Role = (int)x.Role,
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
                    x.MaxDistance,
                    x.ProfileCompletionScore
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
            if (dto.Values != null) user.Values = dto.Values;
            if (dto.Vibe != null) user.Vibe = dto.Vibe;
            if (dto.MaxDistance.HasValue) user.MaxDistance = dto.MaxDistance.Value;

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
                user.Interests,
                user.Values,
                user.Vibe,
                user.MaxDistance
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
            
            if (dto.Gender.HasValue) user.Gender = dto.Gender.Value;
            if (dto.InterestedIn.HasValue) user.InterestedIn = dto.InterestedIn.Value;
            if (dto.Height.HasValue) user.Height = dto.Height.Value;
            if (!string.IsNullOrEmpty(dto.Smoking)) user.Smoking = dto.Smoking;
            if (!string.IsNullOrEmpty(dto.Drinking)) user.Drinking = dto.Drinking;
            if (!string.IsNullOrEmpty(dto.Education)) user.Education = dto.Education;
            if (!string.IsNullOrEmpty(dto.Bio)) user.Bio = dto.Bio;

            user.IsOnboarded = true;
            CalculateCompletionScore(user);
            
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
            CalculateCompletionScore(user);
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
            int? maxDistance = null,
            bool? verifiedOnly = null,
            bool? onlineOnly = null)
        {
            pageSize = Math.Clamp(pageSize, 1, 50);
            page = Math.Max(1, page);

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (currentUser == null) return ServiceResult.NotFound("User not found");

            var swipedIds = await _context.Swipes
                .Where(x => x.FromUserId == userId)
                .Select(x => x.ToUserId)
                .ToListAsync();

            // Lấy danh sách user bị chặn (cả 2 chiều)
            var blockedIds = await _context.Blocks
                .Where(b => b.BlockerId == userId || b.BlockedUserId == userId)
                .Select(b => b.BlockerId == userId ? b.BlockedUserId : b.BlockerId)
                .ToListAsync();

            // DateOfBirth cưỡng từnh tuổi
            DateTime? dobMax = ageMin.HasValue ? DateTime.UtcNow.AddYears(-ageMin.Value) : null;
            DateTime? dobMin = ageMax.HasValue ? DateTime.UtcNow.AddYears(-ageMax.Value - 1) : null;

            // Parse Gender filter safely
            Gender? filterGenderEnum = null;
            if (!string.IsNullOrEmpty(gender))
            {
                if (int.TryParse(gender, out int genderInt) && Enum.IsDefined(typeof(Gender), genderInt))
                {
                    filterGenderEnum = (Gender)genderInt;
                }
                else if (Enum.TryParse<Gender>(gender, true, out var parsedEnum))
                {
                    filterGenderEnum = parsedEnum;
                }
            }

            var query = _context.Users
                .Where(x =>
                    x.Id != userId &&
                    !swipedIds.Contains(x.Id) &&
                    !blockedIds.Contains(x.Id) &&
                    // Filter giới tính nếu có
                    (filterGenderEnum == null || x.Gender == filterGenderEnum) &&
                    // Filter tuổi nếu có
                    (!dobMax.HasValue || x.DateOfBirth <= dobMax) &&
                    (!dobMin.HasValue || x.DateOfBirth >= dobMin) &&
                    // Filter tích xanh nếu có
                    (verifiedOnly != true || x.IsVerified)
                );

            if (onlineOnly == true)
            {
                var onlineUserIds = DatingApp.Hubs.ChatHub.GetOnlineUsers()
                    .Select(idStr => Guid.TryParse(idStr, out var id) ? id : Guid.Empty)
                    .Where(id => id != Guid.Empty)
                    .ToList();
                
                query = query.Where(x => onlineUserIds.Contains(x.Id));
            }

            query = query.OrderByDescending(x => x.ProfileCompletionScore)
                .ThenBy(x => x.CreatedAt);

            var total = await query.CountAsync();

            var usersList = await query
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
                    x.IsVerified,
                    x.Zodiac,
                    x.Mbti,
                    x.Interests,
                    x.Height,
                    x.Occupation,
                    x.Education,
                    x.Smoking,
                    x.Drinking,
                    Age = x.DateOfBirth.HasValue
                        ? (int)((DateTime.UtcNow - x.DateOfBirth.Value).TotalDays / 365.25)
                        : (int?)null,
                    IsSuperLikedBy = _context.Swipes.Any(s => s.FromUserId == x.Id && s.ToUserId == userId && s.IsSuperLike)
                })
                .ToListAsync();

            var usersWithCompatibility = usersList.Select(x => new
            {
                x.Id,
                x.FullName,
                x.Bio,
                x.AvatarUrl,
                x.Photos,
                x.Location,
                x.Gender,
                x.IsVerified,
                x.Zodiac,
                x.Mbti,
                x.Interests,
                x.Age,
                x.IsSuperLikedBy,
                x.Height,
                x.Occupation,
                x.Education,
                x.Smoking,
                x.Drinking,
                CompatibilityScore = CalculateCompatibility(currentUser, x.Interests, x.Zodiac, x.Mbti, x.Age)
            }).ToList();

            return ServiceResult.Ok(new
            {
                data = usersWithCompatibility,
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

        public async Task<ServiceResult> GetDashboardStatsAsync(Guid userId)
        {
            var likesReceived = await _context.Swipes
                .CountAsync(s => s.ToUserId == userId && s.IsLike);

            var totalMatches = await _context.Matches
                .CountAsync(m => m.User1Id == userId || m.User2Id == userId);

            // Gần đây: 5 người thích gần nhất
            var recentLikes = await _context.Swipes
                .Include(s => s.FromUser)
                .Where(s => s.ToUserId == userId && s.IsLike)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Select(s => new
                {
                    s.FromUser.Id,
                    s.FromUser.FullName,
                    s.FromUser.AvatarUrl,
                    s.FromUser.DateOfBirth,
                    Time = s.CreatedAt
                })
                .ToListAsync();

            return ServiceResult.Ok(new
            {
                likesReceived,
                totalMatches,
                recentLikes
            });
        }

        private void CalculateCompletionScore(User user)
        {
            int score = 0;
            
            // Avatar
            if (!string.IsNullOrEmpty(user.AvatarUrl)) score += 20;
            
            // Bio
            if (!string.IsNullOrEmpty(user.Bio)) score += 15;
            
            // Gender & InterestedIn
            if (user.Gender != DatingApp.Enums.Gender.Other) score += 10;
            if (user.InterestedIn.HasValue) score += 5;
            
            // Details
            if (user.Height.HasValue || !string.IsNullOrEmpty(user.Education) || !string.IsNullOrEmpty(user.Occupation)) score += 20;
            
            // Gallery photos
            if (user.Photos != null && user.Photos.Count > 0) score += 20;
            
            // Interests
            if (user.Interests != null && user.Interests.Count > 0) score += 10;
            
            user.ProfileCompletionScore = Math.Min(100, score);
        }

        public static int CalculateCompatibility(User currentUser, List<string>? targetInterests, string? targetZodiac, string? targetMbti, int? targetAge)
        {
            double totalScore = 10; // Base score out of 100

            // 1. Shared Interests (40%)
            double interestScore = 0;
            if (currentUser.Interests != null && currentUser.Interests.Any() && targetInterests != null && targetInterests.Any())
            {
                var shared = currentUser.Interests.Intersect(targetInterests, StringComparer.OrdinalIgnoreCase).Count();
                interestScore = (2.0 * shared) / (currentUser.Interests.Count + targetInterests.Count) * 40.0;
            }
            else
            {
                interestScore = 20;
            }
            totalScore += interestScore;

            // 2. MBTI Compatibility (30%)
            double mbtiScore = 0;
            if (!string.IsNullOrWhiteSpace(currentUser.Mbti) && !string.IsNullOrWhiteSpace(targetMbti))
            {
                string m1 = currentUser.Mbti.Trim().ToUpper();
                string m2 = targetMbti.Trim().ToUpper();

                if (m1.Length == 4 && m2.Length == 4)
                {
                    double matchPoints = 10; // Base
                    for (int i = 0; i < 4; i++)
                    {
                        if (m1[i] == m2[i])
                        {
                            matchPoints += 5;
                        }
                    }
                    
                    if (IsGoldenPair(m1, m2))
                    {
                        matchPoints += 10;
                    }

                    mbtiScore = Math.Min(30, matchPoints);
                }
                else
                {
                    mbtiScore = 15;
                }
            }
            else
            {
                mbtiScore = 15;
            }
            totalScore += mbtiScore;

            // 3. Zodiac Compatibility (20%)
            double zodiacScore = 0;
            if (!string.IsNullOrWhiteSpace(currentUser.Zodiac) && !string.IsNullOrWhiteSpace(targetZodiac))
            {
                string z1 = GetZodiacElement(currentUser.Zodiac);
                string z2 = GetZodiacElement(targetZodiac);

                if (z1 == "Unknown" || z2 == "Unknown")
                {
                    zodiacScore = 10;
                }
                else if (z1 == z2)
                {
                    zodiacScore = 20;
                }
                else if (AreElementsCompatible(z1, z2))
                {
                    zodiacScore = 15;
                }
                else
                {
                    zodiacScore = 5;
                }
            }
            else
            {
                zodiacScore = 10;
            }
            totalScore += zodiacScore;

            // 4. Age compatibility (up to 10 points)
            double ageScore = 0;
            if (currentUser.DateOfBirth.HasValue && targetAge.HasValue)
            {
                int currentAge = (int)((DateTime.UtcNow - currentUser.DateOfBirth.Value).TotalDays / 365.25);
                int diff = Math.Abs(currentAge - targetAge.Value);
                if (diff <= 3) ageScore = 10;
                else if (diff <= 6) ageScore = 7;
                else if (diff <= 10) ageScore = 4;
                else ageScore = 2;
            }
            else
            {
                ageScore = 5;
            }
            totalScore += ageScore;

            // Small hash variation to ensure no two scores look identical but are deterministic per user pair
            int hash = Math.Abs((currentUser.Id.GetHashCode() ^ (targetAge.HasValue ? targetAge.Value : 0).GetHashCode()) % 5);
            totalScore += hash;

            return (int)Math.Clamp(totalScore, 0, 100);
        }

        public static bool IsGoldenPair(string m1, string m2)
        {
            var goldenPairs = new (string, string)[]
            {
                ("INFJ", "ENFP"), ("INFJ", "ENTP"),
                ("ENFP", "INFJ"), ("ENTP", "INFJ"),
                ("INFP", "ENFJ"), ("INFP", "ENTJ"),
                ("ENFJ", "INFP"), ("ENTJ", "INFP"),
                ("INTJ", "ENFP"), ("INTJ", "ENTP"),
                ("INTP", "ENTJ"), ("INTP", "ENFJ"),
                ("ENTJ", "INTP"), ("ENFJ", "INTP"),
                ("ISFJ", "ESFP"), ("ISFJ", "ESTP"),
                ("ESFP", "ISFJ"), ("ESTP", "ISFJ"),
                ("ESFJ", "ISFP"), ("ESFJ", "ISTP"),
                ("ISFP", "ESFJ"), ("ISTP", "ESFJ"),
                ("ISTJ", "ESFP"), ("ISTJ", "ESTP"),
                ("ESTJ", "ISFP"), ("ESTJ", "ISTP")
            };
            return goldenPairs.Any(p => (p.Item1 == m1 && p.Item2 == m2) || (p.Item1 == m2 && p.Item2 == m1));
        }

        public static string GetZodiacElement(string zodiac)
        {
            zodiac = zodiac.Trim().ToLower();
            if (zodiac == "aries" || zodiac == "leo" || zodiac == "sagittarius" ||
                zodiac == "bạch dương" || zodiac == "sư tử" || zodiac == "nhân mã")
                return "Fire";
            if (zodiac == "taurus" || zodiac == "virgo" || zodiac == "capricorn" ||
                zodiac == "kim ngưu" || zodiac == "xử nữ" || zodiac == "ma kết")
                return "Earth";
            if (zodiac == "gemini" || zodiac == "libra" || zodiac == "aquarius" ||
                zodiac == "song tử" || zodiac == "thiên bình" || zodiac == "bảo bình")
                return "Air";
            if (zodiac == "cancer" || zodiac == "scorpio" || zodiac == "pisces" ||
                zodiac == "cự giải" || zodiac == "bọ cạp" || zodiac == "song ngư")
                return "Water";

            return "Unknown";
        }

        public static bool AreElementsCompatible(string e1, string e2)
        {
            if (e1 == "Fire" && e2 == "Air") return true;
            if (e1 == "Air" && e2 == "Fire") return true;
            if (e1 == "Earth" && e2 == "Water") return true;
            if (e1 == "Water" && e2 == "Earth") return true;
            return false;
        }
    }
}
