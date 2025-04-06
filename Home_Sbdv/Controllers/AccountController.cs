using System.Threading.Tasks;
using System.Security.Claims;
using Home_Sbdv.Models;
using Home_Sbdv.Services;
using Home_Sbdv.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Home_Sbdv.Controllers
{
    [RequireHttps] // Enforce HTTPS
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;
        private readonly IUserRepository _userRepository;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger, IUserRepository userRepository)
        {
            _accountService = accountService;
            _logger = logger;
            _userRepository = userRepository;
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registration(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.RegisterUserAsync(model);

                if (result.Success)
                {
                    ModelState.Clear();
                    TempData["Message"] = result.Message;
                    return RedirectToAction("VerificationSent", new { email = model.Email });
                }
                else
                {
                    // Get more specific error details from the result
                    if (result.ErrorCode == "DuplicateEmail")
                        ModelState.AddModelError("Email", result.Message);
                    else if (result.ErrorCode == "DuplicateUsername")
                        ModelState.AddModelError("Username", result.Message);
                    else
                        ModelState.AddModelError("", result.Message);
                }
            }

            return View(model);
        }

        public IActionResult VerificationSent(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token, string email)
        {
            var result = await _accountService.VerifyEmailAsync(token, email);

            if (result.Success)
                TempData["Message"] = result.Message;
            else
                TempData["ErrorMessage"] = result.Message;

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResendVerification()
        {
            return View(new ResendVerificationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerification(ResendVerificationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.ResendVerificationEmailAsync(model.Email);
            TempData["Message"] = result.Message;
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            ViewBag.Message = TempData["Message"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, bool rememberMe = false)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.AuthenticateUserAsync(model, rememberMe);

            if (!result.Success)
            {
                // Special handling for unverified emails
                if (result.ErrorCode == "UnverifiedEmail")
                {
                    ViewBag.UnverifiedEmail = model.UsernameOrEmail;
                }

                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            // At this point, authentication was successful
            // Get the user (we need to do this again to get claims information)
            var user = await _userRepository.GetUserByUsernameOrEmailAsync(model.UsernameOrEmail);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? "UnknownUser"),
                new Claim("Name", $"{user.FirstName ?? "Unknown"} {user.LastName ?? "User"}"),
                new Claim(ClaimTypes.Role, result.Data) // This is the role from the service result
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Configure authentication properties with timeout and remember-me
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddMinutes(30),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect based on role
            return result.Data switch
            {
                "admin" => RedirectToAction("SecurePage", "Dashboard"),
                "staff" => RedirectToAction("Dashboard", "Staff"),
                _ => RedirectToAction("Index", "Home"),
            };
        }

        [Authorize] // Ensure user is logged in
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User {UserId} logged out", userId);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.SendPasswordResetLinkAsync(model.Email);
            TempData["Message"] = result.Message;
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            var result = await _accountService.ValidatePasswordResetTokenAsync(token, email);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.ResetPasswordAsync(model);

            if (result.Success)
            {
                TempData["Message"] = result.Message;
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }
    }
}