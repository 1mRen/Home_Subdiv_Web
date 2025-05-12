using Home_Sbdv.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public interface IFeedbackService
    {
        Task<List<FeedbackViewModel>> GetAllFeedbackAsync();
        Task<FeedbackViewModel> GetFeedbackByIdAsync(int id);
        Task<bool> CreateFeedbackAsync(FeedbackViewModel feedbackModel);
        Task<bool> UpdateFeedbackAsync(int id, FeedbackViewModel feedbackModel);
        Task<bool> DeleteFeedbackAsync(int id);
        Task<bool> UpdateFeedbackStatusAsync(int id, FeedbackResponseViewModel responseModel);
        Task<bool> AssignFeedbackAsync(int id, int staffId);
        Task<List<FeedbackViewModel>> GetUserFeedbackAsync(int userId);
        Task<List<FeedbackViewModel>> GetAssignedFeedbackAsync(int staffId);
        Task<List<FeedbackViewModel>> GetRecentFeedbackAsync(int count);
        List<SelectListItem> GetFeedbackTypeList();
        List<SelectListItem> GetFeedbackStatusList();

        // New methods for view models
        Task<FeedbackListViewModel> GetFeedbackListViewModelAsync();
        Task<FeedbackListViewModel> GetUserFeedbackListViewModelAsync(int userId);
        Task<FeedbackListViewModel> GetAssignedFeedbackListViewModelAsync(int staffId);
        Task<FeedbackStatisticsViewModel> GetFeedbackStatisticsViewModelAsync();
    }
}