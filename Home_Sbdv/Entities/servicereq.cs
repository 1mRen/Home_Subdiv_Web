using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace Home_Sbdv.Entities
{
    public class ServiceRequest
    {
        [Key]
        [Column("request_id")]
        public int Req_Id { get; set; }

        [Required]
        [ForeignKey("User")]
        [Column("user_id")]
        public int Userid { get; set; }

        [Required]
        [Column("request_type")]
        public string Request_Type { get; set; } = String.Empty;

        [Required]
        [Column("description")]
        public string Description { get; set; } = String.Empty;

        [Required]
        [Column("status")]
        public string Status { get; set; } = String.Empty;

        [Column("submitted_at")]
        public DateTime? Submitted_at { get; set; }


    }
}
