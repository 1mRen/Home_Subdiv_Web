using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public interface IEventService
    {
        Task<List<EventViewModel>> GetAllEventsAsync();
        Task<EventViewModel> GetEventByIdAsync(int id);
        Task<bool> CreateEventAsync(EventViewModel eventModel, string username);
        Task<EventViewModel> GetEventForEditAsync(int id);
        Task<bool> UpdateEventAsync(int id, EventViewModel updatedEvent);
        Task<bool> DeleteEventAsync(int id);

        // New method for dashboard
        Task<List<Event>> GetUpcomingEventsAsync(int count);
    }
}