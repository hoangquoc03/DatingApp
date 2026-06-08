using System;
using System.Collections.Generic;

namespace DatingApp.Desktop.Models;

public class RelatedUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    private string? _avatarUrl;
    public string? AvatarUrl 
    { 
        get => _avatarUrl; 
        set => _avatarUrl = string.IsNullOrWhiteSpace(value) ? null : value; 
    }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid? RelatedUserId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public RelatedUserDto? RelatedUser { get; set; }

    public string TimeAgo
    {
        get
        {
            var diff = DateTime.UtcNow - CreatedAt.ToUniversalTime();
            if (diff.TotalDays >= 1) return $"{(int)diff.TotalDays} ngày trước";
            if (diff.TotalHours >= 1) return $"{(int)diff.TotalHours} giờ trước";
            if (diff.TotalMinutes >= 1) return $"{(int)diff.TotalMinutes} phút trước";
            return "Vừa xong";
        }
    }
}

public class NotificationResponse
{
    public List<NotificationDto> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
}
