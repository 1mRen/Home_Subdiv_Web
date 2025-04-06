using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Home_Sbdv.Models;
using Home_Sbdv.Services;
using System.Threading.Tasks;
using Home_Sbdv.Data;
using Microsoft.Extensions.Logging;

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
            return View(viewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();
            return View(reservation);
        }

        public IActionResult Create()
        {
            LoadFacilitiesDropdown();
            return View();
        }

        [HttpPost]
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
                return View(model);
            }

            var (success, errorMessage) = await _reservationService.CreateReservationAsync(model, int.Parse(userId));

            if (!success)
            {
                ModelState.AddModelError("", errorMessage);
                LoadFacilitiesDropdown(model.FacilityId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Reservation created successfully.";
            return RedirectToAction(nameof(FacilityReservationList));
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
            return View(reservation);
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
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var success = await _reservationService.UpdateReservationStatusAsync(id, status);
            if (!success) return NotFound();

            TempData["SuccessMessage"] = "Reservation status updated.";
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

            return View(reservation);
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
    }
}