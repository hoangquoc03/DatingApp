using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DatingApp.Desktop.Models;

public partial class MatchPartnerDto : ObservableObject
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

    [ObservableProperty]
    private bool _isOnline;

    public bool IsVerified { get; set; }
}

public partial class MatchLastMessageDto : ObservableObject
{
    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private DateTime _sentAt;

    [ObservableProperty]
    private bool _isMine;

    [ObservableProperty]
    private bool _isSeen;
}

public partial class MatchDto : ObservableObject
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }

    [ObservableProperty]
    private MatchPartnerDto _partner = new();

    [ObservableProperty]
    private MatchLastMessageDto? _lastMessage;

    [ObservableProperty]
    private int _unreadCount;
}
