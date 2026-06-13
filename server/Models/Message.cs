namespace DatingApp.Models
{
    public class Message
    {
        public Guid Id { get; set; }

        public Guid SenderId { get; set; }
        public User Sender { get; set; } = null!;

        public Guid ReceiverId { get; set; }
        public User Receiver { get; set; } = null!;

        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }  // null = tin nhắn text thuần
        public bool IsSeen { get; set; } = false;
        public DateTime? SeenAt { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}