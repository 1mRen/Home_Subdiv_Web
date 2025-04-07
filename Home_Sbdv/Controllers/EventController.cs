using Home_Sbdv.Models;
using Home_Sbdv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Home_Sbdv.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        // GET: Event/EventList
        public async Task<IActionResult> EventList()
        {
            var events = await _eventService.GetAllEventsAsync();
            return View(events);
        }

        // GET: Event/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
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

                if (await _eventService.CreateEventAsync(eventModel, User.Identity.Name))
                {
                    return RedirectToAction(nameof(EventList));
                }

                return Unauthorized();
            }
            return View(eventModel);
        }

        // GET: Event/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var eventModel = await _eventService.GetEventForEditAsync(id);
            if (eventModel == null)
            {
                return NotFound();
            }

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
                if (await _eventService.UpdateEventAsync(id, updatedEvent))
                {
                    return RedirectToAction(nameof(EventList));
                }

                return NotFound();
            }
            return View(updatedEvent);
        }

        // GET: Event/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
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
            await _eventService.DeleteEventAsync(id);
            return RedirectToAction(nameof(EventList));
        }
    }
}