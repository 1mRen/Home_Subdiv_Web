using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Home_Sbdv.Models
{
    public class VehicleRegistrationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Plate number is required")]
        [Display(Name = "Plate Number")]
        [StringLength(20, ErrorMessage = "Plate number cannot exceed 20 characters")]
        public string PlateNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brand is required")]
        [Display(Name = "Brand")]
        [StringLength(50, ErrorMessage = "Brand cannot exceed 50 characters")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required")]
        [Display(Name = "Model")]
        [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Color is required")]
        [Display(Name = "Color")]
        [StringLength(30, ErrorMessage = "Color cannot exceed 30 characters")]
        public string Color { get; set; } = string.Empty;

        [Required(ErrorMessage = "Official Receipt (OR) is required")]
        [Display(Name = "Official Receipt (OR)")]
        [FileExtensions(Extensions = "pdf,jpg,jpeg,png", ErrorMessage = "Only PDF, JPG, JPEG, and PNG files are allowed")]
        public IFormFile ORDocument { get; set; }

        [Required(ErrorMessage = "Certificate of Registration (CR) is required")]
        [Display(Name = "Certificate of Registration (CR)")]
        [FileExtensions(Extensions = "pdf,jpg,jpeg,png", ErrorMessage = "Only PDF, JPG, JPEG, and PNG files are allowed")]
        public IFormFile CRDocument { get; set; }

        [Required(ErrorMessage = "Valid ID is required")]
        [Display(Name = "Valid ID")]
        [FileExtensions(Extensions = "pdf,jpg,jpeg,png", ErrorMessage = "Only PDF, JPG, JPEG, and PNG files are allowed")]
        public IFormFile IDDocument { get; set; }

        public string ORDocumentPath { get; set; } = string.Empty;
        public string CRDocumentPath { get; set; } = string.Empty;
        public string IDDocumentPath { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public int RequestedByUserId { get; set; }

        public string RequestedByUserName { get; set; } = string.Empty;

        public int? VerifiedByUserId { get; set; }

        public string VerifiedByUserName { get; set; } = string.Empty;

        public DateTime? VerifiedAt { get; set; }

        public string? VerificationNotes { get; set; }

        public int? ApprovedByUserId { get; set; }

        public string ApprovedByUserName { get; set; } = string.Empty;

        public DateTime? ApprovedAt { get; set; }

        public string? ApprovalNotes { get; set; }

        public bool IsFlagged { get; set; } = false;

        public string? FlagReason { get; set; }

        public int? FlagRaisedByUserId { get; set; }

        public string FlagRaisedByUserName { get; set; } = string.Empty;

        public DateTime? FlagRaisedAt { get; set; }

        public DateTime? FlagResolvedAt { get; set; }

        public string AuditTrail { get; set; } = string.Empty;

        // Search/filter properties
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Status")]
        public string? StatusFilter { get; set; }

        [Display(Name = "Date")]
        public DateTime? DateFilter { get; set; }
    }
}