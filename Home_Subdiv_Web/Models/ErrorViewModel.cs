namespace Home_Subdiv_Web.Models
{
    // ViewModel for handling error information
    public class ErrorViewModel
    {
        // Stores the request ID for tracking errors
        public string? RequestId { get; set; }

        // Returns true if RequestId is not null or empty, indicating an error occurred
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
