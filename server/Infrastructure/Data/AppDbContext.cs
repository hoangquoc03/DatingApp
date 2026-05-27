using Microsoft.EntityFrameworkCore;
using DatingApp.Models;

namespace DatingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Swipe> Swipes { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Cấu hình bảng Swipe ───────────────────────────────────────────────
            modelBuilder.Entity<Swipe>()
                .HasOne(s => s.FromUser)
                .WithMany(u => u.SwipesSent)
                .HasForeignKey(s => s.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Swipe>()
                .HasOne(s => s.ToUser)
                .WithMany(u => u.SwipesReceived)
                .HasForeignKey(s => s.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 💡 TỐI ƯU: Đánh chỉ mục hỗn hợp (Composite Index) tăng tốc độ lọc người cũ trong API Discover
            modelBuilder.Entity<Swipe>()
                .HasIndex(s => new { s.FromUserId, s.ToUserId });


            // ── Cấu hình bảng Match ───────────────────────────────────────────────
            // 💡 TỐI ƯU: Khóa Unique Constraint tránh việc tạo bản ghi trùng lặp do Race Condition 
            modelBuilder.Entity<Match>()
                .HasIndex(m => new { m.User1Id, m.User2Id })
                .IsUnique();

            // 💡 FIX LỖI CRASH SWAGGER: Thiết lập cấu hình quan hệ thực thể tường minh cho bảng Match
            modelBuilder.Entity<Match>()
                .HasOne(m => m.UserOne)
                .WithMany()
                .HasForeignKey(m => m.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.UserTwo)
                .WithMany()
                .HasForeignKey(m => m.User2Id)
                .OnDelete(DeleteBehavior.Restrict);


            // ── Cấu hình bảng Message ─────────────────────────────────────────────
            modelBuilder.Entity<Message>()
                .HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(x => x.Receiver)
                .WithMany()
                .HasForeignKey(x => x.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // 💡 TỐI ƯU: Thiết lập index cho trường thời gian giúp câu lệnh OrderBy tải lịch sử chat nhanh hơn
            modelBuilder.Entity<Message>()
                 .HasIndex(msg => msg.SentAt);

            // 💡 TỐI ƯU (Dọn dẹp mục 6): Ra lệnh cho EF Core bỏ qua trường IsRead lặp chức năng, 
            // từ giờ hệ thống chỉ vận hành và theo dõi trạng thái xem qua trường IsSeen.
            modelBuilder.Entity<Message>()
                .Ignore(m => m.IsRead);
        }
    }
}