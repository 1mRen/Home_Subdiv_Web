using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Home_Sbdv.Entities;

namespace Home_Sbdv.Models
{
    public class FeedbackViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title must not exceed 100 characters.")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Feedback type is required.")]
        [Display(Name = "Feedback Type")]
        public string Type { get; set; } = "Suggestion";

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Staff Response")]
        public string? StaffResponse { get; set; }

        [Display(Name = "Assigned To")]
        public int? AssignedToId { get; set; }

        [Display(Name = "Submitted By")]
        public int SubmittedById { get; set; }

        [Display(Name = "Submitted By Name")]
        public string? SubmittedByName { get; set; }

        [Display(Name = "Assigned To Name")]
        public string? AssignedToName { get; set; }

        [Display(Name = "Attachment")]
        public string? AttachmentPath { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated Date")]
        public DateTime? UpdatedAt { get; set; }

        // Helper property for attachment display
        [NotMapped]
        public string DisplayAttachmentPath => !string.IsNullOrEmpty(AttachmentPath) ? AttachmentPath : "#";

        [NotMapped]
        public IFormFile? AttachmentFile { get; set; }

        // Navigation properties
        [NotMapped]
        public Users? SubmittedBy { get; set; }

        [NotMapped]
        public Users? AssignedTo { get; set; }
    }

    public class FeedbackListViewModel
    {
        /// <summary>
        /// List of feedback items to display
        /// </summary>
        public List<FeedbackViewModel> Feedbacks { get; set; } = new List<FeedbackViewModel>();

        /// <summary>
        /// Total count of all feedbacks
        /// </summary>
        public int TotalFeedbacks { get; set; }

        /// <summary>
        /// Count of pending feedbacks
        /// </summary>
        public int PendingFeedbacks { get; set; }

        /// <summary>
        /// Count of in-progress feedbacks
        /// </summary>
        public int InProgressFeedbacks { get; set; }

        /// <summary>
        /// Count of resolved feedbacks
        /// </summary>
        public int ResolvedFeedbacks { get; set; }
    }

    /// <summary>
    /// ViewModel for statistics dashboard, used in Statistics view
    /// </summary>
    public class FeedbackStatisticsViewModel
    {
        /// <summary>
        /// Total count of all feedbacks
        /// </summary>
        public int TotalFeedbacks { get; set; }

        /// <summary>
        /// Count of pending feedbacks
        /// </summary>
        public int PendingFeedbacks { get; set; }

        /// <summary>
        /// Count of in-progress feedbacks
        /// </summary>
        public int InProgressFeedbacks { get; set; }

        /// <summary>
        /// Count of resolved feedbacks
        /// </summary>
        public int ResolvedFeedbacks { get; set; }

        /// <summary>
        /// Distribution of feedback by type (e.g., Suggestion, Complaint, etc.)
        /// </summary>
        public Dictionary<string, int> TypeDistribution { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Monthly trends of feedback submissions
        /// </summary>
        public Dictionary<string, int> MonthlyTrends { get; set; } = new Dictionary<string, int>();
    }

    public class FeedbackResponseViewModel
    {
        [Required]
        public string Status { get; set; }

        [Required(ErrorMessage = "Response is required")]
        public string StaffResponse { get; set; }
    }
}