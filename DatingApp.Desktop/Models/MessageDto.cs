using System;

namespace DatingApp.Desktop.Models;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    private string? _imageUrl;
    public string? ImageUrl 
    { 
        get => _imageUrl; 
        set => _imageUrl = string.IsNullOrWhiteSpace(value) ? null : value; 
    }
    public bool IsSeen { get; set; }
    public DateTime? SeenAt { get; set; }
    public DateTime SentAt { get; set; }
    
    // UI Helpers
    public bool IsMine { get; set; }
    public bool HasImage => !string.IsNullOrEmpty(ImageUrl);
    public bool HasContent => !string.IsNullOrEmpty(Content);
}
