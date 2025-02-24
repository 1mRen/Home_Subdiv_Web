using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


namespace Home_Sbdv.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(ContactNumber), IsUnique = true)]

    public class Users
    {
        [Key]
        [Column("user_id")] // Ensure it matches the actual column name
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50)]
        [Column("firstname")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50)]
        [Column("lastname")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [MaxLength(100)]
        [Column("Email")] // Ensure correct case-sensitive mapping
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(25)] // Adjusted based on database column definition
        [Column("contact_number")]
        public string? ContactNumber { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(50)] // Adjusted based on database column definition
        [Column("username")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MaxLength(255)]
        [Column("password_hash")] // Ensure correct mapping
        public string? Password { get; set; }

        [Column("role")]
        public string? Role { get; set; }


        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(255)]
        [Column("address")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [MaxLength(10)]
        [Column("gender")]
        public string? Gender { get; set; }  // "Male", "Female", "Other"


        [Required(ErrorMessage = "Lot ownership status is required.")]
        [MaxLength(10)]
        [Column("ownership_status")]
        public string? OwnershipStatus { get; set; }  // "Own" or "Rent"

        // Correct column mappings for timestamps
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public virtual ICollection<Announcement>? Announcements { get; set; }
    }

}
