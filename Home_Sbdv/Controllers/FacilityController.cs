using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Home_Sbdv.Controllers
{
    [Authorize]
    public class FacilityController : Controller
    {
        private readonly AppDbContext _context;

        public FacilityController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> FacilityList()
        {
            var facility = await _context.Facilities
                .Select(f => new FacilityViewModel
                {
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName,
                    Description = f.Description,
                    Location = f.Location,
                    AvailabilityStatus = f.AvailabilityStatus,
                    UpdatedAt = f.UpdatedAt
                })
                .ToListAsync();

            return View(facility);
        }

        public async Task<IActionResult> Details(int id)
        {
            var facilityItem = await _context.Facilities
                .Select(f => new FacilityViewModel
                {
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName,
                    Description = f.Description,
                    Location = f.Location,
                    AvailabilityStatus = f.AvailabilityStatus,
                    UpdatedAt = f.UpdatedAt
                })
                .FirstOrDefaultAsync(f => f.FacilityId == id);

            if (facilityItem == null)
            {
                return NotFound();
            }
            return View(facilityItem);
        }

        public IActionResult Create()
        {
            // Populate the dropdown list for availability status
            ViewBag.AvailabilityStatusList = GetAvailabilityStatusList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FacilityName,Description,Location,AvailabilityStatus")] FacilityViewModel facilityModel)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, repopulate the dropdown before returning the view
                ViewBag.AvailabilityStatusList = GetAvailabilityStatusList();
                return View(facilityModel);
            }

            if (User.Identity == null || string.IsNullOrEmpty(User.Identity.Name))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }

            var newFacility = new Facilities
            {
                FacilityName = facilityModel.FacilityName,
                Description = facilityModel.Description,
                Location = facilityModel.Location,
                AvailabilityStatus = facilityModel.AvailabilityStatus,
            };

            _context.Facilities.Add(newFacility);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(FacilityList));
        }

        // Helper method to get the dropdown list items
        private List<SelectListItem> GetAvailabilityStatusList()
        {
            return new List<SelectListItem>
    {
        new SelectListItem { Value = "Available", Text = "Available" },
        new SelectListItem { Value = "Maintenance", Text = "Maintenance" },
        new SelectListItem { Value = "Closed", Text = "Closed" }
    };
        }

        public async Task<IActionResult> Edit(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);
            if (facility == null)
            {
                return NotFound();
            }

            var facilityModel = new FacilityViewModel
            {
                FacilityId = facility.FacilityId,
                FacilityName = facility.FacilityName,
                Description = facility.Description,
                Location = facility.Location,
                AvailabilityStatus = facility.AvailabilityStatus
            };

            ViewBag.AvailabilityStatusList = GetAvailabilityStatusList();
            return View(facilityModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FacilityId,FacilityName,Description,Location,AvailabilityStatus")] FacilityViewModel updatedFacility)
        {
            if (id != updatedFacility.FacilityId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingFacility = await _context.Facilities.FirstOrDefaultAsync(f => f.FacilityId == id);
                if (existingFacility == null)
                {
                    return NotFound();
                }

                // Only updating allowed fields
                existingFacility.FacilityName = updatedFacility.FacilityName;
                existingFacility.Description = updatedFacility.Description;
                existingFacility.Location = updatedFacility.Location;
                existingFacility.AvailabilityStatus = updatedFacility.AvailabilityStatus;
                existingFacility.UpdatedAt = DateTime.UtcNow;  // Ensure timestamp updates

                _context.Update(existingFacility);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(FacilityList));
            }

            // Populate availability status dropdown in case validation fails
            ViewBag.AvailabilityStatusList = GetAvailabilityStatusList();
            return View(updatedFacility);
        }



        public async Task<IActionResult> Delete(int id)
        {
            var facilityItem = await _context.Facilities
                .Select(f => new FacilityViewModel
                {
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName,
                    Description = f.Description,
                    Location = f.Location,
                    AvailabilityStatus = f.AvailabilityStatus,
                })
                .FirstOrDefaultAsync(f => f.FacilityId == id);

            if (facilityItem == null)
            {
                return NotFound();
            }

            return View(facilityItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facilityItem = await _context.Facilities.FindAsync(id);
            if (facilityItem != null)
            {
                _context.Facilities.Remove(facilityItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(FacilityList));
        }
    }
}      