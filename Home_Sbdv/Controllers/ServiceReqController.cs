using Home_Sbdv.Models;
using Home_Sbdv.Data;
using Home_Sbdv.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Home_Sbdv.Constants;

namespace Home_Sbdv.Controllers
{
    [Authorize]
    public class ServiceReqController : Controller
    {
        private readonly IServiceRequestService _serviceRequestService;
        private readonly AppDbContext _context;

        public ServiceReqController(IServiceRequestService serviceRequestService, AppDbContext context)
        {
            _serviceRequestService = serviceRequestService;
            _context = context;
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

        [Authorize(Roles = "admin")]
        public IActionResult AdminCreate()
        {
            ViewBag.Users = _context.Users
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.FullName
            }).ToList();
            return View("/Views/Pages/Admin/ServiceRequest/AdminCreate.cshtml", new ServiceReqViewModel());
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminCreate(ServiceReqViewModel model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View("/Views/Pages/Admin/ServiceRequest/AdminCreate.cshtml", model);
            }

            if (imageFile != null)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
                {
                    ModelState.AddModelError("ImageFile", "Only JPEG, PNG, and GIF images are allowed.");
                    return View("/Views/Pages/Admin/ServiceRequest/AdminCreate.cshtml", model);
                }
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Image size should not exceed 5MB.");
                    return View("/Views/Pages/Admin/ServiceRequest/AdminCreate.cshtml", model);
                }
            }

            // Admin must select a user for the request
            if (model.UserId <= 0)
            {
                ModelState.AddModelError("UserId", "Please select a user for this request.");
                return View("/Views/Pages/Admin/ServiceRequest/AdminCreate.cshtml", model);
            }

            var result = await _serviceRequestService.CreateRequestAsync(model, imageFile);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View("/Views/Pages/Admin/ServiceRequest/AdminCreate.cshtml", model);
            }

            TempData["Success"] = "Service request created successfully.";
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
        public async Task<IActionResult> Create(ServiceReqViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Set the UserId from the current user
                model.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
                {
                    if (!Directory.Exists(FilePaths.ServiceRequestAttachments))
                        Directory.CreateDirectory(FilePaths.ServiceRequestAttachments);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.AttachmentFile.FileName);
                    var filePath = Path.Combine(FilePaths.ServiceRequestAttachments, uniqueFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AttachmentFile.CopyToAsync(stream);
                    }
                    
                    model.AttachmentUrl = FilePaths.GetRelativePath(filePath);
                }

                var result = await _serviceRequestService.CreateServiceRequestAsync(model);
                if (result.Success)
                {
                    TempData["Success"] = "Service request created successfully.";
                    return RedirectToAction(nameof(MyRequests));
                }
                ModelState.AddModelError("", result.Message ?? "Failed to create service request.");
            }
            return View("/Views/Pages/User/ServiceRequest/Create.cshtml", model);
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

        [Authorize(Roles = "homeowner")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _serviceRequestService.GetRequestByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(MyRequests));
            }
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            if (result.Data.UserId != userId)
            {
                TempData["Error"] = "You are not authorized to edit this request.";
                return RedirectToAction(nameof(MyRequests));
            }
            // Only allow edit if not completed, cancelled, or disapproved
            if (result.Data.Status == ServiceRequestStatus.Completed.ToString() ||
                result.Data.Status == ServiceRequestStatus.Cancelled.ToString() ||
                result.Data.Status == ServiceRequestStatus.Disapproved.ToString())
            {
                TempData["Error"] = "You cannot edit a completed, cancelled, or disapproved request.";
                return RedirectToAction(nameof(MyRequests));
            }
            return View("/Views/Pages/User/ServiceRequest/Edit.cshtml", result.Data);
        }

        [Authorize(Roles = "homeowner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceReqViewModel model, IFormFile? imageFile)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            if (model.UserId != userId)
            {
                TempData["Error"] = "You are not authorized to edit this request.";
                return RedirectToAction(nameof(MyRequests));
            }
            var getResult = await _serviceRequestService.GetRequestByIdAsync(id);
            if (!getResult.Success)
            {
                TempData["Error"] = getResult.Message;
                return RedirectToAction(nameof(MyRequests));
            }
            if (getResult.Data.Status == ServiceRequestStatus.Completed.ToString() ||
                getResult.Data.Status == ServiceRequestStatus.Cancelled.ToString() ||
                getResult.Data.Status == ServiceRequestStatus.Disapproved.ToString())
            {
                TempData["Error"] = "You cannot edit a completed, cancelled, or disapproved request.";
                return RedirectToAction(nameof(MyRequests));
            }
            if (!ModelState.IsValid)
            {
                return View("/Views/Pages/User/ServiceRequest/Edit.cshtml", model);
            }
            var result = await _serviceRequestService.UpdateRequestAsync(id, model, imageFile);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View("/Views/Pages/User/ServiceRequest/Edit.cshtml", model);
            }
            TempData["Success"] = "Service request updated successfully.";
            return RedirectToAction(nameof(MyRequests));
        }

        [Authorize(Roles = "homeowner")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _serviceRequestService.GetRequestByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(MyRequests));
            }
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            if (result.Data.UserId != userId)
            {
                TempData["Error"] = "You are not authorized to cancel this request.";
                return RedirectToAction(nameof(MyRequests));
            }
            // Only allow cancel if not completed, cancelled, or disapproved
            if (result.Data.Status == ServiceRequestStatus.Completed.ToString() ||
                result.Data.Status == ServiceRequestStatus.Cancelled.ToString() ||
                result.Data.Status == ServiceRequestStatus.Disapproved.ToString())
            {
                TempData["Error"] = "You cannot cancel a completed, cancelled, or disapproved request.";
                return RedirectToAction(nameof(MyRequests));
            }
            return View("/Views/Pages/User/ServiceRequest/Cancel.cshtml", result.Data);
        }

        [Authorize(Roles = "homeowner")]
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var result = await _serviceRequestService.GetRequestByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(MyRequests));
            }
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            if (result.Data.UserId != userId)
            {
                TempData["Error"] = "You are not authorized to cancel this request.";
                return RedirectToAction(nameof(MyRequests));
            }
            // Only allow cancel if not completed, cancelled, or disapproved
            if (result.Data.Status == ServiceRequestStatus.Completed.ToString() ||
                result.Data.Status == ServiceRequestStatus.Cancelled.ToString() ||
                result.Data.Status == ServiceRequestStatus.Disapproved.ToString())
            {
                TempData["Error"] = "You cannot cancel a completed, cancelled, or disapproved request.";
                return RedirectToAction(nameof(MyRequests));
            }
            var updateResult = await _serviceRequestService.UpdateStatusAsync(id, ServiceRequestStatus.Cancelled.ToString());
            if (!updateResult.Success)
            {
                TempData["Error"] = updateResult.Message;
            }
            else
            {
                TempData["Success"] = "Service request cancelled successfully.";
            }
            return RedirectToAction(nameof(MyRequests));
        }
    }
}
