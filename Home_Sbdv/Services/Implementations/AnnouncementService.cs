using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly AppDbContext _context;

        public AnnouncementService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Announcement>> GetAllAnnouncementsAsync()
        {
            return await _context.Announcements
                .Include(a => a.User)
                .OrderByDescending(a => a.PostedAt)
                .ToListAsync();
        }

        public async Task<Announcement> GetAnnouncementByIdAsync(int id)
        {
            return await _context.Announcements
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> CreateAnnouncementAsync(Announcement announcement, string username)
        {
            try
            {
                var userId = await _context.Users
                    .Where(u => u.Username == username)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();

                if (userId == 0)
                {
                    return false;
                }

                announcement.PostedBy = userId;
                announcement.PostedAt = DateTime.Now;
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

        public async Task<bool> UpdateAnnouncementAsync(int id, Announcement updatedAnnouncement)
        {
            try
            {
                var existingAnnouncement = await _context.Announcements
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (existingAnnouncement == null)
                {
                    return false;
                }

                existingAnnouncement.Title = updatedAnnouncement.Title;
                existingAnnouncement.Content = updatedAnnouncement.Content;
                existingAnnouncement.UpdatedAt = DateTime.Now;

                _context.Update(existingAnnouncement);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAnnouncementAsync(int id)
        {
            try
            {
                var announcement = await _context.Announcements.FindAsync(id);
                if (announcement == null)
                {
                    return false;
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

        // Methods for dashboard
        public async Task<int> GetTotalAnnouncementsCountAsync()
        {
            return await _context.Announcements.CountAsync();
        }

        public async Task<List<Announcement>> GetRecentAnnouncementsAsync(int count)
        {
            return await _context.Announcements
                .Include(a => a.User)
                .OrderByDescending(a => a.PostedAt)
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
                .OrderByDescending(a => a.PostedAt)
                .ToListAsync();
        }
    }
}