using System;

namespace DatingApp.Models
{
    public class Match
    {
        public Guid Id { get; set; }

        // Trường lưu trữ ID (Khóa ngoại)
        public Guid User1Id { get; set; }
        // 🔹 BỔ SUNG: Thuộc tính liên kết vật lý hướng về thực thể User số 1
        public User UserOne { get; set; }

        // Trường lưu trữ ID (Khóa ngoại)
        public Guid User2Id { get; set; }
        // 🔹 BỔ SUNG: Thuộc tính liên kết vật lý hướng về thực thể User số 2
        public User UserTwo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}