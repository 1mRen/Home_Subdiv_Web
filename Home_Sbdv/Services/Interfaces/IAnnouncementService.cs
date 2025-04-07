using Home_Sbdv.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync();
        Task<Announcement> GetAnnouncementByIdAsync(int id);
        Task<bool> CreateAnnouncementAsync(Announcement announcement, string username);
        Task<Announcement> GetAnnouncementForEditAsync(int id);
        Task<bool> UpdateAnnouncementAsync(int id, Announcement updatedAnnouncement);
        Task<bool> DeleteAnnouncementAsync(int id);
    }
}