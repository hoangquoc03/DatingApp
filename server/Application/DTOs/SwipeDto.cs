namespace DatingApp.DTOs
{
    public class SwipeDto
    {
        public Guid ToUserId { get; set; }
        public bool IsLike { get; set; }
        public bool IsSuperLike { get; set; } = false;
    }
}