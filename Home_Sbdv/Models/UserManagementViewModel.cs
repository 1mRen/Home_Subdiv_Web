using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Home_Sbdv.Entities;

namespace Home_Sbdv.Models
{
    // Main view model for the user listing page
    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
        public string SearchTerm { get; set; } = string.Empty;
        public string SortColumn { get; set; } = "LastName";
        public string SortOrder { get; set; } = "asc";
        public string RoleFilter { get; set; } = "all";
    }

    // Pagination information
    public class PaginationInfo
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / PageSize);
    }

    // Individual user view model for list display
    public class UserViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string OwnershipStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool EmailVerified { get; set; }

        // Helper method to convert from entity to view model
        public static UserViewModel FromEntity(Users user)
        {
            return new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                ContactNumber = user.ContactNumber ?? string.Empty,
                Username = user.Username ?? string.Empty,
                Role = user.Role ?? string.Empty,
                Address = user.Address ?? string.Empty,
                Gender = user.Gender ?? string.Empty,
                OwnershipStatus = user.OwnershipStatus ?? string.Empty,
                CreatedAt = user.CreatedAt,
                EmailVerified = user.EmailVerified
            };
        }
    }

    // Create user view model with data annotations for validation
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Please enter a valid phone number.")]
        [MaxLength(25)]
        public string ContactNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [RegularExpression(@"^[a-zA-Z0-9_-]{5,50}$", ErrorMessage = "Username must be 5-50 characters and can only contain letters, numbers, underscores and hyphens.")]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = "resident";  // Default role

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ownership status is required.")]
        [Display(Name = "Ownership Status")]
        public string OwnershipStatus { get; set; } = string.Empty;

        // Convert view model to entity
        public Users ToEntity()
        {
            return new Users
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                ContactNumber = ContactNumber,
                Username = Username,
                Password = Password,  // Will be hashed in the service
                Role = Role,
                Address = Address,
                Gender = Gender,
                OwnershipStatus = OwnershipStatus,
                EmailVerified = false  // Default for new users
            };
        }
    }

    // Edit user view model
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;  // Read-only in the view

        [Required(ErrorMessage = "Phone number is required.")]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Please enter a valid phone number.")]
        [MaxLength(25)]
        public string ContactNumber { get; set; } = string.Empty;

        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;  // Read-only in the view

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ownership status is required.")]
        [Display(Name = "Ownership Status")]
        public string OwnershipStatus { get; set; } = string.Empty;

        public bool EmailVerified { get; set; }

        // Load from entity
        public static EditUserViewModel FromEntity(Users user)
        {
            return new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email,
                ContactNumber = user.ContactNumber ?? string.Empty,
                Username = user.Username ?? string.Empty,
                Role = user.Role ?? string.Empty,
                Address = user.Address ?? string.Empty,
                Gender = user.Gender ?? string.Empty,
                OwnershipStatus = user.OwnershipStatus ?? string.Empty,
                EmailVerified = user.EmailVerified
            };
        }

        // Update an existing entity (for partial updates)
        public void UpdateEntity(Users user)
        {
            user.FirstName = FirstName;
            user.LastName = LastName;
            user.ContactNumber = ContactNumber;
            user.Role = Role;
            user.Address = Address;
            user.Gender = Gender;
            user.OwnershipStatus = OwnershipStatus;
            // Email and Username are not updated
        }
    }

    // User details view model
    public class UserDetailsViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string OwnershipStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool EmailVerified { get; set; }
        public int LoginAttempts { get; set; }
        public bool IsLocked => LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;
        public DateTime? LockoutEnd { get; set; }

        // Helper method to convert from entity
        public static UserDetailsViewModel FromEntity(Users user)
        {
            return new UserDetailsViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email,
                ContactNumber = user.ContactNumber ?? string.Empty,
                Username = user.Username ?? string.Empty,
                Role = user.Role ?? string.Empty,
                Address = user.Address ?? string.Empty,
                Gender = user.Gender ?? string.Empty,
                OwnershipStatus = user.OwnershipStatus ?? string.Empty,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                EmailVerified = user.EmailVerified,
                LoginAttempts = user.LoginAttempts ?? 0,
                LockoutEnd = user.LockoutEnd
            };
        }
    }
}