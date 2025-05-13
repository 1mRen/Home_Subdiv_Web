using Home_Sbdv.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public interface IFacilityService
    {
        Task<List<FacilityViewModel>> GetAllFacilitiesAsync();
        Task<FacilityViewModel> GetFacilityByIdAsync(int id);
        Task<bool> CreateFacilityAsync(FacilityViewModel facilityModel);
        Task<bool> UpdateFacilityAsync(int id, FacilityViewModel facilityModel);
        Task<bool> DeleteFacilityAsync(int id);
        List<SelectListItem> GetAvailabilityStatusList();

        // Updated return types to use view models
        Task<List<FacilityReservationViewModel>> GetRecentReservationsAsync(int count);
        Task<List<FacilityReservationViewModel>> GetUserReservationsAsync(string userId);

        Task<FacilityViewModel> GetFacilityByNameAsync(string facilityName);
    }
}