using System.ComponentModel.DataAnnotations;
using DatingApp.Enums;

namespace DatingApp.Models
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string FullName { get; set; }

        public Gender Gender { get; set; }

        public Gender? InterestedIn { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string Bio { get; set; }

        public string AvatarUrl { get; set; }

        public string Location { get; set; }

        // Thông tin nâng cao
        public int? Height { get; set; }
        public string? Occupation { get; set; }
        public string? Education { get; set; }

        // Tính cách
        public string? Zodiac { get; set; }
        public string? Mbti { get; set; }

        // Lối sống
        public string? Smoking { get; set; }
        public string? Drinking { get; set; }

        // Onboarding fields
        public string? LookingFor { get; set; }
        public List<string>? Interests { get; set; } = new List<string>();
        public string? Lifestyle { get; set; }
        public List<string>? Values { get; set; } = new List<string>();
        public int? MaxDistance { get; set; }
        public string? Vibe { get; set; }
        public bool IsOnboarded { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public UserStatus Status { get; set; }

        public Role Role { get; set; } = Role.User;
        public bool IsActive { get; set; } = true;

        public string? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }

        public string? PasswordResetTokenHash { get; set; }
        public DateTime? PasswordResetTokenExpiresAt { get; set; }

        public ICollection<Photo> Photos { get; set; } = new List<Photo>();

        public ICollection<Swipe> SwipesSent { get; set; }
        public ICollection<Swipe> SwipesReceived { get; set; }
    }
}