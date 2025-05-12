using Home_Sbdv.Models;
using Home_Sbdv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Claims;
using Home_Sbdv.Constants;

namespace Home_Sbdv.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IUserManagementService _userService;

        public FeedbackController(IFeedbackService feedbackService, IUserManagementService userService)
        {
            _feedbackService = feedbackService;
            _userService = userService;
        }

        // User actions
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Get current user ID
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var feedbackListViewModel = await _feedbackService.GetUserFeedbackListViewModelAsync(userId);
            return View("/Views/Pages/User/Feedback/MyFeedbacks.cshtml", feedbackListViewModel);
        }


        [Authorize]
        public IActionResult Create()
        {
            ViewBag.FeedbackTypeList = _feedbackService.GetFeedbackTypeList();
            return View("/Views/Pages/User/Feedback/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Title,Description,Type,AttachmentFile")] FeedbackViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FeedbackTypeList = _feedbackService.GetFeedbackTypeList();
                return View("/Views/Pages/User/Feedback/Create.cshtml", model);
            }

            try
            {
                // Get current user ID
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                model.SubmittedById = userId;
                model.Status = "Pending";

                // Handle file upload
                if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
                {
                    if (!Directory.Exists(FilePaths.FeedbackAttachments))
                        Directory.CreateDirectory(FilePaths.FeedbackAttachments);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.AttachmentFile.FileName);
                    var filePath = Path.Combine(FilePaths.FeedbackAttachments, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AttachmentFile.CopyToAsync(stream);
                    }

                    model.AttachmentPath = FilePaths.GetRelativePath(filePath);
                }

                var success = await _feedbackService.CreateFeedbackAsync(model);
                if (!success)
                {
                    TempData["Error"] = "Failed to submit feedback. Please try again.";
                    return View("/Views/Pages/User/Feedback/Create.cshtml", model);
                }

                TempData["Success"] = "Your feedback has been submitted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                ViewBag.FeedbackTypeList = _feedbackService.GetFeedbackTypeList();
                return View("/Views/Pages/User/Feedback/Create.cshtml", model);
            }
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);

            if (feedback == null)
            {
                return NotFound();
            }

            // Ensure user can only view their own feedback
            if (feedback.SubmittedById != userId && !User.IsInRole("admin") && !User.IsInRole("staff"))
            {
                return Forbid();
            }

            return View("/Views/Pages/User/Feedback/Details.cshtml", feedback);
        }

        // Admin actions
        public async Task<IActionResult> AdminList()
        {
            var feedbackListViewModel = await _feedbackService.GetFeedbackListViewModelAsync();
            return View("/Views/Pages/Admin/Feedback/List.cshtml", feedbackListViewModel);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminDetails(int id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }
            return View("/Views/Pages/Admin/Feedback/Details.cshtml", feedback);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Assign(int id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            // Get staff list for dropdown
            ViewBag.StaffList = await _userService.GetStaffSelectListAsync();

            return View("/Views/Pages/Admin/Feedback/Assign.cshtml", feedback);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Assign(int id, [Bind("AssignedToId")] FeedbackViewModel model)
        {
            if (model.AssignedToId == null || model.AssignedToId <= 0)
            {
                TempData["Error"] = "Please select a staff member to assign.";
                ViewBag.StaffList = await _userService.GetStaffSelectListAsync();
                var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
                return View("/Views/Pages/Admin/Feedback/Assign.cshtml", feedback);
            }

            var success = await _feedbackService.AssignFeedbackAsync(id, model.AssignedToId.Value);
            if (!success)
            {
                TempData["Error"] = "Failed to assign feedback. Please try again.";
                return RedirectToAction(nameof(AdminDetails), new { id });
            }

            TempData["Success"] = "Feedback has been assigned successfully.";
            return RedirectToAction(nameof(AdminList));
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }
            return View("/Views/Pages/Admin/Feedback/Delete.cshtml", feedback);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _feedbackService.DeleteFeedbackAsync(id);
            if (!success)
            {
                TempData["Error"] = "Failed to delete feedback.";
                return RedirectToAction(nameof(AdminDetails), new { id });
            }

            TempData["Success"] = "Feedback deleted successfully.";
            return RedirectToAction(nameof(AdminList));
        }

        // Staff actions
        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffList()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var feedbackListViewModel = await _feedbackService.GetAssignedFeedbackListViewModelAsync(userId);
            return View("/Views/Pages/Staff/Feedback/List.cshtml", feedbackListViewModel);
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffDetails(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);

            if (feedback == null)
            {
                return NotFound();
            }

            // Staff can only view feedback assigned to them or unassigned
            if (feedback.AssignedToId != userId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            return View("/Views/Pages/Staff/Feedback/Details.cshtml", feedback);
        }

        [Authorize(Roles = "staff,admin")]
        public async Task<IActionResult> Respond(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);

            if (feedback == null)
            {
                return NotFound();
            }

            // Staff can only respond to feedback assigned to them
            if (!User.IsInRole("admin") && feedback.AssignedToId != userId)
            {
                return Forbid();
            }

            ViewBag.StatusList = _feedbackService.GetFeedbackStatusList();
            ViewBag.Feedback = feedback; // Pass the full feedback for display
            return View("/Views/Pages/Staff/Feedback/Respond.cshtml", new FeedbackResponseViewModel { Status = feedback.Status, StaffResponse = feedback.StaffResponse });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "staff,admin")]
        public async Task<IActionResult> Respond(int id, FeedbackResponseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
                ViewBag.StatusList = _feedbackService.GetFeedbackStatusList();
                ViewBag.Feedback = feedback;
                return View("/Views/Pages/Staff/Feedback/Respond.cshtml", model);
            }

            var success = await _feedbackService.UpdateFeedbackStatusAsync(id, model);
            if (!success)
            {
                TempData["Error"] = "Failed to submit response. Please try again.";
                return RedirectToAction("Respond", new { id });
            }

            TempData["Success"] = "Response submitted successfully.";

            // Determine which action to redirect to based on user role
            if (User.IsInRole("admin"))
            {
                return RedirectToAction("AdminList");
            }

            return RedirectToAction("StaffList");
        }

        // Download attachment
        public async Task<IActionResult> Download(int id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null || string.IsNullOrEmpty(feedback.AttachmentPath))
            {
                return NotFound();
            }

            // Security check
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isAuthorized = feedback.SubmittedById == userId ||
                                feedback.AssignedToId == userId ||
                                User.IsInRole("admin");

            if (!isAuthorized)
            {
                return Forbid();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", feedback.AttachmentPath.TrimStart('~', '/'));

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileName = Path.GetFileName(filePath);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Statistics()
        {
            var model = await _feedbackService.GetFeedbackStatisticsViewModelAsync();
            return View("/Views/Pages/Admin/Feedback/Statistics.cshtml", model);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AllFeedbacks()
        {
            var model = await _feedbackService.GetFeedbackListViewModelAsync();

            // Get staff list for assignment dropdown
            ViewBag.StaffMembers = await _userService.GetStaffListAsync();

            return View("/Views/Pages/Admin/Feedback/AllFeedbacks.cshtml", model);
        }

        // Admin action to assign staff to feedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AssignStaff(int feedbackId, int staffId)
        {
            if (staffId <= 0)
            {
                TempData["Error"] = "Please select a valid staff member.";
                return RedirectToAction(nameof(AllFeedbacks));
            }

            var success = await _feedbackService.AssignFeedbackAsync(feedbackId, staffId);
            if (!success)
            {
                TempData["Error"] = "Failed to assign staff. Please try again.";
                return RedirectToAction(nameof(AllFeedbacks));
            }

            TempData["Success"] = "Feedback has been assigned successfully.";
            return RedirectToAction(nameof(AllFeedbacks));
        }

        // Admin/Staff action to update feedback status
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,staff")]
        public async Task<IActionResult> UpdateStatus(int id, FeedbackResponseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Response is required.";
                return RedirectToAction(nameof(AllFeedbacks));
            }

            var success = await _feedbackService.UpdateFeedbackStatusAsync(id, model);
            if (!success)
            {
                TempData["Error"] = "Failed to update status. Please try again.";
                return RedirectToAction(nameof(AllFeedbacks));
            }

            TempData["Success"] = "Feedback status has been updated successfully.";
            return RedirectToAction(User.IsInRole("admin") ? "AllFeedbacks" : "StaffList");
        }

        // User action for "My Feedbacks" page
        [Authorize]
        public async Task<IActionResult> MyFeedbacks()
        {
            // Get current user ID
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var model = await _feedbackService.GetUserFeedbackListViewModelAsync(userId);
            return View("/Views/Pages/User/Feedback/MyFeedbacks.cshtml", model);
        }
    }
}
