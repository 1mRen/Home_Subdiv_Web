using Home_Sbdv.Models;
using Home_Sbdv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Home_Sbdv.Constants;

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
        public async Task<IActionResult> Create([Bind("Title,Content,AttachmentFile")] AnnouncementViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (User.Identity == null || string.IsNullOrEmpty(User.Identity.Name))
                {
                    return Unauthorized();
                }

                if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
                {
                    if (!Directory.Exists(FilePaths.AnnouncementAttachments))
                        Directory.CreateDirectory(FilePaths.AnnouncementAttachments);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.AttachmentFile.FileName);
                    var filePath = Path.Combine(FilePaths.AnnouncementAttachments, uniqueFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AttachmentFile.CopyToAsync(stream);
                    }
                    
                    model.AttachmentPath = FilePaths.GetRelativePath(filePath);
                }

                var success = await _announcementService.CreateAnnouncementAsync(model, User.Identity.Name);
                if (success)
                {
                    return RedirectToAction(nameof(AnnouncementList));
                }
            }
            return View("/Views/Pages/Admin/Announcement/Create.cshtml", model);
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
                try
                {
                    // Handle file upload
                    if (viewModel.AttachmentFile != null && viewModel.AttachmentFile.Length > 0)
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

                    ModelState.AddModelError("", "Failed to update announcement.");
                    return View("/Views/Pages/Admin/Announcement/Edit.cshtml", viewModel);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating announcement: {ex.Message}");
                }
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


        // GET: Announcement/StaffAnnouncementList
        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffAnnouncementList()
        {
            var announcements = await _announcementService.GetAllAnnouncementsAsync();
            var viewModel = new AnnouncementListViewModel(announcements);
            return View("/Views/Pages/Staff/Announcement/StaffAnnouncementList.cshtml", viewModel);
        }

        // GET: Announcement/StaffDetails/5
        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffDetails(int id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            var viewModel = new AnnouncementViewModel(announcement);
            return View("/Views/Pages/Staff/Announcement/StaffDetails.cshtml", viewModel);
        }

        // GET: Announcement/StaffEdit/5
        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffEdit(int id)
        {
            var announcement = await _announcementService.GetAnnouncementForEditAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            var viewModel = new AnnouncementViewModel(announcement);
            return View("/Views/Pages/Staff/Announcement/StaffEdit.cshtml", viewModel);
        }

        // POST: Announcement/StaffEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffEdit(int id, AnnouncementViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle file upload
                    if (viewModel.AttachmentFile != null && viewModel.AttachmentFile.Length > 0)
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
                        return RedirectToAction(nameof(StaffAnnouncementList));
                    }

                    ModelState.AddModelError("", "Failed to update announcement.");
                    return View("/Views/Pages/Staff/Announcement/StaffEdit.cshtml", viewModel);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating announcement: {ex.Message}");
                }
            }

            return View("/Views/Pages/Staff/Announcement/StaffEdit.cshtml", viewModel);
        }
    }
}