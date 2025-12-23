using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models
{
    [Table("PostMedia")]
    public class PostMedia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Url { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string MediaType { get; set; } = "image";
        // 'image' hoặc 'video'

        [ForeignKey("PostId")]
        public Post? Post { get; set; }
    }

}
