using System;

namespace DatingApp.Desktop.Models;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    private string? _avatarUrl;
    public string? AvatarUrl 
    { 
        get => _avatarUrl; 
        set => _avatarUrl = string.IsNullOrWhiteSpace(value) ? null : value; 
    }
    public int Role { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public bool IsOnline { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsOnboarded { get; set; }
    public int ProfileCompletionScore { get; set; }
    
    // --- Profile Fields ---
    public string Bio { get; set; } = string.Empty;
    public int Gender { get; set; } 
    public DateTime? DateOfBirth { get; set; }
    public int? InterestedIn { get; set; }
    public int? Height { get; set; }
    public string Occupation { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    
    // Tính cách
    public string Zodiac { get; set; } = string.Empty;
    public string Mbti { get; set; } = string.Empty;

    // Lối sống
    public string Smoking { get; set; } = string.Empty;
    public string Drinking { get; set; } = string.Empty;

    // Onboarding fields
    public string LookingFor { get; set; } = string.Empty;
    public System.Collections.Generic.List<string> Interests { get; set; } = new();
    public string Lifestyle { get; set; } = string.Empty;
    public System.Collections.Generic.List<string> Values { get; set; } = new();
    public int? MaxDistance { get; set; }
    public string Vibe { get; set; } = string.Empty;
    
    public string Location { get; set; } = string.Empty;

    public System.Collections.Generic.List<PhotoDto> Photos { get; set; } = new();

    public string RoleDisplay => Role == 1 ? "Admin" : "User";
    public string StatusDisplay => IsActive ? "Hoạt động" : "Bị Khóa";
}
