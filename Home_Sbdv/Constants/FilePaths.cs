namespace Home_Sbdv.Constants
{
    public static class FilePaths
    {
        private static readonly string BaseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        
        public static readonly string FacilityImages = Path.Combine(BaseUploadPath, "Facilities");
        public static readonly string AnnouncementAttachments = Path.Combine(BaseUploadPath, "Announcements");
        public static readonly string ServiceRequestAttachments = Path.Combine(BaseUploadPath, "ServiceRequests");

        public static string GetRelativePath(string fullPath)
        {
            return fullPath.Replace(Directory.GetCurrentDirectory(), "").Replace("\\", "/");
        }
    }
} 