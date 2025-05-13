using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home_Sbdv.Entities
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Type { get; set; } // e.g., "Complaint", "Suggestion", "Question"

        [Required]
        public string Status { get; set; } // e.g., "Pending", "In Progress", "Resolved"

        public string? StaffResponse { get; set; }

        // Change type from string to int to match Users.Id
        public int? AssignedToId { get; set; }

        [Required]
        // Change type from string to int to match Users.Id
        public int SubmittedById { get; set; }

        public string? AttachmentPath { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties with explicit foreign key configuration
        [ForeignKey("SubmittedById")]
        public virtual Users SubmittedBy { get; set; }

        [ForeignKey("AssignedToId")]
        public virtual Users AssignedTo { get; set; }
    }
}