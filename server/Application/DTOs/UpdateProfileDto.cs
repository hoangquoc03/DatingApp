using System.ComponentModel.DataAnnotations;

namespace DatingApp.DTOs 
{
    public class UpdateProfileDto
    {
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        public string? FullName { get; set; }

        [StringLength(500, ErrorMessage = "Tiểu sử không được vượt quá 500 ký tự.")]
        public string? Bio { get; set; }

        [StringLength(200, ErrorMessage = "Vị trí không được vượt quá 200 ký tự.")]
        public string? Location { get; set; }
    }
}