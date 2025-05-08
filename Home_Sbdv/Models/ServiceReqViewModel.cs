using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home_Sbdv.Models
{
    public class ServiceReqViewModel
    {
        public int RequestId { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Request type is required.")]
        public string Request_Type { get; set; }

        [Required]
        public string Description { get; set; }

    }
}
