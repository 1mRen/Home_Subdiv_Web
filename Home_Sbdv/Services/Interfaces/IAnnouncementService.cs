using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public interface IAnnouncementService
    {
        Task<List<Announcement>> GetAllAnnouncementsAsync();
        Task<Announcement> GetAnnouncementByIdAsync(int id);
        Task<bool> CreateAnnouncementAsync(AnnouncementViewModel model, string username);
        Task<Announcement> GetAnnouncementForEditAsync(int id);
        Task<bool> UpdateAnnouncementAsync(int id, Announcement updatedAnnouncement, string webRootPath);
        Task<bool> DeleteAnnouncementAsync(int id, string webRootPath);
        // Dashboard methods
        Task<int> GetTotalAnnouncementsCountAsync();
        Task<List<Announcement>> GetRecentAnnouncementsAsync(int count);
        Task<List<Announcement>> GetAnnouncementsByUserIdAsync(string userId);
        Task<bool> TogglePublishStatusAsync(int id);    
        Task<List<Announcement>> GetPublishedAnnouncementsAsync();
    }
}