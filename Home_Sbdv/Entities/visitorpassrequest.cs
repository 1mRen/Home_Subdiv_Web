using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home_Sbdv.Entities
{
    [Table("visitorpassrequests")]
    public class VisitorPassRequest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("requested_by_user_id")]
        public int RequestedByUserId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("visitor_name")]
        public string VisitorName { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("visitor_contact")]
        public string? VisitorContact { get; set; }

        [StringLength(255)]
        [Column("purpose")]
        public string? Purpose { get; set; }

        [Column("visit_date")]
        public DateTime VisitDate { get; set; }

        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Declined, CheckedIn, CheckedOut

        [Column("requested_at")]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [Column("approved_by_user_id")]
        public int? ApprovedByUserId { get; set; }

        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }

        [Column("audit_trail")]
        public string? AuditTrail { get; set; }

        // Navigation property
        [ForeignKey("RequestedByUserId")]
        public virtual Users RequestedBy { get; set; }

        // You can add this navigation property if you have a relationship with the approver user
         [ForeignKey("ApprovedByUserId")]
        public virtual Users ApprovedBy { get; set; }
    }
}