using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home_Sbdv.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public string Type { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; }

        public string? Link { get; set; }

        public string? Role { get; set; }

        public bool IsGlobal { get; set; }
    }
} 