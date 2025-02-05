using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home_Subdiv_Web.Models
{
    // ViewModel for user registration
    public class RegistrationViewModel
    {
        // First name field with validation constraints
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "Max 50 characters allowed.")]
        public string FirstName { get; set; }

        // Last name field with validation constraints
        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50, ErrorMessage = "Max 50 characters allowed.")]
        public string LastName { get; set; }

        // Email field with validation and format checks
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [MaxLength(100, ErrorMessage = "Max 100 characters allowed.")]
        [RegularExpression(@"^[\w\.-]+@([\w-]+\.)+[a-zA-Z]{2,}$", ErrorMessage = "Please Enter a Valid Email.")]
        public string Email { get; set; }

        // Contact number field with validation constraints
        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(12, ErrorMessage = "Max 12 characters allowed.")]
        public string ContactNumber { get; set; }

        // Username field with validation constraints
        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(20, ErrorMessage = "Max 20 characters allowed.")]
        public string Username { get; set; }

        // Password field with validation constraints
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Max 20 or min 5 characters allowed.")]
        [Column("PasswordHash")] // Maps to the database column "PasswordHash"
        public string Password { get; set; }

        // Confirm Password field that must match Password
        [Compare("Password", ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        public string confirmPassword { get; set; }
    }
}
