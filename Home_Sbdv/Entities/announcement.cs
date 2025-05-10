using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Home_Sbdv.Entities
{
    [Table("announcements")]
    public class Announcement
    {
        [Key]
        [Column("announcement_id")]
        public int AnnouncementId { get; set; }

        [Required]
        [Column("title")]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Column("posted_by")]
        public int PostedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Required]
        [Column("is_published")]
        public bool IsPublished { get; set; } = true;

        [Column("attachment_path")]
        [StringLength(255)]
        public string? AttachmentPath { get; set; }

        [Column("image_path")]
        [StringLength(255)]
        public string? ImagePath { get; set; }

        [ForeignKey("PostedBy")]
        public virtual Users? User { get; set; }
    }
}