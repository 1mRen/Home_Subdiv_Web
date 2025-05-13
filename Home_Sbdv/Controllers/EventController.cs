using Home_Sbdv.Models;
using Home_Sbdv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
            return View("/Views/Pages/Admin/Event/EventList.cshtml", events);
        }

        // GET: Event/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }
            return View("/Views/Pages/Admin/Event/Details.cshtml", eventItem);
        }

        // GET: Event/Create
        public IActionResult Create()
        {
            // Set default values for new events
            var model = new EventViewModel
            {
                EventDate = DateTime.Today,
                StartTime = new TimeSpan(9, 0, 0), // Default to 9:00 AM
                EndTime = new TimeSpan(17, 0, 0)   // Default to 5:00 PM
            };
            return View("/Views/Pages/Admin/Event/Create.cshtml", model);
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel eventModel)
        {
            if (ModelState.IsValid)
            {
                if (User.Identity == null || string.IsNullOrEmpty(User.Identity.Name))
                {
                    return Unauthorized();
                }

                // Validate that end time is after start time
                if (eventModel.EndTime <= eventModel.StartTime)
                {
                    ModelState.AddModelError("EndTime", "End time must be after start time");
                    return View(eventModel);
                }

                if (await _eventService.CreateEventAsync(eventModel, User.Identity.Name))
                {
                    // Notify all users about the new event
                    // We need the event ID and title, so fetch the event by unique fields
                    var createdEvent = await _eventService.GetEventByNameAndDateAsync(eventModel.EventName, eventModel.EventDate);
                    if (createdEvent != null)
                    {
                        var notificationService = HttpContext.RequestServices.GetService(typeof(Home_Sbdv.Services.Interfaces.INotificationService)) as Home_Sbdv.Services.Interfaces.INotificationService;
                        if (notificationService != null)
                        {
                            await notificationService.NotifyEventCreated(createdEvent.EventId, createdEvent.EventName);
                        }
                    }
                    return RedirectToAction(nameof(EventList));
                }
                return Unauthorized();
            }
            return View("/Views/Pages/Admin/Event/Create.cshtml", eventModel);
        }

        // GET: Event/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var eventModel = await _eventService.GetEventForEditAsync(id);
            if (eventModel == null)
            {
                return NotFound();
            }
            return View("/Views/Pages/Admin/Event/Edit.cshtml", eventModel);
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel updatedEvent)
        {
            if (id != updatedEvent.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Validate that end time is after start time
                if (updatedEvent.EndTime <= updatedEvent.StartTime)
                {
                    ModelState.AddModelError("EndTime", "End time must be after start time");
                    return View(updatedEvent);
                }

                if (await _eventService.UpdateEventAsync(id, updatedEvent))
                {
                    return RedirectToAction(nameof(EventList));
                }
                return NotFound();
            }
            return View("/Views/Pages/Admin/Event/Edit.cshtml", updatedEvent);
        }

        // GET: Event/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }
            return View("/Views/Pages/Admin/Event/Delete.cshtml", eventItem);
        }

        // POST: Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _eventService.DeleteEventAsync(id);
            return RedirectToAction(nameof(EventList));
        }

        // GET: Event/ViewAll - For homeowners to view all events
        [AllowAnonymous] // Allow non-authenticated users to view events
        public async Task<IActionResult> ViewAll()
        {
            var events = await _eventService.GetAllEventsAsync();
            return View("/Views/Pages/User/Event/ViewAll.cshtml", events);
        }

        // GET: Event/View/5 - For homeowners to view a specific event
        [AllowAnonymous] // Allow non-authenticated users to view specific event
        public async Task<IActionResult> View(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }
            return View("/Views/Pages/User/Event/View.cshtml", eventItem);
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffEventList()
        {
            var events = await _eventService.GetAllEventsAsync();
            return View("/Views/Pages/Staff/Event/EventList.cshtml", events);
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffDetails(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }
            return View("/Views/Pages/Staff/Event/Details.cshtml", eventItem);
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffEdit(int id)
        {
            var eventModel = await _eventService.GetEventForEditAsync(id);
            if (eventModel == null)
            {
                return NotFound();
            }
            return View("/Views/Pages/Staff/Event/Edit.cshtml", eventModel);
        }

        [HttpPost]
        [Authorize(Roles = "staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StaffEdit(int id, EventViewModel updatedEvent)
        {
            if (id != updatedEvent.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Validate that end time is after start time
                if (updatedEvent.EndTime <= updatedEvent.StartTime)
                {
                    ModelState.AddModelError("EndTime", "End time must be after start time");
                    return View(updatedEvent);
                }

                if (await _eventService.UpdateEventAsync(id, updatedEvent))
                {
                    return RedirectToAction("StaffEventList");
                }
                return NotFound();
            }
            return View("/Views/Pages/Staff/Event/Edit.cshtml", updatedEvent);
        }
    }
}