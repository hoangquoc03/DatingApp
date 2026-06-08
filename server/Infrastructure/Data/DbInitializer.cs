using DatingApp.Models;
using DatingApp.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatingApp.Data
{
    public static class DbInitializer
    {
        public static async Task SeedData(AppDbContext context)
        {
            if (await context.Users.CountAsync() > 2)
            {
                return; // Đã có đủ dữ liệu người dùng
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword("password");

            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "thaolinh@gmail.com",
                    PasswordHash = passwordHash,
                    FullName = "Nguyễn Thảo Linh",
                    Gender = Gender.Female,
                    InterestedIn = Gender.Male,
                    DateOfBirth = DateTime.UtcNow.AddYears(-22),
                    Bio = "Thích cà phê vỉa hè và những bản nhạc Indie nhẹ nhàng. Tìm kiếm một người bạn đồng hành chân thành.",
                    AvatarUrl = "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?q=80&w=600&auto=format&fit=crop",
                    Location = "Hà Nội",
                    Zodiac = "Song Ngư",
                    Mbti = "ENFP",
                    Interests = new List<string> { "Du lịch", "Cà phê", "Âm nhạc", "Vẽ tranh" },
                    IsVerified = true,
                    IsOnboarded = true,
                    IsActive = true,
                    Status = UserStatus.Active,
                    Role = Role.User,
                    ProfileCompletionScore = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "minhquan@gmail.com",
                    PasswordHash = passwordHash,
                    FullName = "Trần Minh Quân",
                    Gender = Gender.Male,
                    InterestedIn = Gender.Female,
                    DateOfBirth = DateTime.UtcNow.AddYears(-25),
                    Bio = "Software Engineer. Đam mê công nghệ, thích đọc sách khoa học viễn tưởng và xem phim trinh thám.",
                    AvatarUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?q=80&w=600&auto=format&fit=crop",
                    Location = "TP. Hồ Chí Minh",
                    Zodiac = "Ma Kết",
                    Mbti = "INTJ",
                    Interests = new List<string> { "Đọc sách", "Thể thao", "Xem phim", "Công nghệ" },
                    IsVerified = true,
                    IsOnboarded = true,
                    IsActive = true,
                    Status = UserStatus.Active,
                    Role = Role.User,
                    ProfileCompletionScore = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "quinhtrang@gmail.com",
                    PasswordHash = passwordHash,
                    FullName = "Lê Quỳnh Trang",
                    Gender = Gender.Female,
                    InterestedIn = Gender.Male,
                    DateOfBirth = DateTime.UtcNow.AddYears(-21),
                    Bio = "Nhiếp ảnh gia tự do. Mình yêu thiên nhiên, thích nấu ăn và đi du lịch bụi.",
                    AvatarUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?q=80&w=600&auto=format&fit=crop",
                    Location = "Đà Nẵng",
                    Zodiac = "Cự Giải",
                    Mbti = "INFJ",
                    Interests = new List<string> { "Nhiếp ảnh", "Nấu ăn", "Du lịch", "Cà phê" },
                    IsVerified = false,
                    IsOnboarded = true,
                    IsActive = true,
                    Status = UserStatus.Active,
                    Role = Role.User,
                    ProfileCompletionScore = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "ducanh@gmail.com",
                    PasswordHash = passwordHash,
                    FullName = "Phạm Đức Anh",
                    Gender = Gender.Male,
                    InterestedIn = Gender.Female,
                    DateOfBirth = DateTime.UtcNow.AddYears(-28),
                    Bio = "Kinh doanh tự do. Thích tập gym, đi du lịch nghỉ dưỡng và trò chuyện về startup.",
                    AvatarUrl = "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?q=80&w=600&auto=format&fit=crop",
                    Location = "Hà Nội",
                    Zodiac = "Sư Tử",
                    Mbti = "ENTJ",
                    Interests = new List<string> { "Kinh doanh", "Gym", "Du lịch", "Xem phim" },
                    IsVerified = true,
                    IsOnboarded = true,
                    IsActive = true,
                    Status = UserStatus.Active,
                    Role = Role.User,
                    ProfileCompletionScore = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "myduyen@gmail.com",
                    PasswordHash = passwordHash,
                    FullName = "Hoàng Mỹ Duyên",
                    Gender = Gender.Female,
                    InterestedIn = Gender.Male,
                    DateOfBirth = DateTime.UtcNow.AddYears(-24),
                    Bio = "Yêu thích cái đẹp, thời trang và làm đẹp. Rất thích tụ tập cà phê tán gẫu cuối tuần cùng bạn bè.",
                    AvatarUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?q=80&w=600&auto=format&fit=crop",
                    Location = "TP. Hồ Chí Minh",
                    Zodiac = "Thiên Bình",
                    Mbti = "ESFJ",
                    Interests = new List<string> { "Mua sắm", "Ẩm thực", "Cà phê", "Làm đẹp" },
                    IsVerified = false,
                    IsOnboarded = true,
                    IsActive = true,
                    Status = UserStatus.Active,
                    Role = Role.User,
                    ProfileCompletionScore = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "thanhlam@gmail.com",
                    PasswordHash = passwordHash,
                    FullName = "Vũ Thanh Lâm",
                    Gender = Gender.Male,
                    InterestedIn = Gender.Female,
                    DateOfBirth = DateTime.UtcNow.AddYears(-23),
                    Bio = "Thích chơi game, nghe nhạc lo-fi và lập trình. Tìm kiếm một người có thể cùng ngồi im lặng ngắm mưa.",
                    AvatarUrl = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?q=80&w=600&auto=format&fit=crop",
                    Location = "Hà Nội",
                    Zodiac = "Bảo Bình",
                    Mbti = "INTP",
                    Interests = new List<string> { "Xem phim", "Game", "Công nghệ", "Âm nhạc" },
                    IsVerified = false,
                    IsOnboarded = true,
                    IsActive = true,
                    Status = UserStatus.Active,
                    Role = Role.User,
                    ProfileCompletionScore = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "khanhvy@gmail.com",
                    PasswordHash = passwordHash,
                    FullName = "Đỗ Khánh Vy",
                    Gender = Gender.Female,
                    InterestedIn = Gender.Male,
                    DateOfBirth = DateTime.UtcNow.AddYears(-20),
                    Bio = "Sinh viên Luật. Thích viết lách, đọc sách bên tách trà ấm. Mơ mộng và nhạy cảm.",
                    AvatarUrl = "https://images.unsplash.com/photo-1517841905240-472988babdf9?q=80&w=600&auto=format&fit=crop",
                    Location = "Hà Nội",
                    Zodiac = "Bọ Cạp",
                    Mbti = "INFP",
                    Interests = new List<string> { "Viết lách", "Âm nhạc", "Đọc sách", "Cà phê" },
                    IsVerified = true,
                    IsOnboarded = true,
                    IsActive = true,
                    Status = UserStatus.Active,
                    Role = Role.User,
                    ProfileCompletionScore = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "hoangnam@gmail.com",
                    PasswordHash = passwordHash,
                    FullName = "Nguyễn Hoàng Nam",
                    Gender = Gender.Male,
                    InterestedIn = Gender.Female,
                    DateOfBirth = DateTime.UtcNow.AddYears(-26),
                    Bio = "Thích thể thao mạo hiểm, phượt đường dài và thử thách bản thân. Sống năng động.",
                    AvatarUrl = "https://images.unsplash.com/photo-1492562080023-ab3db95bfbce?q=80&w=600&auto=format&fit=crop",
                    Location = "TP. Hồ Chí Minh",
                    Zodiac = "Bạch Dương",
                    Mbti = "ESTP",
                    Interests = new List<string> { "Thể thao", "Xe cộ", "Du lịch", "Gym" },
                    IsVerified = true,
                    IsOnboarded = true,
                    IsActive = true,
                    Status = UserStatus.Active,
                    Role = Role.User,
                    ProfileCompletionScore = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "tuyetmai@gmail.com",
                    PasswordHash = passwordHash,
                    FullName = "Phan Tuyết Mai",
                    Gender = Gender.Female,
                    InterestedIn = Gender.Male,
                    DateOfBirth = DateTime.UtcNow.AddYears(-26),
                    Bio = "Thích các hoạt động tình nguyện, yêu động vật, tập yoga hằng ngày và nấu ăn tốt lành.",
                    AvatarUrl = "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?q=80&w=600&auto=format&fit=crop",
                    Location = "Đà Nẵng",
                    Zodiac = "Xử Nữ",
                    Mbti = "ENFJ",
                    Interests = new List<string> { "Thiện nguyện", "Yoga", "Ẩm thực", "Trà sữa" },
                    IsVerified = false,
                    IsOnboarded = true,
                    IsActive = true,
                    Status = UserStatus.Active,
                    Role = Role.User,
                    ProfileCompletionScore = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "anhtuan@gmail.com",
                    PasswordHash = passwordHash,
                    FullName = "Bùi Anh Tuấn",
                    Gender = Gender.Male,
                    InterestedIn = Gender.Female,
                    DateOfBirth = DateTime.UtcNow.AddYears(-27),
                    Bio = "Đam mê leo núi, dã ngoại cuối tuần. Thích chụp ảnh phong cảnh thiên nhiên.",
                    AvatarUrl = "https://images.unsplash.com/photo-1489980508314-941910ded1f4?q=80&w=600&auto=format&fit=crop",
                    Location = "TP. Hồ Chí Minh",
                    Zodiac = "Kim Ngưu",
                    Mbti = "ISTP",
                    Interests = new List<string> { "Nhiếp ảnh", "Leo núi", "Phượt", "Cà phê" },
                    IsVerified = true,
                    IsOnboarded = true,
                    IsActive = true,
                    Status = UserStatus.Active,
                    Role = Role.User,
                    ProfileCompletionScore = 100,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }
    }
}
