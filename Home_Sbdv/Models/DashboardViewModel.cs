using System.Collections.Generic;
using Home_Sbdv.Models;

namespace Home_Sbdv.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int UnverifiedUsers { get; set; }
        public int TotalAnnouncements { get; set; }
        // Changed from List<Event> to List<EventViewModel>
        public List<EventViewModel> UpcomingEvents { get; set; } = new List<EventViewModel>();
        public List<AnnouncementViewModel> RecentAnnouncements { get; set; } = new List<AnnouncementViewModel>();
    }

    public class StaffDashboardViewModel
    {
        public List<AnnouncementViewModel> MyAnnouncements { get; set; } = new List<AnnouncementViewModel>();
        public List<AnnouncementViewModel> RecentAnnouncements { get; set; } = new List<AnnouncementViewModel>();
        // Changed from List<Event> to List<EventViewModel>
        public List<EventViewModel> UpcomingEvents { get; set; } = new List<EventViewModel>();
        public List<FacilityReservationViewModel> FacilityReservations { get; set; } = new List<FacilityReservationViewModel>();
    }

    public class HomeownerDashboardViewModel
    {
        public List<AnnouncementViewModel> RecentAnnouncements { get; set; } = new List<AnnouncementViewModel>();
        // Changed from List<Event> to List<EventViewModel>
        public List<EventViewModel> UpcomingEvents { get; set; } = new List<EventViewModel>();
        public List<FacilityReservationViewModel> MyReservations { get; set; } = new List<FacilityReservationViewModel>();
    }
}