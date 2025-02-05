using System.Data;
using System.Security.Claims;
using Home_Subdiv_Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Home_Subdiv_Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        // Method to determine the user role based on their username
        private string ExtractRole(string username)
        {
            username = username.ToLower(); // Convert to lowercase for case-insensitive check
            if (username.Contains("admin")) return "Admin";
            else if (username.Contains("staff")) return "Staff";

            return "Home Owner"; // Default role
        }

        // Constructor to inject database context
        public AccountController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        // Display the list of users
        public IActionResult Index()
        {
            return View(_context.Users.ToList());
        }

        // Show the registration form
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        // Handle user registration
        public IActionResult Registration(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    user account = new user()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        ContactNumber = model.ContactNumber,
                        Username = model.Username,
                        Password = model.Password,  // Consider hashing this for security
                        Role = ExtractRole(model.Username) // Extract Role from Username
                    };

                    _context.Users.Add(account);
                    _context.SaveChanges();

                    ModelState.Clear();
                    ViewBag.Message = $"{account.FirstName} {account.LastName} successfully registered. Please Login";
                    return View();
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Error: Please enter a unique Email or Username.");
                    return View(model);
                }
            }
            return View(model);
        }

        // Show the login form
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        // Handle user login
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users
                    .Where(x => (x.Username == model.UserNameorEmail || x.Email == model.UserNameorEmail)
                                && x.Password == model.Password)
                    .FirstOrDefault();

                if (user != null)
                {
                    // Success - Assign Role
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim("Name", user.FirstName + " " + user.LastName),
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("SecurePage");
                }
                else
                {
                    ModelState.AddModelError("", "Username/Email or Password is incorrect");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Handle user creation with role-based username formatting
        public async Task<IActionResult> Create(user newUser)
        {
            if (ModelState.IsValid)
            {
                // Append the role to the username (except for Homeowners)
                if (newUser.Role != "Homeowner")
                {
                    newUser.Username = $"{newUser.Username}-{newUser.Role.ToLower()}";
                }

                _context.Add(newUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(newUser);
        }

        // Handle user logout
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("index");
        }

        // Admin dashboard - Accessible only to Admin users
        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        // Staff dashboard - Accessible only to Staff users
        [Authorize(Roles = "Staff")]
        public IActionResult StaffDashboard()
        {
            return View();
        }

        [Authorize]
        // Secure page - Accessible to all authorized users
        public IActionResult SecurePage()
        {
            ViewBag.Name = HttpContext.User.Identity.Name;
            ViewBag.Role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            return View();
        }
    }
}
