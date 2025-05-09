using System.Security.Claims;
using Home_Sbdv.Data;
using Home_Sbdv.Models;
using Home_Sbdv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Home_Sbdv.Controllers
{
    [Authorize]
    public class FacilityReservationController : Controller
    {
        private readonly IFacilityReservationService _reservationService;
        private readonly AppDbContext _context; // Kept for the dropdown loading
        private readonly ILogger<FacilityReservationController> _logger;

        public FacilityReservationController(
            IFacilityReservationService reservationService,
            AppDbContext context,
            ILogger<FacilityReservationController> logger)
        {
            _reservationService = reservationService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> FacilityReservationList()
        {
            var viewModels = await _reservationService.GetAllReservationsAsync();
            return View("/Views/Pages/Admin/FacilityReservation/FacilityReservationList.cshtml", viewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();
            return View("/Views/Pages/Admin/FacilityReservation/Details.cshtml", reservation);
        }

        [Authorize(Roles = "staff")]
        public IActionResult Create()
        {
            LoadFacilitiesDropdown();
            return View("/Views/Pages/Staff/FacilityReservation/Create.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FacilityId,ReservationDate,StartTime,EndTime")] FacilityReservationViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("User ID claim not found for {User}", User.Identity?.Name);
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                LoadFacilitiesDropdown(model.FacilityId);
                return View("/Views/Pages/Staff/FacilityReservation/Create.cshtml", model);
            }

            var (success, errorMessage) = await _reservationService.CreateReservationAsync(model, int.Parse(userId));

            if (!success)
            {
                ModelState.AddModelError("", errorMessage);
                LoadFacilitiesDropdown(model.FacilityId);
                return View("/Views/Pages/Staff/FacilityReservation/Create.cshtml", model);
            }

            TempData["SuccessMessage"] = "Reservation created successfully.";
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");

            var canEdit = await _reservationService.CanUserModifyReservation(
                id, int.Parse(userId), isAdmin);

            if (!canEdit) return Forbid();

            LoadFacilitiesDropdown(reservation.FacilityId);
            return View("/Views/Pages/Admin/FacilityReservation/Edit.cshtml", reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservationId,FacilityId,ReservationDate,StartTime,EndTime,Status")] FacilityReservationViewModel model)
        {
            if (id != model.ReservationId) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadFacilitiesDropdown(model.FacilityId);
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");

            var (success, errorMessage) = await _reservationService.UpdateReservationAsync(
                id, model, int.Parse(userId), isAdmin);

            if (!success)
            {
                ModelState.AddModelError("", errorMessage);
                LoadFacilitiesDropdown(model.FacilityId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Reservation updated successfully.";
            return RedirectToAction(nameof(FacilityReservationList));
        }

        [HttpPost]
        [Authorize(Roles = "admin,staff")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");
            var success = await _reservationService.UpdateReservationStatusAsync(id, status, int.Parse(userId), isAdmin);
            if (!success) return Forbid();

            TempData["SuccessMessage"] = "Reservation status updated.";
            return RedirectToAction(nameof(FacilityReservationList));
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");
            var canCancel = await _reservationService.CanUserCancelReservation(id, int.Parse(userId), isAdmin);
            if (!canCancel) return Forbid();
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();
            return View("/Views/Pages/Staff/FacilityReservation/Cancel.cshtml", reservation);
        }

        [HttpPost, ActionName("Cancel")]
        [Authorize(Roles = "staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");
            var canCancel = await _reservationService.CanUserCancelReservation(id, int.Parse(userId), isAdmin);
            if (!canCancel) return Forbid();
            var success = await _reservationService.UpdateReservationStatusAsync(id, "Cancelled", int.Parse(userId), isAdmin);
            if (!success) return Forbid();
            TempData["SuccessMessage"] = "Reservation cancelled successfully.";
            return RedirectToAction(nameof(FacilityReservationList));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");

            var canDelete = await _reservationService.CanUserModifyReservation(
                id, int.Parse(userId), isAdmin);

            if (!canDelete) return Forbid();

            return View("/Views/Pages/Admin/FacilityReservation/Delete.cshtml", reservation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");

            var success = await _reservationService.DeleteReservationAsync(
                id, int.Parse(userId), isAdmin);

            if (!success) return Forbid();

            TempData["SuccessMessage"] = "Reservation deleted successfully.";
            return RedirectToAction(nameof(FacilityReservationList));
        }

        private void LoadFacilitiesDropdown(int? selectedId = null)
        {
            ViewBag.FacilityId = new SelectList(_context.Facilities, "FacilityId", "FacilityName", selectedId);
        }

        // User-facing actions
        [AllowAnonymous]
        public async Task<IActionResult> MyReservations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyReservations") });
            }

            var viewModels = await _reservationService.GetUserReservationsAsync(int.Parse(userId));
            return View("/Views/Pages/User/FacilityReservation/MyReservations.cshtml", viewModels);
        }

        [AllowAnonymous]
        public IActionResult BookFacility()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("BookFacility") });
            }

            LoadFacilitiesDropdown();
            return View("/Views/Pages/User/FacilityReservation/BookFacility.cshtml", new FacilityReservationViewModel
            {
                ReservationDate = DateTime.Today,
                StartTime = new TimeSpan(9, 0, 0), // Default start time: 9:00 AM
                EndTime = new TimeSpan(10, 0, 0)   // Default end time: 10:00 AM
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> BookFacility([Bind("FacilityId,ReservationDate,StartTime,EndTime")] FacilityReservationViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                LoadFacilitiesDropdown(model.FacilityId);
                return View("/Views/Pages/User/FacilityReservation/BookFacility.cshtml", model);
            }

            var (success, errorMessage) = await _reservationService.CreateReservationAsync(model, int.Parse(userId));

            if (!success)
            {
                ModelState.AddModelError("", errorMessage);
                LoadFacilitiesDropdown(model.FacilityId);
                return View("/Views/Pages/User/FacilityReservation/BookFacility.cshtml", model);
            }

            TempData["SuccessMessage"] = "Facility booked successfully! Your reservation is pending approval.";
            return RedirectToAction(nameof(MyReservations));
        }

        public async Task<IActionResult> UserEdit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();

            if (reservation.CreatedBy != int.Parse(userId))
            {
                return Forbid();
            }

            LoadFacilitiesDropdown(reservation.FacilityId);
            return View("/Views/Pages/User/FacilityReservation/Edit.cshtml", reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdit(int id, [Bind("ReservationId,FacilityId,ReservationDate,StartTime,EndTime")] FacilityReservationViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != model.ReservationId) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadFacilitiesDropdown(model.FacilityId);
                return View("/Views/Pages/User/FacilityReservation/Edit.cshtml", model);
            }

            // Preserve status as "Pending" for user edits
            model.Status = "Pending";

            var (success, errorMessage) = await _reservationService.UpdateReservationAsync(
                id, model, int.Parse(userId), false);

            if (!success)
            {
                ModelState.AddModelError("", errorMessage);
                LoadFacilitiesDropdown(model.FacilityId);
                return View("/Views/Pages/User/FacilityReservation/Edit.cshtml", model);
            }

            TempData["SuccessMessage"] = "Reservation updated successfully!";
            return RedirectToAction(nameof(MyReservations));
        }

        public async Task<IActionResult> UserDelete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();

            if (reservation.CreatedBy != int.Parse(userId))
            {
                return Forbid();
            }

            return View("/Views/Pages/User/FacilityReservation/Delete.cshtml", reservation);
        }

        [HttpPost, ActionName("UserDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserDeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var success = await _reservationService.DeleteReservationAsync(
                id, int.Parse(userId), false);

            if (!success) return Forbid();

            TempData["SuccessMessage"] = "Reservation cancelled successfully!";
            return RedirectToAction(nameof(MyReservations));
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> List()
        {
            var reservations = await _reservationService.GetAllReservationsAsync();
            return View("/Views/Pages/Staff/FacilityReservation/List.cshtml", reservations);
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffDetails(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();
            return View("/Views/Pages/Staff/FacilityReservation/Details.cshtml", reservation);
        }
    }
}