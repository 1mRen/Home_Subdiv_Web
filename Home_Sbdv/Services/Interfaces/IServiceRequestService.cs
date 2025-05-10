using Home_Sbdv.Models;
using Microsoft.AspNetCore.Http;

namespace Home_Sbdv.Services.Interfaces
{
    public interface IServiceRequestService
    {
        Task<ServiceResult<List<ServiceReqViewModel>>> GetAllRequestsAsync();
        Task<ServiceResult<ServiceReqViewModel>> GetRequestByIdAsync(int id);
        Task<ServiceResult<ServiceReqViewModel>> CreateRequestAsync(ServiceReqViewModel model, IFormFile? imageFile);
        Task<ServiceResult<ServiceReqViewModel>> UpdateRequestAsync(int id, ServiceReqViewModel model, IFormFile? imageFile);
        Task<ServiceResult<bool>> DeleteRequestAsync(int id);
        Task<ServiceResult<ServiceReqViewModel>> ApproveRequestAsync(int id);
        Task<ServiceResult<ServiceReqViewModel>> RejectRequestAsync(int id);
        Task<ServiceResult<ServiceReqViewModel>> UpdateStatusAsync(int id, string status);
        Task<ServiceResult<List<ServiceReqViewModel>>> GetUserRequestsAsync(int userId);
        Task<ServiceResult<bool>> CreateServiceRequestAsync(ServiceReqViewModel model);
    }
} 