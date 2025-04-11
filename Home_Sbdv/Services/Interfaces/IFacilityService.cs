using Home_Sbdv.Entities;
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
        Task<bool> CreateFacilityAsync(FacilityViewModel facilityModel, string username);
        Task<bool> UpdateFacilityAsync(int id, FacilityViewModel facilityModel);
        Task<bool> DeleteFacilityAsync(int id);
        List<SelectListItem> GetAvailabilityStatusList();

        // New methods for dashboard
        Task<List<FacilityReservation>> GetRecentReservationsAsync(int count);
        Task<List<FacilityReservation>> GetUserReservationsAsync(string userId);
    }
}