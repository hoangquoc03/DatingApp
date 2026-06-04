namespace DatingApp.Desktop.Models;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserProfile User { get; set; } = new();
}

public class UserProfile
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int Role { get; set; }  // 0 = User, 1 = Admin — matches backend "Role = (int)user.Role"
    public bool IsAdmin => Role == 1;
    public bool IsOnboarded { get; set; }
    public int ProfileCompletionScore { get; set; }
}
