using Home_Sbdv.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public interface IEventService
    {
        // Admin methods
        Task<List<EventViewModel>> GetAllEventsAsync();
        Task<EventViewModel> GetEventByIdAsync(int id);
        Task<bool> CreateEventAsync(EventViewModel eventModel, string username);
        Task<EventViewModel> GetEventForEditAsync(int id);
        Task<bool> UpdateEventAsync(int id, EventViewModel updatedEvent);
        Task<bool> DeleteEventAsync(int id);

        // Dashboard methods - modified to return EventViewModel consistently
        Task<List<EventViewModel>> GetUpcomingEventsAsync(int count);
        Task<List<EventViewModel>> GetUpcomingEventsForDashboardAsync(int count);

        Task<EventViewModel> GetEventByNameAndDateAsync(string eventName, DateTime eventDate);
    }
}