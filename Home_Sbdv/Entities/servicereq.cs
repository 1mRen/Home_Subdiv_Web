using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Home_Sbdv.Entities
{
    [Table("servicerequests")]
    public class ServiceRequest
    {
        [Key]
        [Column("request_id")]
        public int Req_Id { get; set; }
        
        [Required]
        [ForeignKey("Users")]
        [Column("user_id")]
        public int Userid { get; set; }
        
        [Required]
        [Column("request_type")]
        [StringLength(255)]
        public string Request_Type { get; set; } = string.Empty;
        
        [Required]
        [Column("description")]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Default to Pending
        
        [Column("submitted_at")]
        public DateTime? Submitted_at { get; set; } = DateTime.UtcNow;
        
        // New image path property
        [Column("image_path")]
        [StringLength(255)]
        public string? Image_Path { get; set; }
        
        [Column("attachment_path")]
        [StringLength(255)]
        public string? Attachment_Path { get; set; }
        
        public virtual Users? User { get; set; }
    }
}