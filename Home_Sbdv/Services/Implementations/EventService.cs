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
                    Description = e.Description,
                    EventDate = e.EventDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location,
                    CreatedBy = e.User != null ? e.User.Id : 0,
                    CreatedByName = e.User != null ? e.User.FullName : "Unknown",
                    LastUpdated = e.LastUpdated
                })
                .OrderBy(e => e.EventDate)
                .ThenBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<EventViewModel> GetEventByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.User)
                .Where(e => e.EventId == id)
                .Select(e => new EventViewModel
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location,
                    CreatedBy = e.User != null ? e.User.Id : 0,
                    CreatedByName = e.User != null ? e.User.FullName : "Unknown",
                    LastUpdated = e.LastUpdated
                })
                .FirstOrDefaultAsync();
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
                    Description = eventModel.Description,
                    EventDate = eventModel.EventDate.Date, // Ensure we only store the date part
                    StartTime = eventModel.StartTime,
                    EndTime = eventModel.EndTime,
                    Location = eventModel.Location,
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
                Description = eventItem.Description,
                EventDate = eventItem.EventDate,
                StartTime = eventItem.StartTime,
                EndTime = eventItem.EndTime,
                Location = eventItem.Location
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
                existingEvent.Description = updatedEvent.Description;
                existingEvent.EventDate = updatedEvent.EventDate.Date; // Ensure we only store the date part
                existingEvent.StartTime = updatedEvent.StartTime;
                existingEvent.EndTime = updatedEvent.EndTime;
                existingEvent.Location = updatedEvent.Location;
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

        // FIXED: Modified to return EventViewModel instead of Event entities
        public async Task<List<EventViewModel>> GetUpcomingEventsAsync(int count)
        {
            return await _context.Events
                .Include(e => e.User)
                .Where(e => e.EventDate >= DateTime.Now.Date)
                .OrderBy(e => e.EventDate)
                .ThenBy(e => e.StartTime)
                .Take(count)
                .Select(e => new EventViewModel
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location,
                    CreatedBy = e.User != null ? e.User.Id : 0,
                    CreatedByName = e.User != null ? e.User.FullName : "Unknown",
                    LastUpdated = e.LastUpdated
                })
                .ToListAsync();
        }

        // NEW: Added specific method for dashboard to ensure consistency
        public async Task<List<EventViewModel>> GetUpcomingEventsForDashboardAsync(int count)
        {
            return await GetUpcomingEventsAsync(count);
        }
    }
}