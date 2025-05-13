using System;
using System.Collections.Generic;

namespace Home_Sbdv.Models
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Link { get; set; }
    }

    public class NotificationListViewModel
    {
        public List<NotificationViewModel> Notifications { get; set; }
        public int UnreadCount { get; set; }
    }
} 