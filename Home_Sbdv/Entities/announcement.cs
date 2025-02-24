using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home_Sbdv.Entities
{
    public class Announcement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("announcement_id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("title")]
        public string Title { get; set; } = "No Title";

        [Required]
        [Column("content")]
        public string Content { get; set; } = "No Content";

        [Required]
        [Column("posted_by")]
        public int PostedBy { get; set; } // Foreign key for Users

        [ForeignKey("PostedBy")]
        public virtual Users? User { get; set; } // Navigation Property

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }


    }
}
    