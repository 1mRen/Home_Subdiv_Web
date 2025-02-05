using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Home_Subdiv_Web.Controllers
{
    // Define unique constraints for Email, Username, and ContactNumber
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(ContactNumber), IsUnique = true)]
    public class user
    {
        // Primary Key
        [Key]
        public int Id { get; set; }

        // First Name field with required validation and max length constraint
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "Max 50 characters allowed.")]
        public string FirstName { get; set; }

        // Last Name field with required validation and max length constraint
        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50, ErrorMessage = "Max 50 characters allowed.")]
        public string LastName { get; set; }

        // Email field with required validation, email format, and max length constraint
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [MaxLength(100, ErrorMessage = "Max 100 characters allowed.")]
        public string Email { get; set; }

        // Contact Number field with required validation and max length constraint
        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(12, ErrorMessage = "Max 12 characters allowed.")]
        public string ContactNumber { get; set; }

        // Username field with required validation and max length constraint
        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(20, ErrorMessage = "Max 20 characters allowed.")]
        public string Username { get; set; }

        // Password field with required validation, data type, and max length constraint
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MaxLength(20, ErrorMessage = "Max 20 characters allowed.")]
        [Column("PasswordHash")] // Maps the column name to "PasswordHash" in the database
        public string Password { get; set; }

        // Role field for user role management
        public string Role { get; set; }

        // Timestamp Property - Auto-generated on creation (set to Asia/Shanghai timezone)
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Shanghai");

        // Timestamp Property - Auto-updated on modification
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? UpdatedAt { get; set; }
    }
}
