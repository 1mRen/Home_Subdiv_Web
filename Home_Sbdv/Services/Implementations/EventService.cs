using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;

        public EventService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<EventViewModel>> GetAllEventsAsync()
        {
            return await _context.Events
                .Include(e => e.User)
                .Select(e => new EventViewModel
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventDescription = e.EventDescription,
                    EventDate = e.EventDate,
                    CreatedBy = e.User != null ? e.User.Id : 0,
                    CreatedByName = e.User != null ? e.User.FullName : "Unknown",
                    LastUpdated = e.LastUpdated
                })
                .ToListAsync();
        }

        public async Task<EventViewModel> GetEventByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.User)
                .Select(e => new EventViewModel
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventDescription = e.EventDescription,
                    EventDate = e.EventDate,
                    CreatedBy = e.User != null ? e.User.Id : 0,
                    CreatedByName = e.User != null ? e.User.FullName : "Unknown",
                    LastUpdated = e.LastUpdated
                })
                .FirstOrDefaultAsync(e => e.EventId == id);
        }

        public async Task<bool> CreateEventAsync(EventViewModel eventModel, string username)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return false;
                }

                var newEvent = new Event
                {
                    EventName = eventModel.EventName,
                    EventDescription = eventModel.EventDescription,
                    EventDate = eventModel.EventDate,
                    CreatedBy = user.Id,
                    LastUpdated = DateTime.UtcNow
                };

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<EventViewModel> GetEventForEditAsync(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return null;
            }

            return new EventViewModel
            {
                EventId = eventItem.EventId,
                EventName = eventItem.EventName,
                EventDescription = eventItem.EventDescription,
                EventDate = eventItem.EventDate
            };
        }

        public async Task<bool> UpdateEventAsync(int id, EventViewModel updatedEvent)
        {
            try
            {
                var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
                if (existingEvent == null)
                {
                    return false;
                }

                existingEvent.EventName = updatedEvent.EventName;
                existingEvent.EventDescription = updatedEvent.EventDescription;
                existingEvent.EventDate = updatedEvent.EventDate;
                existingEvent.LastUpdated = DateTime.UtcNow;

                _context.Update(existingEvent);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            try
            {
                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem == null)
                {
                    return false;
                }

                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // New method for dashboard
        public async Task<List<Event>> GetUpcomingEventsAsync(int count)
        {
            return await _context.Events
                .Include(e => e.User)
                .Where(e => e.EventDate >= DateTime.Now)
                .OrderBy(e => e.EventDate)
                .Take(count)
                .ToListAsync();
        }
    }
}