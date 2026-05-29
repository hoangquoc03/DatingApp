using DatingApp.Enums;

namespace DatingApp.DTOs
{
    public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }

    public Gender Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
}