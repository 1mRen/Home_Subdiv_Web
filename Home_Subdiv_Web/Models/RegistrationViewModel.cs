 using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home_Subdiv_Web.Models
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "Max 50 characters allowed.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50, ErrorMessage = "Max 50 characters allowed.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [MaxLength(100, ErrorMessage = "Max 100 characters allowed.")]
        //[EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"^[\w\.-]+@([\w-]+\.)+[a-zA-Z]{2,}$", ErrorMessage = "Please Enter a Valid Email.")]

        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(12, ErrorMessage = "Max 12 characters allowed.")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(20, ErrorMessage = "Max 20 characters allowed.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Max 20 or min 5 characters allowed.")]
        [Column("PasswordHash")]
        public string Password { get; set; }
            
        [Compare("Password", ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        public string confirmPassword { get; set; }
    }
}
