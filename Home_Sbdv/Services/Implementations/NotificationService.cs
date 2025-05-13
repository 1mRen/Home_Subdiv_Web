using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using Home_Sbdv.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Home_Sbdv.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<NotificationViewModel>> GetUserNotificationsAsync(int userId, string userRole)
        {
            return await _context.Notifications
                .Where(n =>
                    (n.UserId == userId) ||
                    (!string.IsNullOrEmpty(n.Role) && n.Role.ToLower() == userRole) ||
                    n.IsGlobal
                )
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    Link = n.Link
                })
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId, string userRole)
        {
            return await _context.Notifications
                .CountAsync(n =>
                    !n.IsRead &&
                    (
                        (n.UserId == userId) ||
                        (!string.IsNullOrEmpty(n.Role) && n.Role.ToLower() == userRole) ||
                        n.IsGlobal
                    )
                );
        }

        public async Task MarkAsReadAsync(int notificationId, string userRole)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId, string userRole)
        {
            var notifications = await _context.Notifications
                .Where(n =>
                    !n.IsRead &&
                    (
                        (n.UserId == userId) ||
                        (!string.IsNullOrEmpty(n.Role) && n.Role.ToLower() == userRole) ||
                        n.IsGlobal
                    )
                )
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteNotificationAsync(int notificationId, string userRole)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CreateNotificationAsync(int userId, string message, string type, string link = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Link = link
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsNotificationOwnerAsync(int notificationId, int userId)
        {
            return await _context.Notifications
                .AnyAsync(n => n.Id == notificationId && n.UserId == userId);
        }

        public async Task<NotificationViewModel> GetNotificationByIdAsync(int notificationId)
        {
            var notification = await _context.Notifications
                .Where(n => n.Id == notificationId)
                .Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    Link = n.Link
                })
                .FirstOrDefaultAsync();

            return notification;
        }

        public async Task<List<NotificationViewModel>> GetNotificationsByTypeAsync(int userId, string type)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && n.Type == type)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    Link = n.Link
                })
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountByTypeAsync(int userId, string type)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && n.Type == type && !n.IsRead);
        }

        // Helper methods for specific notification types
        public async Task NotifyServiceRequestSubmitted(int requestId, int userId, string requestType)
        {
            var user = await _context.Users.FindAsync(userId);
            string role = user?.Role?.ToLower();
            var message = $"New {requestType} service request has been submitted.";
            var link = (role == "admin" || role == "staff")
                ? $"/ServiceReq/Details/{requestId}"
                : $"/ServiceReq/View/{requestId}";
            await CreateNotificationAsync(userId, message, "ServiceRequest", link);
        }

        public async Task NotifyServiceRequestStatusChanged(int requestId, int userId, string status, string requestType)
        {
            var user = await _context.Users.FindAsync(userId);
            string role = user?.Role?.ToLower();
            var message = $"Your {requestType} service request status has been updated to {status}.";
            var link = (role == "admin" || role == "staff")
                ? $"/ServiceReq/Details/{requestId}"
                : $"/ServiceReq/View/{requestId}";
            await CreateNotificationAsync(userId, message, "ServiceRequest", link);
        }

        public async Task NotifyFeedbackSubmitted(int feedbackId, int userId, string feedbackType)
        {
            var message = $"New {feedbackType} feedback has been submitted.";
            // Notify all admins
            var adminUsers = await _context.Users.Where(u => u.Role.ToLower() == "admin").ToListAsync();
            foreach (var admin in adminUsers)
            {
                await CreateNotificationAsync(admin.Id, message, "Feedback", $"/Admin/Feedback/Details/{feedbackId}");
            }
        }

        public async Task NotifyFeedbackResponse(int feedbackId, int userId, string staffResponse)
        {
            var message = $"You have received a response to your feedback: {staffResponse}";
            var link = $"/User/Feedback/Details/{feedbackId}";
            await CreateNotificationAsync(userId, message, "Feedback", link);
        }

        public async Task NotifyAnnouncementCreated(int announcementId, string title)
        {
            var message = $"New announcement: {title}";
            var users = await _context.Users.ToListAsync();
            foreach (var user in users)
            {
                string role = user.Role?.ToLower();
                string link = role switch
                {
                    "admin" => $"/Admin/Announcement/Details/{announcementId}",
                    "staff" => $"/Staff/Announcement/Details/{announcementId}",
                    _ => $"/User/Announcement/Details/{announcementId}"
                };
                await CreateNotificationAsync(user.Id, message, "Announcement", link);
            }
        }

        public async Task NotifyVisitorPassRequest(int requestId, int userId, string status)
        {
            var user = await _context.Users.FindAsync(userId);
            string role = user?.Role?.ToLower();
            var message = $"Your visitor pass request has been {status.ToLower()}.";
            var link = (role == "admin" || role == "staff")
                ? $"/VisitorPass/Details/{requestId}"
                : $"/VisitorPass/View/{requestId}";
            await CreateNotificationAsync(userId, message, "VisitorPass", link);
        }

        public async Task NotifyFacilityReservation(int reservationId, int userId, string status)
        {
            var user = await _context.Users.FindAsync(userId);
            string role = user?.Role?.ToLower();
            var message = $"Your facility reservation has been {status.ToLower()}.";
            var link = role switch
            {
                "admin" => $"/Admin/FacilityReservation/Details/{reservationId}",
                "staff" => $"/Staff/FacilityReservation/Details/{reservationId}",
                _ => $"/User/FacilityReservation/Details/{reservationId}"
            };
            await CreateNotificationAsync(userId, message, "FacilityReservation", link);
        }

        public async Task NotifySystemAlert(string message, string? link = null)
        {
            // Find all admin users
            var adminUsers = await _context.Users.Where(u => u.Role.ToLower() == "admin").ToListAsync();
            foreach (var admin in adminUsers)
            {
                await CreateNotificationAsync(admin.Id, message, "SystemAlert", link);
            }
        }

        public async Task NotifyAdminUserRegistration(int newUserId, string newUserName)
        {
            var message = $"New user registered: {newUserName} (ID: {newUserId})";
            // Find all admin users
            var adminUsers = await _context.Users.Where(u => u.Role.ToLower() == "admin").ToListAsync();
            foreach (var admin in adminUsers)
            {
                await CreateNotificationAsync(admin.Id, message, "UserRegistration");
            }
        }

        public async Task NotifyEventCreated(int eventId, string eventTitle)
        {
            var message = $"New event: {eventTitle}";
            var users = await _context.Users.ToListAsync();
            foreach (var user in users)
            {
                string role = user.Role?.ToLower();
                string link = role switch
                {
                    "admin" => $"/Admin/Event/Details/{eventId}",
                    "staff" => $"/Staff/Event/Details/{eventId}",
                    _ => $"/User/Event/Details/{eventId}"
                };
                await CreateNotificationAsync(user.Id, message, "Event", link);
            }
        }

        public async Task NotifyNewFacility(int facilityId, string facilityName)
        {
            var message = $"New facility available: {facilityName}";
            var users = await _context.Users.ToListAsync();
            foreach (var user in users)
            {
                string role = user.Role?.ToLower();
                string link = role switch
                {
                    "admin" => $"/Facility/Details/{facilityId}",
                    "staff" => $"/Facility/StaffDetails/{facilityId}",
                    _ => $"/Facility/View/{facilityId}"
                };
                await CreateNotificationAsync(user.Id, message, "Facility", link);
            }
        }

        public async Task NotifyFeedbackAssignedToStaff(int feedbackId, int staffUserId, string feedbackType)
        {
            var message = $"You have been assigned a new {feedbackType} feedback to handle.";
            var link = $"/Staff/Feedback/Details/{feedbackId}";
            await CreateNotificationAsync(staffUserId, message, "FeedbackAssignment", link);
        }

        public async Task NotifyFeedbackUnassignedFromStaff(int feedbackId, int staffUserId, string feedbackType)
        {
            var message = $"You have been unassigned from {feedbackType} feedback.";
            var link = $"/Staff/Feedback/Details/{feedbackId}";
            await CreateNotificationAsync(staffUserId, message, "FeedbackUnassignment", link);
        }

        public async Task NotifyFeedbackStatusChanged(int feedbackId, int userId, string status)
        {
            var message = $"Your feedback status has been updated to {status}.";
            var link = $"/User/Feedback/Details/{feedbackId}";
            await CreateNotificationAsync(userId, message, "FeedbackStatus", link);
        }

        public async Task NotifyAdminsFeedbackAssigned(int feedbackId, string staffName)
        {
            var admins = await _context.Users.Where(u => u.Role.ToLower() == "admin").ToListAsync();
            foreach (var admin in admins)
            {
                await CreateNotificationAsync(
                    admin.Id,
                    $"Feedback #{feedbackId} has been assigned to {staffName}.",
                    "FeedbackAssignment",
                    $"/Admin/Feedback/Details/{feedbackId}"
                );
            }
        }

        public async Task NotifyUserFeedbackAssigned(int feedbackId, int userId, string staffName)
        {
            await CreateNotificationAsync(
                userId,
                $"Your feedback has been assigned to {staffName}.",
                "FeedbackAssignment",
                $"/User/Feedback/Details/{feedbackId}"
            );
        }

        public async Task NotifyAdminsFeedbackResolvedOrClosed(int feedbackId, string status)
        {
            var admins = await _context.Users.Where(u => u.Role.ToLower() == "admin").ToListAsync();
            foreach (var admin in admins)
            {
                await CreateNotificationAsync(
                    admin.Id,
                    $"Feedback #{feedbackId} has been marked as {status}.",
                    "FeedbackStatus",
                    $"/Admin/Feedback/Details/{feedbackId}"
                );
            }
        }

        public async Task NotifyUserFeedbackDeleted(int feedbackId, int userId)
        {
            await CreateNotificationAsync(
                userId,
                $"Your feedback #{feedbackId} has been deleted by an admin.",
                "FeedbackDeleted",
                null
            );
        }

        public async Task NotifyAdminsFeedbackDeleted(int feedbackId)
        {
            var admins = await _context.Users.Where(u => u.Role.ToLower() == "admin").ToListAsync();
            foreach (var admin in admins)
            {
                await CreateNotificationAsync(
                    admin.Id,
                    $"Feedback #{feedbackId} has been deleted.",
                    "FeedbackDeleted",
                    null
                );
            }
        }
    }
} 