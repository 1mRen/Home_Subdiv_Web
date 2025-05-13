using System;
using System.Threading.Tasks;
using Home_Sbdv.Models;
using Home_Sbdv.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace Home_Sbdv.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public async Task<IActionResult> GetNotifications(int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value?.ToLower();
                var notifications = await _notificationService.GetUserNotificationsAsync(userId.Value, userRole);
                var paginatedNotifications = notifications
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                return Json(new
                {
                    Notifications = paginatedNotifications,
                    TotalCount = notifications.Count,
                    CurrentPage = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching notifications" });
            }
        }

        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value?.ToLower();
                var count = await _notificationService.GetUnreadCountAsync(userId.Value, userRole);
                return Json(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching unread count" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid notification ID" });
                }

                var userId = GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value?.ToLower();
                await _notificationService.MarkAsReadAsync(id, userRole);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while marking notification as read" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value?.ToLower();
                await _notificationService.MarkAllAsReadAsync(userId.Value, userRole);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while marking all notifications as read" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid notification ID" });
                }

                var userId = GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value?.ToLower();
                await _notificationService.DeleteNotificationAsync(id, userRole);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting notification" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value?.ToLower();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId.Value, userRole);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId.Value, userRole);
            var model = new NotificationListViewModel
            {
                Notifications = notifications,
                UnreadCount = unreadCount
            };

            if (userRole == "admin")
                return View("~/Views/Pages/Notification/IndexAdmin.cshtml", model);
            if (userRole == "staff")
                return View("~/Views/Pages/Notification/IndexStaff.cshtml", model);
            // Default to user/homeowner
            return View("~/Views/Pages/Notification/Index.cshtml", model);
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }
            return userId;
        }
    }
} 