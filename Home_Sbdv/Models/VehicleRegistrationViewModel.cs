    using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Home_Sbdv.Entities;

namespace Home_Sbdv.Models
{
    public class VehicleRegistrationViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Plate number is required.")]
        [StringLength(20, ErrorMessage = "Plate number must not exceed 20 characters.")]
        [Display(Name = "Plate Number")]
        public string PlateNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brand is required.")]
        [StringLength(50, ErrorMessage = "Brand must not exceed 50 characters.")]
        [Display(Name = "Brand")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required.")]
        [StringLength(50, ErrorMessage = "Model must not exceed 50 characters.")]
        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Color is required.")]
        [StringLength(30, ErrorMessage = "Color must not exceed 30 characters.")]
        [Display(Name = "Color")]
        public string Color { get; set; } = string.Empty;

        [Display(Name = "OR Document")]
        public string? ORDocumentPath { get; set; }

        [Display(Name = "CR Document")]
        public string? CRDocumentPath { get; set; }

        [Display(Name = "ID Document")]
        public string? IDDocumentPath { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Requested At")]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Requested By User ID")]
        public int RequestedByUserId { get; set; }

        [Display(Name = "Requested By")]
        public string? RequestedByUserName { get; set; }

        [Display(Name = "Approved By User ID")]
        public int? ApprovedByUserId { get; set; }

        [Display(Name = "Approved By")]
        public string? ApprovedByUserName { get; set; }

        [Display(Name = "Approved At")]
        public DateTime? ApprovedAt { get; set; }

        [Display(Name = "Approval Notes")]
        public string? ApprovalNotes { get; set; }

        // Helper properties for document display
        /*
        [NotMapped]
        public string DisplayORDocumentPath => !string.IsNullOrEmpty(ORDocumentPath) ? ORDocumentPath : "#";

        [NotMapped]
        public string DisplayCRDocumentPath => !string.IsNullOrEmpty(CRDocumentPath) ? CRDocumentPath : "#";

        [NotMapped]
        public string DisplayIDDocumentPath => !string.IsNullOrEmpty(IDDocumentPath) ? IDDocumentPath : "#";

        // File upload properties
        [NotMapped]
        [Required(ErrorMessage = "OR document is required.")]
        [Display(Name = "OR Document")]
        public IFormFile? ORDocumentFile { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "CR document is required.")]
        [Display(Name = "CR Document")]
        public IFormFile? CRDocumentFile { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "ID document is required.")]
        [Display(Name = "ID Document")]
        public IFormFile? IDDocumentFile { get; set; }

        // Navigation properties
        [NotMapped]
        public Users? RequestedByUser { get; set; }

        [NotMapped]
        public Users? ApprovedByUser { get; set; }

        // Status constants
        public const string STATUS_PENDING = "Pending";
        public const string STATUS_APPROVED = "Approved";
        public const string STATUS_REJECTED = "Rejected";
        public const string STATUS_CANCELLED = "Cancelled";

        // Helper methods
        [NotMapped]
        public bool IsPending => Status == STATUS_PENDING;

        [NotMapped]
        public bool IsApproved => Status == STATUS_APPROVED;

        [NotMapped]
        public bool IsRejected => Status == STATUS_REJECTED;

        [NotMapped]
        public bool IsCancelled => Status == STATUS_CANCELLED;

        [NotMapped]
        public bool CanBeEdited => IsPending;

        [NotMapped]
        public bool CanBeCancelled => IsPending;

        [NotMapped]
        public bool CanBeApproved => IsPending;

        [NotMapped]
        public bool CanBeRejected => IsPending;

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            STATUS_APPROVED => "success",
            STATUS_REJECTED => "danger",
            STATUS_CANCELLED => "secondary",
            _ => "warning"
        };
    }*/
    }

    public class VehicleRegistrationListViewModel
    {
        /// <summary>
        /// List of vehicle registration items to display
        /// </summary>
        public List<VehicleRegistrationViewModel> Registrations { get; set; } = new List<VehicleRegistrationViewModel>();

        /// <summary>
        /// Total count of all registrations
        /// </summary>
        public int TotalRegistrations { get; set; }

        /// <summary>
        /// Count of pending registrations
        /// </summary>
        public int PendingRegistrations { get; set; }

        /// <summary>
        /// Count of approved registrations
        /// </summary>
        public int ApprovedRegistrations { get; set; }

        /// <summary>
        /// Count of rejected registrations
        /// </summary>
        public int RejectedRegistrations { get; set; }

        /// <summary>
        /// Count of cancelled registrations
        /// </summary>
        public int CancelledRegistrations { get; set; }

        /// <summary>
        /// Monthly registration data for chart
        /// </summary>
        public List<MonthlyRegistrationData> MonthlyRegistrations { get; set; } = new List<MonthlyRegistrationData>();

        /// <summary>
        /// Search term for filtering registrations
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Status filter for filtering registrations
        /// </summary>
        public string? StatusFilter { get; set; }

        /// <summary>
        /// Start date for filtering registrations
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// End date for filtering registrations
        /// </summary>
        public DateTime? ToDate { get; set; }
    }

    public class MonthlyRegistrationData
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class VehicleRegistrationStatisticsViewModel
    {
        /// <summary>
        /// Total count of all registrations
        /// </summary>
        public int TotalRegistrations { get; set; }

        /// <summary>
        /// Count of pending registrations
        /// </summary>
        public int PendingRegistrations { get; set; }

        /// <summary>
        /// Count of approved registrations
        /// </summary>
        public int ApprovedRegistrations { get; set; }

        /// <summary>
        /// Count of rejected registrations
        /// </summary>
        public int RejectedRegistrations { get; set; }

        /// <summary>
        /// Count of cancelled registrations
        /// </summary>
        public int CancelledRegistrations { get; set; }

        /// <summary>
        /// Distribution of registrations by brand
        /// </summary>
        public Dictionary<string, int> BrandDistribution { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Monthly trends of registration submissions
        /// </summary>
        public Dictionary<string, int> MonthlyTrends { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Monthly registration data for chart
        /// </summary>
        public List<MonthlyRegistrationData> MonthlyRegistrations { get; set; } = new List<MonthlyRegistrationData>();

        /// <summary>
        /// Top vehicle brands with their counts
        /// </summary>
        public Dictionary<string, int> TopBrands { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Distribution of registrations by status
        /// </summary>
        public Dictionary<string, int> StatusDistribution { get; set; } = new Dictionary<string, int>();
    }

    public class VehicleRegistrationApprovalViewModel
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "Approval notes are required")]
        [Display(Name = "Approval Notes")]
        public string ApprovalNotes { get; set; } = string.Empty;

        /// <summary>
        /// ID of the registration being approved/rejected
        /// </summary>
        public int RegistrationId { get; set; }

        /// <summary>
        /// ID of the user performing the approval/rejection
        /// </summary>
        public int ApproverId { get; set; }
    }
}