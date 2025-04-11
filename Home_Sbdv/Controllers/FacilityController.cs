using Home_Sbdv.Models;
using Home_Sbdv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Home_Sbdv.Controllers
{
    [Authorize]
    public class FacilityController : Controller
    {
        private readonly IFacilityService _facilityService;

        public FacilityController(IFacilityService facilityService)
        {
            _facilityService = facilityService;
        }

        public async Task<IActionResult> FacilityList()
        {
            var facilities = await _facilityService.GetAllFacilitiesAsync();
            return View("/Views/Pages/Admin/Facility/FacilityList.cshtml" ,facilities);
        }

        public async Task<IActionResult> Details(int id)
        {
            var facilityItem = await _facilityService.GetFacilityByIdAsync(id);
            if (facilityItem == null)
            {
                return NotFound();
            }
            return View("/Views/Pages/Admin/Facility/Details.cshtml", facilityItem);
        }

        public IActionResult Create()
        {
            ViewBag.AvailabilityStatusList = _facilityService.GetAvailabilityStatusList();
            return View("/Views/Pages/Admin/Facility/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FacilityName,Description,Location,AvailabilityStatus")] FacilityViewModel facilityModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AvailabilityStatusList = _facilityService.GetAvailabilityStatusList();
                return View(facilityModel);
            }

            if (User.Identity == null || string.IsNullOrEmpty(User.Identity.Name))
            {
                return Unauthorized();
            }

            var success = await _facilityService.CreateFacilityAsync(facilityModel, User.Identity.Name);
            if (!success)
            {
                return Unauthorized();
            }

            return RedirectToAction(nameof(FacilityList));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var facilityModel = await _facilityService.GetFacilityByIdAsync(id);
            if (facilityModel == null)
            {
                return NotFound();
            }

            ViewBag.AvailabilityStatusList = _facilityService.GetAvailabilityStatusList();
            return View("/Views/Pages/Admin/Facility/Edit.cshtml", facilityModel);
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
                var success = await _facilityService.UpdateFacilityAsync(id, updatedFacility);
                if (!success)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(FacilityList));
            }

            ViewBag.AvailabilityStatusList = _facilityService.GetAvailabilityStatusList();
            return View("/Views/Pages/Admin/Facility/FacilityEdit.cshtml", updatedFacility);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var facilityItem = await _facilityService.GetFacilityByIdAsync(id);
            if (facilityItem == null)
            {
                return NotFound();
            }

            return View("/Views/Pages/Admin/Facility/Delete.cshtml", facilityItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _facilityService.DeleteFacilityAsync(id);
            return RedirectToAction(nameof(FacilityList));
        }
    }
}