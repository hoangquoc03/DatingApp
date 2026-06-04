using System.ComponentModel.DataAnnotations;

namespace DatingApp.DTOs
{
    public class UpdateProfileDto
    {
        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        // Thông tin nâng cao
        public int? Height { get; set; }

        [StringLength(100)]
        public string? Occupation { get; set; }

        [StringLength(200)]
        public string? Education { get; set; }

        // Tính cách
        [StringLength(50)]
        public string? Zodiac { get; set; }

        [StringLength(10)]
        public string? Mbti { get; set; }

        // Lối sống
        [StringLength(50)]
        public string? Smoking { get; set; }

        [StringLength(50)]
        public string? Drinking { get; set; }

        [StringLength(50)]
        public string? LookingFor { get; set; }

        [StringLength(50)]
        public string? Lifestyle { get; set; }

        // Sở thích
        public List<string>? Interests { get; set; }
        
        public List<string>? Values { get; set; }

        public int? MaxDistance { get; set; }

        [StringLength(50)]
        public string? Vibe { get; set; }
    }
}