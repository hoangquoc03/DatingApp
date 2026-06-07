using System;

namespace DatingApp.Desktop.Models;

public class MatchPartnerDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    private string? _avatarUrl;
    public string? AvatarUrl 
    { 
        get => _avatarUrl; 
        set => _avatarUrl = string.IsNullOrWhiteSpace(value) ? null : value; 
    }
    public string Bio { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
}

public class MatchLastMessageDto
{
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsMine { get; set; }
    public bool IsSeen { get; set; }
}

public class MatchDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public MatchPartnerDto Partner { get; set; } = new();
    public MatchLastMessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}
