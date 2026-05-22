namespace DatingApp.Models
{
    public class Swipe
    {
        public Guid Id { get; set; }

        public Guid FromUserId { get; set; }
        public User FromUser { get; set; }

        public Guid ToUserId { get; set; }
        public User ToUser { get; set; }

        public bool IsLike { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}