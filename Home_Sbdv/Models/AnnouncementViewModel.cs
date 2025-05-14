using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
namespace Home_Sbdv.Models
{
    public class AnnouncementViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [Display(Name = "Content")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Posted By")]
        public string PostedByUsername { get; set; } = string.Empty;

        [Display(Name = "Posted At")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Last Updated")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Published")]
        public bool IsPublished { get; set; } = true;

        [Display(Name = "Attachment")]
        public IFormFile? AttachmentFile { get; set; }

        [Display(Name = "Current Attachment")]
        public string? AttachmentPath { get; set; }

        [Display(Name = "Image")]
        public string? ImagePath { get; set; }

        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        [NotMapped]
        public bool HasImage => !string.IsNullOrEmpty(ImagePath);

        // Default constructor
        public AnnouncementViewModel() { }

        // Constructor to create from Announcement entity
        public AnnouncementViewModel(Entities.Announcement announcement)
        {
            Id = announcement.AnnouncementId;
            Title = announcement.Title;
            Content = announcement.Content;
            PostedByUsername = announcement.User?.Username ?? "Unknown";
            CreatedAt = announcement.CreatedAt ?? DateTime.Now;
            UpdatedAt = announcement.UpdatedAt;
            IsPublished = announcement.IsPublished;
            AttachmentPath = announcement.AttachmentPath;
            ImagePath = announcement.ImagePath;
        }

        // Convert ViewModel back to Entity
        public Entities.Announcement ToEntity()
        {
            return new Entities.Announcement
            {
                AnnouncementId = this.Id,
                Title = this.Title,
                Content = this.Content,
                // PostedBy will be set by the service
                CreatedAt = this.Id == 0 ? DateTime.Now : this.CreatedAt, // Only set for new announcements
                UpdatedAt = this.Id != 0 ? DateTime.Now : null, // Only set for existing announcements
                IsPublished = this.IsPublished,
                AttachmentPath = this.AttachmentPath,
                ImagePath = this.ImagePath
            };
        }
    }

    // Additional ViewModel for listing multiple announcements
    public class AnnouncementListViewModel
    {
        public List<AnnouncementViewModel> Announcements { get; set; } = new List<AnnouncementViewModel>();

        public AnnouncementListViewModel() { }

        public AnnouncementListViewModel(IEnumerable<Entities.Announcement> announcements)
        {
            if (announcements != null)
            {
                foreach (var announcement in announcements)
                {
                    Announcements.Add(new AnnouncementViewModel(announcement));
                }
            }
        }
    }
}