using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatingApp.Models
{
    public class Photo
    {
        public int Id { get; set; }

        [Required]
        public string Url { get; set; }

        public bool IsMain { get; set; }

        public string? PublicId { get; set; } // Để xoá ảnh trên Cloudinary

        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
