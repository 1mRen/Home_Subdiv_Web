using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Home_Sbdv.Attributes;
using Home_Sbdv.Models;
using Home_Sbdv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home_Sbdv.Controllers
{
    [RequireHttps]
    [Authorize] // Basic authorization for all dashboard access
    public class DashboardController : Controller
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IAnnouncementService _announcementService;
        private readonly IEventService _eventService;
        private readonly IFacilityService _facilityService;
        private readonly IFacilityReservationService _facilityReservationService;

        public DashboardController(
            IUserManagementService userManagementService,
            IAnnouncementService announcementService,
            IEventService eventService,
            IFacilityService facilityService,
            IFacilityReservationService facilityReservationService)
        {
            _userManagementService = userManagementService;
            _announcementService = announcementService;
            _eventService = eventService;
            _facilityService = facilityService;
            _facilityReservationService = facilityReservationService;
        }

        // Generic entry point that redirects based on role
        public IActionResult Index()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            return userRole?.ToLower() switch
            {
                "admin" => RedirectToAction("AdminDashboard"),
                "staff" => RedirectToAction("StaffDashboard"),
                "homeowner" => RedirectToAction("HomeownerDashboard"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // Admin dashboard
        [RoleAuthorize("admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var announcements = await _announcementService.GetRecentAnnouncementsAsync(5);
            var announcementViewModels = announcements
                .Select(a => new AnnouncementViewModel(a))
                .ToList();

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _userManagementService.GetTotalUsersCountAsync(),
                UnverifiedUsers = await _userManagementService.GetUnverifiedUsersCountAsync(),
                TotalAnnouncements = await _announcementService.GetTotalAnnouncementsCountAsync(),
                // Using the updated method that returns EventViewModel
                UpcomingEvents = await _eventService.GetUpcomingEventsForDashboardAsync(5),
                RecentAnnouncements = announcementViewModels
            };
            return View("/Views/Pages/Admin/Dashboard/AdminDashboard.cshtml", viewModel);
        }

        // Staff dashboard
        [RoleAuthorize("staff")]
        public async Task<IActionResult> StaffDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var announcements = await _announcementService.GetAnnouncementsByUserIdAsync(userId);
            var announcementViewModels = announcements
                .Select(a => new AnnouncementViewModel(a))
                .ToList();

            var viewModel = new StaffDashboardViewModel
            {
                MyAnnouncements = announcementViewModels,
                // Using the updated method that returns EventViewModel
                UpcomingEvents = await _eventService.GetUpcomingEventsForDashboardAsync(5),
                // Using the updated method that returns FacilityReservationViewModel
                FacilityReservations = await _facilityService.GetRecentReservationsAsync(10)
            };
            return View("/Views/Pages/Staff/Dashboard/StaffDashboard.cshtml", viewModel);
        }

        // Homeowner dashboard
        [RoleAuthorize("homeowner")]
        public async Task<IActionResult> HomeownerDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int userIdInt))
            {
                return RedirectToAction("Index", "Home");
            }

            var announcements = await _announcementService.GetRecentAnnouncementsAsync(5);
            var announcementViewModels = announcements
                .Select(a => new AnnouncementViewModel(a))
                .ToList();

            var viewModel = new HomeownerDashboardViewModel
            {
                RecentAnnouncements = announcementViewModels,
                // Using the updated method that returns EventViewModel
                UpcomingEvents = await _eventService.GetUpcomingEventsForDashboardAsync(5),
                // Using the FacilityReservationService that already returns the correct view model type
                MyReservations = await _facilityReservationService.GetUserReservationsAsync(userIdInt)
            };
            return View("/Views/Pages/User/Dashboard/HomeownerDashboard.cshtml", viewModel);
        }
    }
}