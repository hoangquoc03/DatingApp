using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatingApp.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty; // "NewLike", "NewMatch", "System"

        public Guid? RelatedUserId { get; set; } // Người trigger thông báo này

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
