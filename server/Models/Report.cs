using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatingApp.Models
{
    public class Report
    {
        public int Id { get; set; }

        public Guid ReporterId { get; set; }

        [ForeignKey("ReporterId")]
        public User Reporter { get; set; } = null!;

        public Guid ReportedUserId { get; set; }

        [ForeignKey("ReportedUserId")]
        public User ReportedUser { get; set; } = null!;

        [Required]
        public string Reason { get; set; } = string.Empty; // "fake_profile", "harassment", "inappropriate", "spam", "other"

        public string? Description { get; set; } // Chi tiết bổ sung

        public string Status { get; set; } = "pending"; // "pending", "reviewed", "resolved"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
