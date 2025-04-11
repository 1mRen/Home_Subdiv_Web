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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AnnouncementController(IAnnouncementService announcementService, IWebHostEnvironment webHostEnvironment)
        {
            _announcementService = announcementService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Announcement/AnnouncementList
        public async Task<IActionResult> AnnouncementList()
        {
            var announcements = await _announcementService.GetAllAnnouncementsAsync();
            var viewModel = new AnnouncementListViewModel(announcements);
            return View("/Views/Pages/Admin/Announcement/AnnouncementList.cshtml", viewModel);
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
            return View("/Views/Pages/Admin/Announcement/Details.cshtml", viewModel);
        }

        // GET: Announcement/Create
        public IActionResult Create()
        {
            return View("/Views/Pages/Admin/Announcement/Create.cshtml", new AnnouncementViewModel());
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

                // Handle file upload
                if (viewModel.AttachmentFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "announcements");

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.AttachmentFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.AttachmentFile.CopyToAsync(fileStream);
                    }

                    // Save file path to viewModel
                    viewModel.AttachmentPath = "/uploads/announcements/" + uniqueFileName;
                }

                var announcement = viewModel.ToEntity();
                if (await _announcementService.CreateAnnouncementAsync(announcement, User.Identity.Name, _webHostEnvironment.WebRootPath))
                {
                    return RedirectToAction(nameof(AnnouncementList));
                }
                return Unauthorized();
            }
            return View("/Views/Pages/Admin/Announcement/Create.cshtml", viewModel);
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
            return View("/Views/Pages/Admin/Announcement/Edit.cshtml", viewModel);
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
                // Handle file upload
                if (viewModel.AttachmentFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "announcements");

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.AttachmentFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.AttachmentFile.CopyToAsync(fileStream);
                    }

                    // Save file path to viewModel
                    viewModel.AttachmentPath = "/uploads/announcements/" + uniqueFileName;
                }

                var announcement = viewModel.ToEntity();
                if (await _announcementService.UpdateAnnouncementAsync(id, announcement, _webHostEnvironment.WebRootPath))
                {
                    return RedirectToAction(nameof(AnnouncementList));
                }
                return NotFound();
            }
            return View("/Views/Pages/Admin/Announcement/Edit.cshtml", viewModel);
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
            return View("/Views/Pages/Admin/Announcement/Delete.cshtml", viewModel);
        }

        // POST: Announcement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _announcementService.DeleteAnnouncementAsync(id, _webHostEnvironment.WebRootPath);
            return RedirectToAction(nameof(AnnouncementList));
        }

        // POST: Announcement/TogglePublishStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublishStatus(int id)
        {
            if (await _announcementService.TogglePublishStatusAsync(id))
            {
                return RedirectToAction(nameof(AnnouncementList));
            }
            return NotFound();
        }

        public async Task<IActionResult> ViewAll()
        {
            var publishedAnnouncements = await _announcementService.GetPublishedAnnouncementsAsync();
            var viewModel = new AnnouncementListViewModel(publishedAnnouncements);
            return View("/Views/Pages/User/Announcement/ViewAll.cshtml", viewModel);
        }

        // GET: Announcement/View/5
        public async Task<IActionResult> View(int id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null || !announcement.IsPublished)
            {
                return NotFound();
            }

            var viewModel = new AnnouncementViewModel(announcement);
            return View("/Views/Pages/User/Announcement/View.cshtml", viewModel);
        }
    }
}