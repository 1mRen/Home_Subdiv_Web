using Home_Sbdv.Data;
using Home_Sbdv.Models;
using Home_Sbdv.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Home_Sbdv.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AnnouncementService(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<List<Announcement>> GetAllAnnouncementsAsync()
        {
            return await _context.Announcements
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Announcement> GetAnnouncementByIdAsync(int id)
        {
            return await _context.Announcements
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AnnouncementId == id);
        }

        private async Task<string?> SaveFileAsync(IFormFile? file, string subDirectory)
        {
            if (file == null || file.Length == 0)
                return null;

            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "SecureFiles", subDirectory);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return a secure identifier for the file
            return $"{subDirectory}/{uniqueFileName}";
        }

        public async Task<bool> CreateAnnouncementAsync(AnnouncementViewModel model, string username)
        {
            try
            {
                // Check if username is null or empty
                if (string.IsNullOrEmpty(username))
                {
                    return false;
                }

                // Find the user by username
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return false;
                }

                // Save attachment if provided
                string? attachmentPath = null;
                if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
                {
                    attachmentPath = await SaveFileAsync(model.AttachmentFile, "announcements");
                }

                // Save image if provided
                string? imagePath = null;
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    imagePath = await SaveFileAsync(model.ImageFile, "announcements/images");
                }

                var announcement = new Announcement
                {
                    Title = model.Title,
                    Content = model.Content,
                    PostedBy = user.Id,
                    CreatedAt = DateTime.Now,
                    IsPublished = model.IsPublished,
                    AttachmentPath = attachmentPath,
                    ImagePath = imagePath
                };

                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Announcement> GetAnnouncementForEditAsync(int id)
        {
            return await _context.Announcements.FindAsync(id);
        }

        public async Task<bool> UpdateAnnouncementAsync(int id, Announcement updatedAnnouncement, string webRootPath)
        {
            try
            {
                var existingAnnouncement = await _context.Announcements
                    .FirstOrDefaultAsync(a => a.AnnouncementId == id);

                if (existingAnnouncement == null)
                {
                    return false;
                }

                existingAnnouncement.Title = updatedAnnouncement.Title;
                existingAnnouncement.Content = updatedAnnouncement.Content;
                existingAnnouncement.UpdatedAt = DateTime.Now;
                existingAnnouncement.IsPublished = updatedAnnouncement.IsPublished;

                // Only update attachment if a new one is provided
                if (!string.IsNullOrEmpty(updatedAnnouncement.AttachmentPath) &&
                    updatedAnnouncement.AttachmentPath != existingAnnouncement.AttachmentPath)
                {
                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(existingAnnouncement.AttachmentPath))
                    {
                        var oldFilePath = Path.Combine(_environment.ContentRootPath, "SecureFiles", existingAnnouncement.AttachmentPath);
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                    }
                    existingAnnouncement.AttachmentPath = updatedAnnouncement.AttachmentPath;
                }

                _context.Update(existingAnnouncement);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAnnouncementAsync(int id, string webRootPath)
        {
            try
            {
                var announcement = await _context.Announcements.FindAsync(id);
                if (announcement == null)
                {
                    return false;
                }

                // Delete attachment file if exists
                if (!string.IsNullOrEmpty(announcement.AttachmentPath))
                {
                    var filePath = Path.Combine(_environment.ContentRootPath, "SecureFiles", announcement.AttachmentPath);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> TogglePublishStatusAsync(int id)
        {
            try
            {
                var announcement = await _context.Announcements.FindAsync(id);
                if (announcement == null)
                {
                    return false;
                }

                announcement.IsPublished = !announcement.IsPublished;
                announcement.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Methods for dashboard
        public async Task<int> GetTotalAnnouncementsCountAsync()
        {
            return await _context.Announcements.CountAsync();
        }

        public async Task<List<Announcement>> GetRecentAnnouncementsAsync(int count)
        {
            return await _context.Announcements
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Announcement>> GetAnnouncementsByUserIdAsync(string userId)
        {
            if (!int.TryParse(userId, out int id))
                return new List<Announcement>();

            return await _context.Announcements
                .Include(a => a.User)
                .Where(a => a.PostedBy == id)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Announcement>> GetPublishedAnnouncementsAsync()
        {
            return await _context.Announcements
                .Include(a => a.User)
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}