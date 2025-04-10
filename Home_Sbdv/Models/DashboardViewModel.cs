// DashboardViewModels.cs
using System.Collections.Generic;
using Home_Sbdv.Entities;

namespace Home_Sbdv.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int UnverifiedUsers { get; set; }
        public int TotalAnnouncements { get; set; }
        public List<Event> UpcomingEvents { get; set; }
        public List<AnnouncementViewModel> RecentAnnouncements { get; set; }
    }

    public class StaffDashboardViewModel
    {
        public List<AnnouncementViewModel> MyAnnouncements { get; set; }
        public List<Event> UpcomingEvents { get; set; }
        public List<FacilityReservation> FacilityReservations { get; set; }
    }

    public class HomeownerDashboardViewModel
    {
        public List<AnnouncementViewModel> RecentAnnouncements { get; set; }
        public List<Event> UpcomingEvents { get; set; }
        public List<FacilityReservationViewModel> MyReservations { get; set; }
    }
}