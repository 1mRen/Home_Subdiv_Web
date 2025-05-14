using System.Collections.Generic;
using System.Threading.Tasks;
using Home_Sbdv.Models;

namespace Home_Sbdv.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationViewModel>> GetUserNotificationsAsync(int userId, string userRole);
        Task<int> GetUnreadCountAsync(int userId, string userRole);
        Task MarkAsReadAsync(int notificationId, string userRole);
        Task MarkAllAsReadAsync(int userId, string userRole);
        Task DeleteNotificationAsync(int notificationId, string userRole);
        Task CreateNotificationAsync(int userId, string message, string type, string link = null);
        Task<bool> IsNotificationOwnerAsync(int notificationId, int userId);
        Task<NotificationViewModel> GetNotificationByIdAsync(int notificationId);
        Task<List<NotificationViewModel>> GetNotificationsByTypeAsync(int userId, string type);
        Task<int> GetUnreadCountByTypeAsync(int userId, string type);

        // Helper methods for specific notification types
        Task NotifyServiceRequestSubmitted(int requestId, int userId, string requestType);
        Task NotifyServiceRequestStatusChanged(int requestId, int userId, string status, string requestType);
        Task NotifyFeedbackSubmitted(int feedbackId, int userId, string feedbackType);
        Task NotifyFeedbackResponse(int feedbackId, int userId, string staffResponse);
        Task NotifyAnnouncementCreated(int announcementId, string title);
        Task NotifyVisitorPassRequest(int requestId, int userId, string status);
        Task NotifyFacilityReservation(int reservationId, int userId, string status);
        Task NotifySystemAlert(string message, string? link = null);
        Task NotifyAdminUserRegistration(int newUserId, string newUserName);
        Task NotifyEventCreated(int eventId, string eventTitle);
        Task NotifyNewFacility(int facilityId, string facilityName);
        Task NotifyFeedbackUnassignedFromStaff(int feedbackId, int staffUserId, string feedbackType);
        Task NotifyFeedbackStatusChanged(int feedbackId, int userId, string status);
        Task NotifyAdminsFeedbackAssigned(int feedbackId, string staffName);
        Task NotifyUserFeedbackAssigned(int feedbackId, int userId, string staffName);
        Task NotifyAdminsFeedbackResolvedOrClosed(int feedbackId, string status);
        Task NotifyUserFeedbackDeleted(int feedbackId, int userId);
        Task NotifyAdminsFeedbackDeleted(int feedbackId);
        Task NotifyFeedbackAssignedToStaff(int feedbackId, int staffUserId, string feedbackType);
    }
} 