using System.Security.Claims;
using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Home_Sbdv.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

[Authorize]
public class FacilityReservationController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<FacilityReservationController> _logger;

    public FacilityReservationController(AppDbContext context, ILogger<FacilityReservationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> FacilityReservationList()
    {
        var reservations = await _context.FacilityReservations
            .Include(r => r.User)
            .Include(r => r.Facility)
            .ToListAsync();

        var viewModels = reservations.Select(ProjectToViewModel).ToList();

        return View(viewModels);
    }

    public async Task<IActionResult> Details(int id)
    {
        var reservation = await _context.FacilityReservations
            .Include(r => r.User)
            .Include(r => r.Facility)
            .FirstOrDefaultAsync(r => r.ReservationId == id);

        if (reservation == null) return NotFound();
        return View(ProjectToViewModel(reservation));
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

        var conflict = await _context.FacilityReservations
            .FirstOrDefaultAsync(r =>
                r.FacilityId == model.FacilityId &&
                r.ReservationDate == model.ReservationDate &&
                model.StartTime < r.EndTime && model.EndTime > r.StartTime);

        if (conflict != null)
        {
            ModelState.AddModelError("", $"Conflict detected: This facility is already reserved from {conflict.StartTime:t} to {conflict.EndTime:t}.");
            LoadFacilitiesDropdown(model.FacilityId);
            return View(model);
        }

        var reservation = new FacilityReservation
        {
            FacilityId = model.FacilityId,
            ReservationDate = model.ReservationDate,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            Status = "Pending",
            UserId = int.Parse(userId)
        };

        _context.Add(reservation);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Reservation created successfully.";
        return RedirectToAction(nameof(FacilityReservationList));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var reservation = await _context.FacilityReservations
            .Include(r => r.Facility)
            .FirstOrDefaultAsync(r => r.ReservationId == id);

        if (reservation == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("admin") && reservation.UserId != int.Parse(userId))
            return Forbid();

        var model = ProjectToViewModel(reservation);
        LoadFacilitiesDropdown(reservation.FacilityId);
        return View(model);
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

        var reservation = await _context.FacilityReservations.FindAsync(id);
        if (reservation == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("admin") && reservation.UserId != int.Parse(userId))
            return Forbid();

        var conflict = await _context.FacilityReservations
            .FirstOrDefaultAsync(r =>
                r.ReservationId != model.ReservationId &&
                r.FacilityId == model.FacilityId &&
                r.ReservationDate == model.ReservationDate &&
                model.StartTime < r.EndTime && model.EndTime > r.StartTime);

        if (conflict != null)
        {
            ModelState.AddModelError("", $"Conflict detected: This facility is already reserved from {conflict.StartTime:t} to {conflict.EndTime:t}.");
            LoadFacilitiesDropdown(model.FacilityId);
            return View(model);
        }

        reservation.FacilityId = model.FacilityId;
        reservation.ReservationDate = model.ReservationDate;
        reservation.StartTime = model.StartTime;
        reservation.EndTime = model.EndTime;
        reservation.Status = model.Status;

        try
        {
            _context.Update(reservation);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Reservation updated successfully.";
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError("", "Unable to save changes. The reservation was modified by another user.");
            LoadFacilitiesDropdown(model.FacilityId);
            return View(model);
        }

        return RedirectToAction(nameof(FacilityReservationList));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var reservation = await _context.FacilityReservations.FindAsync(id);
        if (reservation == null) return NotFound();

        reservation.Status = status;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Reservation status updated.";
        return RedirectToAction(nameof(FacilityReservationList));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var reservation = await _context.FacilityReservations
            .Include(r => r.User)
            .Include(r => r.Facility)
            .FirstOrDefaultAsync(r => r.ReservationId == id);

        if (reservation == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("admin") && reservation.UserId != int.Parse(userId))
            return Forbid();

        return View(ProjectToViewModel(reservation));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var reservation = await _context.FacilityReservations.FindAsync(id);
        if (reservation != null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("admin") && reservation.UserId != int.Parse(userId))
                return Forbid();

            _context.FacilityReservations.Remove(reservation);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Reservation deleted successfully.";
        }
        return RedirectToAction(nameof(FacilityReservationList));
    }

    private void LoadFacilitiesDropdown(int? selectedId = null)
    {
        ViewBag.FacilityId = new SelectList(_context.Facilities, "FacilityId", "FacilityName", selectedId);
    }

    private FacilityReservationViewModel ProjectToViewModel(FacilityReservation reservation)
    {
        return new FacilityReservationViewModel
        {
            ReservationId = reservation.ReservationId,
            FacilityId = reservation.FacilityId,
            FacilityName = reservation.Facility?.FacilityName ?? "Unknown",
            ReservationDate = reservation.ReservationDate,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            Status = reservation.Status,
            CreatedBy = reservation.User?.Id ?? 0,
            CreatedByName = reservation.User?.FullName ?? "Unknown"
        };
    }
}
