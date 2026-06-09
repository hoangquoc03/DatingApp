using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DatingApp.Desktop.Models;

public partial class MessageDto : ObservableObject
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasContent))]
    private string _content = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImage))]
    private string? _imageUrl;

    [ObservableProperty]
    private bool _isSeen;

    public DateTime? SeenAt { get; set; }
    public DateTime SentAt { get; set; }
    
    // UI Helpers
    public bool IsMine { get; set; }
    public bool HasImage => !string.IsNullOrEmpty(ImageUrl);
    public bool HasContent => !string.IsNullOrEmpty(Content);
}
