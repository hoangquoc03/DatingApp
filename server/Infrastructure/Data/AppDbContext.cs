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
        }
    }
}