using Home_Sbdv.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public interface IFacilityReservationService
    {
        Task<List<FacilityReservationViewModel>> GetAllReservationsAsync();
        Task<FacilityReservationViewModel> GetReservationByIdAsync(int id);
        Task<(bool Success, string ErrorMessage)> CreateReservationAsync(FacilityReservationViewModel model, int userId);
        Task<(bool Success, string ErrorMessage)> UpdateReservationAsync(int id, FacilityReservationViewModel model, int userId, bool isAdmin);
        Task<bool> UpdateReservationStatusAsync(int id, string status);
        Task<bool> DeleteReservationAsync(int id, int userId, bool isAdmin);
        Task<bool> CanUserModifyReservation(int reservationId, int userId, bool isAdmin);

        // New methods for dashboard
        Task<List<FacilityReservationViewModel>> GetRecentReservationsAsync(int count);
        Task<List<FacilityReservationViewModel>> GetUserReservationsAsync(int userId);
    }
}