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

        public AccountController(AppDbContext appDbContext)
        {
            _context = appDbContext;
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

        public IActionResult Registration(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                user account = new user();
                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.Email = model.Email;
                account.ContactNumber = model.ContactNumber;
                account.Username = model.Username;
                account.Password = model.Password;

                _context.Users.Add(account);
                _context.SaveChanges();

                try
                {
                    ModelState.Clear();
                    ViewBag.Message = $"{account.FirstName} {account.LastName} successfully registered. Please Login";

                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Please enter unique Email or Password.");
                    return View();
                }
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
                var user = _context.Users.Where(x => x.Username == model.UserNameorEmail || x.Email == model.UserNameorEmail && x.Password == model.Password).FirstOrDefault();
                if (user != null)
                {
                    // Success
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim("Name", user.Email),
                        new Claim(ClaimTypes.Role, "User"),
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

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("index");
        }

        [Authorize]

        public IActionResult SecurePage()
        {
            ViewBag.Name = HttpContext.User.Identity.Name;
            return View();
        }

    }
}