using System.ComponentModel.DataAnnotations.Schema;

namespace DatingApp.Models
{
    public class Block
    {
        public int Id { get; set; }

        public Guid BlockerId { get; set; }

        [ForeignKey("BlockerId")]
        public User Blocker { get; set; } = null!;

        public Guid BlockedUserId { get; set; }

        [ForeignKey("BlockedUserId")]
        public User BlockedUser { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
