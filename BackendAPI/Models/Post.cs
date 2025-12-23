using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendAPI.Models
{
    [Table("Posts")] // đảm bảo trùng tên bảng trong SQL
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(100)]
        public string? Title { get; set; }

        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 🔗 Quan hệ
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public ICollection<PostMedia> Media { get; set; } = new List<PostMedia>();

    }
}
