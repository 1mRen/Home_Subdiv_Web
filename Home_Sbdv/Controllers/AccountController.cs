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

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
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
                if (await _context.Users.AnyAsync(x => x.Email == model.Email || x.Username == model.Username))
                {
                    ModelState.AddModelError("", "Email or Username already exists.");
                    return View();
                }

                users account = new users
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    ContactNumber = model.ContactNumber,
                    Username = model.Username,
                    Password = model.Password // Plain text storage (NOT recommended for production)
                };

                _context.Users.Add(account);
                await _context.SaveChangesAsync();

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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x =>
                        (x.Username == model.UserNameorEmail || x.Email == model.UserNameorEmail)
                        && x.Password == model.Password); // No hashing, plain text comparison

                if (user != null)
                {
                    // Success - Create authentication claims
                    var claims = new List<Claim>
{
                    new Claim(ClaimTypes.Email, user.Email),  // ✅ Correct claim for email
                    new Claim(ClaimTypes.Name, user.Username),  // ✅ Use Name claim for username
                    new Claim(ClaimTypes.Role, "User"),
};

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("SecurePage");

                }

                ModelState.AddModelError("", "Invalid Username/Email or Password.");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult SecurePage()
        {
            ViewBag.Name = User.Identity.Name;
            return View();
        }
    }
}
