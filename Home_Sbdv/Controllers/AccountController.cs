using System.Security.Claims;
using Home_Sbdv.Data;
using Home_Sbdv.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Home_Sbdv.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        private string ExtractRole(string username)
        {
            username = username.ToLower(); // Convert to lowercase for case-insensitive check
            if (username.Contains("admin")) return "Admin";
            else if (username.Contains("staff")) return "Staff";
            return "HomeOwner"; // Default role
        }

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Users.ToList());
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                Users account = new Users
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Gender = model.Gender,    // New field (Ensure it's validated)
                    Email = model.Email,
                    ContactNumber = model.ContactNumber,
                    Username = model.Username,
                    Password = model.Password, // Plain text storage (NOT recommended for production)
                    Role = ExtractRole(model.Username),
                    Address = model.Address,  // New field
                    OwnershipStatus = model.OwnershipStatus // New field (Own/Rent)
                };

                _context.Users.Add(account);
                _context.SaveChanges();

                ModelState.Clear();
                ViewBag.Message = $"{account.FirstName} {account.LastName} successfully registered. Please Login";
                return View();
            }
            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
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
        public async Task<IActionResult> Create(Users newUser)
        {
            if (ModelState.IsValid)
            {
                // Append the role to the username (except for Homeowners)
                if (newUser.Role != "HomeOwner")
                {
                    newUser.Username = $"{newUser.Username}-{newUser.Role.ToLower()}";
                }
                _context.Add(newUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(newUser);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult SecurePage()
        {
            ViewBag.Name = HttpContext.User.Identity.Name;
            ViewBag.Role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            return View();
        }
    }
}
