namespace Home_Sbdv.Constants
{
    public static class FilePaths
    {
        private static readonly string BaseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        public static readonly string FacilityImages = Path.Combine(BaseUploadPath, "facility-images");
        public static readonly string AnnouncementAttachments = Path.Combine(BaseUploadPath, "announcement-attachments");
        public static readonly string ServiceRequestAttachments = Path.Combine(BaseUploadPath, "service-request-attachments");
        public static readonly string FeedbackAttachments = Path.Combine(BaseUploadPath, "feedback-attachments");

        public static string GetRelativePath(string fullPath)
        {
            return "~/" + Path.GetRelativePath(Directory.GetCurrentDirectory(), fullPath).Replace("\\", "/");
        }
    }
} 