using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home_Sbdv.Entities
{
    public class ContactDirectory
    {
        [Key]
        [Column("contact_id")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("department")]
        public string? Department { get; set; }

        [StringLength(50)]
        [Column("phone")]
        public string? Phone { get; set; }

        [StringLength(100)]
        [Column("email")]
        public string? Email { get; set; }

        [StringLength(255)]
        [Column("description")]
        public string? Description { get; set; }

        [StringLength(255)]
        [Column("photo_url")]
        public string? PhotoUrl { get; set; }
    }
} 