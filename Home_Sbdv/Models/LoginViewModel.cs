using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Home_Sbdv.Models
{
    public class LoginViewModel
    {
        // Username or Email field required for login
        [Required(ErrorMessage = "Username or Email is required.")]
        [MaxLength(50, ErrorMessage = "Max 50 characters allowed.")]
        [DisplayName("Username or Email")]
        public string UserNameorEmail { get; set; }
        // Password field with validation constraints
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Max 20 or min 5 characters allowed.")]
        [Column("PasswordHash")] // Maps to the database column "PasswordHash"
        public string Password { get; set; }
    }
}
