using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public interface IServiceReqService
    {
        Task<List<ServiceRequest>> GetAllServiceRequestsAsync();
        Task<ServiceRequest> GetServiceRequestByIdAsync(int id);
        Task<(bool Success, string ErrorMessage)> CreateRequestAsync(ServiceReqViewModel model, int userId);
        Task<(bool Success, string ErrorMessage)> UpdateRequestAsync(int id, ServiceReqViewModel model, int userId, bool isAdmin);
        Task<bool> UpdateRequestStatusAsync(int id, string status);
        Task<bool> DeleteRequestAsync(int id, int userId, bool isAdmin);
        Task<bool> CanUserModifyRequest(int reservationId, int userId, bool isAdmin);

    }
}
