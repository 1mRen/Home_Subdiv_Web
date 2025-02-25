using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Home_Sbdv.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly AppDbContext _context;

        public EventController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Event/EventList
        public async Task<IActionResult> EventList()
        {
            var events = await _context.Events
                .Include(e => e.User)
                .Select(e => new EventViewModel
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventDescription = e.EventDescription,
                    EventDate = e.EventDate,
                    CreatedBy = e.User != null ? e.User.Id : 0, // Stores User ID
                    CreatedByName = e.User != null ? e.User.FullName : "Unknown", // Stores Full Name
                    LastUpdated = e.LastUpdated
                })
                .ToListAsync();

            return View(events);
        }

        // GET: Event/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var eventItem = await _context.Events
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

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // GET: Event/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventName,EventDescription,EventDate")] EventViewModel eventModel)
        {
            if (ModelState.IsValid)
            {
                if (User.Identity == null || string.IsNullOrEmpty(User.Identity.Name))
                {
                    return Unauthorized();
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);

                if (user == null)
                {
                    return Unauthorized();
                }

                var newEvent = new Event
                {
                    EventName = eventModel.EventName,
                    EventDescription = eventModel.EventDescription,
                    EventDate = eventModel.EventDate,
                    CreatedBy = user.Id,  // Store user ID
                    LastUpdated = DateTime.UtcNow
                };

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(EventList));
            }
            return View(eventModel);
        }

        // GET: Event/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            var eventModel = new EventViewModel
            {
               EventId = eventItem.EventId,
                EventName = eventItem.EventName,
                EventDescription = eventItem.EventDescription,
                EventDate = eventItem.EventDate
            };

            return View(eventModel);
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDescription,EventDate")] EventViewModel updatedEvent)
        {
            if (id != updatedEvent.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
                if (existingEvent == null)
                {
                    return NotFound();
                }

                existingEvent.EventName = updatedEvent.EventName;
                existingEvent.EventDescription = updatedEvent.EventDescription;
                existingEvent.EventDate = updatedEvent.EventDate;
                existingEvent.LastUpdated = DateTime.UtcNow;

                _context.Update(existingEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(EventList));
            }
            return View(updatedEvent);
        }

        // GET: Event/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var eventItem = await _context.Events
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

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // POST: Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(EventList));
        }
    }
}
