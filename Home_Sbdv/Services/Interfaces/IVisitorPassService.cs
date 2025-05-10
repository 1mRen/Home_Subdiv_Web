using Home_Sbdv.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home_Sbdv.Services.Interfaces
{
    public interface IVisitorPassService
    {
        Task<List<VisitorPassRequestViewModel>> GetAllAsync();
        Task<List<VisitorPassRequestViewModel>> GetAllRequestsAsync();
        Task<List<VisitorPassRequestViewModel>> GetByUserAsync(int userId);
        Task<VisitorPassRequestViewModel?> GetByIdAsync(int id);
        Task<bool> CreateAsync(VisitorPassRequestViewModel model);
        Task<bool> ApproveAsync(int id, int approverUserId);
        Task<bool> DeclineAsync(int id, int approverUserId);
        Task<bool> CheckInAsync(int id, int staffUserId);
        Task<bool> CheckOutAsync(int id, int staffUserId);
        Task<bool> CancelAsync(int id, int userId);
    }
}