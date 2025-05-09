using Home_Sbdv.Models;
using Home_Sbdv.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Home_Sbdv.Controllers
{
    [Authorize]
    public class ServiceReqController : Controller
    {
        private readonly IServiceRequestService _serviceRequestService;

        public ServiceReqController(IServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }

        // Admin Actions
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminList()
        {
            var result = await _serviceRequestService.GetAllRequestsAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<ServiceReqViewModel>());
            }
            return View("/Views/Pages/Admin/ServiceRequest/AdminList.cshtml", result.Data);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminDetails(int id)
        {
            var result = await _serviceRequestService.GetRequestByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(AdminList));
            }
            return View("/Views/Pages/Admin/ServiceRequest/AdminDetails.cshtml", result.Data);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _serviceRequestService.DeleteRequestAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = "Service request deleted successfully.";
            }
            return RedirectToAction(nameof(AdminList));
        }

        // Staff Actions
        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffList()
        {
            var result = await _serviceRequestService.GetAllRequestsAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<ServiceReqViewModel>());
            }
            return View("/Views/Pages/Staff/ServiceRequest/StaffList.cshtml", result.Data);
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffDetails(int id)
        {
            var result = await _serviceRequestService.GetRequestByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(StaffList));
            }
            return View("/Views/Pages/Staff/ServiceRequest/StaffDetails.cshtml", result.Data);
        }

        [Authorize(Roles = "staff")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _serviceRequestService.ApproveRequestAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = "Service request approved successfully.";
            }
            return RedirectToAction(nameof(StaffList));
        }

        [Authorize(Roles = "staff")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _serviceRequestService.RejectRequestAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = "Service request rejected successfully.";
            }
            return RedirectToAction(nameof(StaffList));
        }

        [Authorize(Roles = "staff")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var result = await _serviceRequestService.UpdateStatusAsync(id, status);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = $"Service request status updated to {status} successfully.";
            }
            return RedirectToAction(nameof(StaffList));
        }

        // User Actions
        [Authorize(Roles = "homeowner")]
        public IActionResult Create()
        {
            return View("/Views/Pages/User/ServiceRequest/Create.cshtml", new ServiceReqViewModel());
        }

        [Authorize(Roles = "homeowner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceReqViewModel model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View("/Views/Pages/User/ServiceRequest/Create.cshtml", model);
            }

            if (imageFile != null)
            {
                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
                {
                    ModelState.AddModelError("ImageFile", "Only JPEG, PNG, and GIF images are allowed.");
                    return View("/Views/Pages/User/ServiceRequest/Create.cshtml", model);
                }

                // Validate file size (max 5MB)
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Image size should not exceed 5MB.");
                    return View("/Views/Pages/User/ServiceRequest/Create.cshtml", model);
                }
            }

            model.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _serviceRequestService.CreateRequestAsync(model, imageFile);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View("/Views/Pages/User/ServiceRequest/Create.cshtml", model);
            }

            TempData["Success"] = "Service request submitted successfully.";
            return RedirectToAction(nameof(MyRequests));
        }

        [Authorize(Roles = "homeowner")]
        public async Task<IActionResult> MyRequests()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _serviceRequestService.GetUserRequestsAsync(userId);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<ServiceReqViewModel>());
            }
            return View("/Views/Pages/User/ServiceRequest/MyRequests.cshtml", result.Data);
        }

        [Authorize(Roles = "homeowner")]
        public async Task<IActionResult> Details(int id)
        {
            var result = await _serviceRequestService.GetRequestByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(MyRequests));
            }

            // Verify that the request belongs to the current user
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (result.Data.UserId != userId)
            {
                TempData["Error"] = "You are not authorized to view this request.";
                return RedirectToAction(nameof(MyRequests));
            }

            return View("/Views/Pages/User/ServiceRequest/Details.cshtml", result.Data);
        }
    }
}
