using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using VehicleRegistration.Services;
using VehicleRegistration.ViewModels;

namespace VehicleRegistration.Controllers
{
    public class VehicleRegistrationController : Controller
    {
        private readonly IRegistrationService _registrationService;

        public VehicleRegistrationController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        // GET: VehicleRegistration
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("admin"))
            {
                var model = await _registrationService.GetRegistrationListViewModelAsync();
                return View("/Views/Pages/Admin/VehicleRegistration/List.cshtml", model);
            }
            else if (User.IsInRole("staff"))
            {
                var model = await _registrationService.GetRegistrationListViewModelAsync();
                return View("/Views/Pages/Staff/VehicleRegistration/List.cshtml", model);
            }
            else
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var model = await _registrationService.GetUserRegistrationListViewModelAsync(userId);
                return View("/Views/Pages/User/VehicleRegistration/MyRegistrations.cshtml", model);
            }
        }

        // GET: VehicleRegistration/Approve/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var registration = await _registrationService.GetRegistrationByIdAsync(id);

            if (registration == null)
                return NotFound();

            if (registration.Status != "Pending")
            {
                TempData["ErrorMessage"] = "This registration has already been processed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.StatusList = _registrationService.GetRegistrationStatusList();

            return View(new VehicleRegistrationApprovalViewModel
            {
                Status = "Approved",
                ApprovalNotes = ""
            });
        }

        // POST: VehicleRegistration/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Approve(int id, VehicleRegistrationApprovalViewModel model)
        {
            if (ModelState.IsValid)
            {
                var registration = await _registrationService.GetRegistrationByIdAsync(id);

                if (registration == null)
                    return NotFound();

                if (registration.Status != "Pending")
                {
                    TempData["ErrorMessage"] = "This registration has already been processed.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var approverId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var success = await _registrationService.UpdateRegistrationStatusAsync(id, model, approverId);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Vehicle registration {model.Status.ToLower()} successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Failed to process the registration. Please try again.");
                }
            }

            ViewBag.StatusList = _registrationService.GetRegistrationStatusList();
            return View(model);
        }

        // GET: VehicleRegistration/Reject/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var registration = await _registrationService.GetRegistrationByIdAsync(id);

            if (registration == null)
                return NotFound();

            if (registration.Status != "Pending")
            {
                TempData["ErrorMessage"] = "This registration has already been processed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.StatusList = _registrationService.GetRegistrationStatusList();

            return View(new VehicleRegistrationApprovalViewModel
            {
                Status = "Rejected",
                ApprovalNotes = ""
            });
        }

        // POST: VehicleRegistration/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Reject(int id, VehicleRegistrationApprovalViewModel model)
        {
            if (ModelState.IsValid)
            {
                var registration = await _registrationService.GetRegistrationByIdAsync(id);

                if (registration == null)
                    return NotFound();

                if (registration.Status != "Pending")
                {
                    TempData["ErrorMessage"] = "This registration has already been processed.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var approverId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var success = await _registrationService.UpdateRegistrationStatusAsync(id, model, approverId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Vehicle registration rejected successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Failed to reject the registration. Please try again.");
                }
            }

            ViewBag.StatusList = _registrationService.GetRegistrationStatusList();
            return View(model);
        }
    }
} 