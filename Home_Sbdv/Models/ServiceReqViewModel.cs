using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Home_Sbdv.Models
{
    public enum ServiceRequestStatus
    {
        Pending,
        Approved,
        Disapproved,
        InProgress,
        Completed,
        Cancelled
    }

    public class ServiceReqViewModel
    {
        [Key]
        [Column("request_id")]
        [Display(Name = "Request ID")]
        public int RequestId { get; set; }

        [Required]
        [Column("user_id")]
        [Display(Name = "User ID")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Request type is required.")]
        [StringLength(50, ErrorMessage = "Request type must not exceed 50 characters.")]
        [Column("request_type")]
        [Display(Name = "Request Type")]
        public string Request_Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        [Column("description")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column("status")]
        [Display(Name = "Status")]
        public string Status { get; set; } = ServiceRequestStatus.Pending.ToString();

        [Column("submitted_at")]
        [Display(Name = "Submitted At")]
        public DateTime? Submitted_at { get; set; }

        [NotMapped]
        [Display(Name = "Submitted By")]
        public string? SubmittedByName { get; set; }

        // Added for image storage
        [Column("image_path")]
        [Display(Name = "Proof Image")]
        public string? Image_Path { get; set; }

        // For file upload in forms
        [NotMapped]
        [Display(Name = "Upload Proof Image")]
        public IFormFile? ImageFile { get; set; }

        // Helper method to check if the request has an image
        [NotMapped]
        public bool HasImage => !string.IsNullOrEmpty(Image_Path);

        public string GetFormattedSubmissionDate()
        {
            return Submitted_at?.ToString("MMM dd, yyyy HH:mm") ?? "N/A";
        }

        public string GetStatusBadgeClass()
        {
            return Status switch
            {
                "Pending" => "bg-warning text-dark",
                "Approved" => "bg-success",
                "Disapproved" => "bg-danger",
                "InProgress" => "bg-info",
                "Completed" => "bg-success",
                "Cancelled" => "bg-secondary",
                _ => "bg-secondary"
            };
        }

        [NotMapped]
        public bool CanBeApproved => Status == ServiceRequestStatus.Pending.ToString();

        [NotMapped]
        public bool CanBeRejected => Status == ServiceRequestStatus.Pending.ToString();

        [NotMapped]
        public bool CanBeDeleted => Status == ServiceRequestStatus.Pending.ToString() ||
                                  Status == ServiceRequestStatus.Cancelled.ToString();
    }
}