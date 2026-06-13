using DatingApp.Enums;

namespace DatingApp.DTOs
{
    public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public Gender Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
}