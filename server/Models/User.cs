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

        public bool IsVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public UserStatus Status { get; set; }

        public string? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }

        public string? PasswordResetTokenHash { get; set; }
        public DateTime? PasswordResetTokenExpiresAt { get; set; }

        public ICollection<Swipe> SwipesSent { get; set; }
        public ICollection<Swipe> SwipesReceived { get; set; }
    }
}