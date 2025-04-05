using System.Security.Claims;
using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Home_Sbdv.Models;

[Authorize]
public class FacilityReservationController : Controller
{
    private readonly AppDbContext _context;

    public FacilityReservationController(AppDbContext context)
    {
        _context = context;
    }

    // 🟢 1. List all reservations
    public async Task<IActionResult> FacilityReservationList()
    {
        var reservations = await _context.FacilityReservations
            .Include(r => r.User)
            .Include(r => r.Facility)
            .Select(r => new FacilityReservationViewModel
            {
                ReservationId = r.ReservationId,
                FacilityId = r.FacilityId,
                FacilityName = r.Facility != null ? r.Facility.FacilityName : "Unknown",
                ReservationDate = r.ReservationDate,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status,
                CreatedBy = r.User != null ? r.User.Id : 0,
                CreatedByName = r.User != null ? r.User.FullName : "Unknown"
            })
            .ToListAsync();

        return View("FacilityReservationList", reservations);
    }

    // 🟢 2. View details
    public async Task<IActionResult> Details(int id)
    {
        var reservation = await _context.FacilityReservations
            .Include(r => r.User)
            .Include(r => r.Facility)
            .Select(r => new FacilityReservationViewModel
            {
                ReservationId = r.ReservationId,
                FacilityId = r.FacilityId,
                FacilityName = r.Facility != null ? r.Facility.FacilityName : "Unknown",
                ReservationDate = r.ReservationDate,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status,
                CreatedBy = r.User != null ? r.User.Id : 0,
                CreatedByName = r.User != null ? r.User.FullName : "Unknown"
            })
            .FirstOrDefaultAsync(r => r.ReservationId == id);

        if (reservation == null) return NotFound();
        return View("Details", reservation);
    }

    // 🟢 3. Create GET
    public IActionResult Create()
    {
        LoadFacilitiesDropdown(); // 👇 Refactor for reuse
        return View("Create");
    }

    // 🟢 4. Create POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FacilityId,ReservationDate,StartTime,EndTime")] FacilityReservationViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // 🧼 Cleaner
        if (userId == null) return Unauthorized();

        if (ModelState.IsValid)
        {
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
            return RedirectToAction(nameof(FacilityReservationList));
        }

        LoadFacilitiesDropdown(model.FacilityId);
        return View("Create", model);
    }

    // 🟢 5. Edit GET
    public async Task<IActionResult> Edit(int id)
    {
        var reservation = await _context.FacilityReservations
            .Include(r => r.Facility)
            .FirstOrDefaultAsync(r => r.ReservationId == id);
        if (reservation == null) return NotFound();

        var model = new FacilityReservationViewModel
        {
            ReservationId = reservation.ReservationId,
            FacilityId = reservation.FacilityId,
            FacilityName = reservation.Facility?.FacilityName ?? "Unknown",
            ReservationDate = reservation.ReservationDate,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            Status = reservation.Status
        };

        LoadFacilitiesDropdown(reservation.FacilityId);
        return View("Edit", model);
    }

    // 🟢 6. Edit POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ReservationId,FacilityId,ReservationDate,StartTime,EndTime,Status")] FacilityReservationViewModel model)
    {
        if (id != model.ReservationId) return NotFound();

        if (ModelState.IsValid)
        {
            var reservation = await _context.FacilityReservations.FindAsync(id);
            if (reservation == null) return NotFound();

            reservation.FacilityId = model.FacilityId;
            reservation.ReservationDate = model.ReservationDate;
            reservation.StartTime = model.StartTime;
            reservation.EndTime = model.EndTime;
            reservation.Status = model.Status;

            _context.Update(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(FacilityReservationList));
        }

        LoadFacilitiesDropdown(model.FacilityId);
        return View("Edit", model);
    }

    // 🟢 7. Update status (Admin only)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var reservation = await _context.FacilityReservations.FindAsync(id);
        if (reservation == null) return NotFound();

        reservation.Status = status;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(FacilityReservationList));
    }

    // 🟢 8. Delete GET
    public async Task<IActionResult> Delete(int id)
    {
        var reservation = await _context.FacilityReservations
            .Include(r => r.Facility)
            .FirstOrDefaultAsync(r => r.ReservationId == id);
        if (reservation == null) return NotFound();

        return View("Delete", reservation);
    }

    // 🟢 9. Delete POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var reservation = await _context.FacilityReservations.FindAsync(id);
        if (reservation != null)
        {
            _context.FacilityReservations.Remove(reservation);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(FacilityReservationList));
    }

    // 🔁 Shared helper for dropdown
    private void LoadFacilitiesDropdown(int? selectedId = null)
    {
        ViewBag.FacilityId = new SelectList(_context.Facilities, "FacilityId", "FacilityName", selectedId);
    }
}
