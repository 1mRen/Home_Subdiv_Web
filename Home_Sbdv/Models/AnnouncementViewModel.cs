using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Home_Sbdv.Models
{
    public class AnnouncementViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [Display(Name = "Content")]
        public string Content { get; set; }

        [Display(Name = "Posted By")]
        public string PostedByUsername { get; set; }

        [Display(Name = "Posted At")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime PostedAt { get; set; }

        [Display(Name = "Last Updated")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Published")]
        public bool IsPublished { get; set; } = true;

        [Display(Name = "Attachment")]
        public IFormFile? AttachmentFile { get; set; }

        [Display(Name = "Current Attachment")]
        public string? AttachmentPath { get; set; }

        // Default constructor
        public AnnouncementViewModel() { }

        // Constructor to create from Announcement entity
        public AnnouncementViewModel(Entities.Announcement announcement)
        {
            Id = announcement.Id;
            Title = announcement.Title;
            Content = announcement.Content;
            PostedByUsername = announcement.User?.Username ?? "Unknown";
            PostedAt = announcement.PostedAt;
            UpdatedAt = announcement.UpdatedAt;
            IsPublished = announcement.IsPublished;
            AttachmentPath = announcement.AttachmentPath;
        }

        // Convert ViewModel back to Entity
        public Entities.Announcement ToEntity()
        {
            return new Entities.Announcement
            {
                Id = this.Id,
                Title = this.Title,
                Content = this.Content,
                // Note: PostedBy is not set here as it should be handled by the service
                PostedAt = this.Id == 0 ? DateTime.Now : this.PostedAt, // Only set for new announcements
                UpdatedAt = this.Id != 0 ? DateTime.Now : null, // Only set for existing announcements
                IsPublished = this.IsPublished,
                AttachmentPath = this.AttachmentPath
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