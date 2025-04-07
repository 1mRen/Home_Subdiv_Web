using Home_Sbdv.Models;
using Home_Sbdv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var viewModel = new AnnouncementListViewModel(announcements);
            return View(viewModel);
        }

        // GET: Announcement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            var viewModel = new AnnouncementViewModel(announcement);
            return View(viewModel);
        }

        // GET: Announcement/Create
        public IActionResult Create()
        {
            return View(new AnnouncementViewModel());
        }

        // POST: Announcement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnnouncementViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Ensure user is authenticated before proceeding
                if (User.Identity == null || string.IsNullOrEmpty(User.Identity.Name))
                {
                    return Unauthorized();
                }

                var announcement = viewModel.ToEntity();
                if (await _announcementService.CreateAnnouncementAsync(announcement, User.Identity.Name))
                {
                    return RedirectToAction(nameof(AnnouncementList));
                }
                return Unauthorized();
            }
            return View(viewModel);
        }

        // GET: Announcement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var announcement = await _announcementService.GetAnnouncementForEditAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            var viewModel = new AnnouncementViewModel(announcement);
            return View(viewModel);
        }

        // POST: Announcement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnnouncementViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var announcement = viewModel.ToEntity();
                if (await _announcementService.UpdateAnnouncementAsync(id, announcement))
                {
                    return RedirectToAction(nameof(AnnouncementList));
                }
                return NotFound();
            }
            return View(viewModel);
        }

        // GET: Announcement/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            var viewModel = new AnnouncementViewModel(announcement);
            return View(viewModel);
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