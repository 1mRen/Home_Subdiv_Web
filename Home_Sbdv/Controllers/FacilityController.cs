using Home_Sbdv.Models;
using Home_Sbdv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Home_Sbdv.Constants;

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
            return View("/Views/Pages/Admin/Facility/FacilityList.cshtml", facilities);
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
        public async Task<IActionResult> Create([Bind("FacilityName,Description,Location,Capacity,AvailabilityStatus,ImageFile")] FacilityViewModel facilityModel)
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

            // Handle image upload
            if (facilityModel.ImageFile != null && facilityModel.ImageFile.Length > 0)
            {
                if (!Directory.Exists(FilePaths.FacilityImages))
                    Directory.CreateDirectory(FilePaths.FacilityImages);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(facilityModel.ImageFile.FileName);
                var filePath = Path.Combine(FilePaths.FacilityImages, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await facilityModel.ImageFile.CopyToAsync(stream);
                }
                
                facilityModel.ImageUrl = FilePaths.GetRelativePath(filePath);
            }

            var success = await _facilityService.CreateFacilityAsync(facilityModel);
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

        // Non-admin user facility views
        [AllowAnonymous] // Or use appropriate authorization attribute for regular users
        public async Task<IActionResult> ViewAll()
        {
            var facilities = await _facilityService.GetAllFacilitiesAsync();
            return View("/Views/Pages/User/Facility/ViewAll.cshtml", facilities);
        }

        [AllowAnonymous] // Or use appropriate authorization attribute for regular users
        public async Task<IActionResult> View(int id)
        {
            var facility = await _facilityService.GetFacilityByIdAsync(id);
            if (facility == null)
            {
                return NotFound();
            }
            return View("/Views/Pages/User/Facility/View.cshtml", facility);
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffFacilityList()
        {
            var facilities = await _facilityService.GetAllFacilitiesAsync();
            return View("/Views/Pages/Staff/Facility/FacilityList.cshtml", facilities);
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffDetails(int id)
        {
            var facility = await _facilityService.GetFacilityByIdAsync(id);
            if (facility == null)
            {
                return NotFound();
            }
            return View("/Views/Pages/Staff/Facility/Details.cshtml", facility);
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> EditStatus(int id)
        {
            var facility = await _facilityService.GetFacilityByIdAsync(id);
            if (facility == null)
            {
                return NotFound();
            }
            ViewBag.AvailabilityStatusList = _facilityService.GetAvailabilityStatusList();
            return View("/Views/Pages/Staff/Facility/EditStatus.cshtml", facility);
        }

        [HttpPost]
        [Authorize(Roles = "staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, [Bind("FacilityId,AvailabilityStatus")] FacilityViewModel model)
        {
            var facility = await _facilityService.GetFacilityByIdAsync(id);
            if (facility == null)
            {
                return NotFound();
            }
            // Only update status
            facility.AvailabilityStatus = model.AvailabilityStatus;
            await _facilityService.UpdateFacilityAsync(id, facility);
            TempData["Success"] = "Facility status updated.";
            return RedirectToAction("StaffFacilityList");
        }
    }
}