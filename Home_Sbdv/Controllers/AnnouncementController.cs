using Home_Sbdv.Entities;
using Home_Sbdv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Home_Sbdv.Controllers
{
    [Authorize]
    public class AnnouncementController : Controller
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        // GET: Announcement/AnnouncementList
        public async Task<IActionResult> AnnouncementList()
        {
            var announcements = await _announcementService.GetAllAnnouncementsAsync();
            return View(announcements);
        }

        // GET: Announcement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // GET: Announcement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Announcement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content")] Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                // Ensure user is authenticated before proceeding
                if (User.Identity == null || string.IsNullOrEmpty(User.Identity.Name))
                {
                    return Unauthorized();
                }

                if (await _announcementService.CreateAnnouncementAsync(announcement, User.Identity.Name))
                {
                    return RedirectToAction(nameof(AnnouncementList));
                }

                return Unauthorized();
            }
            return View(announcement);
        }

        // GET: Announcement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var announcement = await _announcementService.GetAnnouncementForEditAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        // POST: Announcement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content")] Announcement updatedAnnouncement)
        {
            if (id != updatedAnnouncement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _announcementService.UpdateAnnouncementAsync(id, updatedAnnouncement))
                {
                    return RedirectToAction(nameof(AnnouncementList));
                }

                return NotFound();
            }
            return View(updatedAnnouncement);
        }

        // GET: Announcement/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // POST: Announcement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _announcementService.DeleteAnnouncementAsync(id);
            return RedirectToAction(nameof(AnnouncementList));
        }
    }
}