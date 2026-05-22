namespace DatingApp.Models
{
    public class Match
    {
        public Guid Id { get; set; }

        public Guid User1Id { get; set; }
        public Guid User2Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}