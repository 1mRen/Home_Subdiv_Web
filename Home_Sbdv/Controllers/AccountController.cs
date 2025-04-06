using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Configuration;

namespace Home_Sbdv.Controllers
{
    [RequireHttps] // Enforce HTTPS
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        public AccountController(AppDbContext context, ILogger<AccountController> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
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
                try
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == model.Email || u.Username == model.Username);

                    if (existingUser != null)
                    {
                        if (existingUser.Email == model.Email)
                            ModelState.AddModelError("Email", "This email is already registered.");
                        if (existingUser.Username == model.Username)
                            ModelState.AddModelError("Username", "This username is already taken.");

                        return View(model);
                    }

                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    var verificationToken = Guid.NewGuid().ToString();

                    var account = new Users
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Gender = model.Gender,
                        Email = model.Email,
                        ContactNumber = model.ContactNumber,
                        Username = model.Username,
                        Password = hashedPassword,
                        Role = "HomeOwner", // Always default
                        Address = model.Address,
                        OwnershipStatus = model.OwnershipStatus,
                        LoginAttempts = 0,
                        EmailVerified = false,
                        EmailVerificationToken = verificationToken,
                        EmailVerificationExpiry = DateTime.UtcNow.AddDays(3), // Token valid for 3 days
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(account);
                    await _context.SaveChangesAsync();

                    // Send verification email
                    await SendVerificationEmail(account.Email, verificationToken);

                    _logger.LogInformation("User {Username} successfully registered. Verification email sent.", model.Username);

                    ModelState.Clear();
                    TempData["Message"] = $"{account.FirstName} {account.LastName} successfully registered. Please check your email to verify your account.";
                    return RedirectToAction("VerificationSent", new { email = account.Email });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during user registration for {Email}", model.Email);
                    ModelState.AddModelError("", "An error occurred during registration. Please try again.");
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
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Invalid verification link.";
                return RedirectToAction("Login");
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() &&
                                       u.EmailVerificationToken == token);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Invalid verification link.";
                    return RedirectToAction("Login");
                }

                if (user.EmailVerified)
                {
                    TempData["Message"] = "Your email has already been verified. Please login.";
                    return RedirectToAction("Login");
                }

                if (user.EmailVerificationExpiry < DateTime.UtcNow)
                {
                    TempData["ErrorMessage"] = "Your verification link has expired. Please request a new one.";
                    return RedirectToAction("ResendVerification");
                }

                // Update user verification status
                user.EmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationExpiry = null;
                await _context.SaveChangesAsync();

                TempData["Message"] = "Your email has been successfully verified. You can now login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email {Email}", email);
                TempData["ErrorMessage"] = "An error occurred during verification. Please try again.";
                return RedirectToAction("Login");
            }
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

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                // Don't reveal if email exists (security)
                TempData["Message"] = "If your email exists in our system, a new verification link will be sent.";

                if (user != null && !user.EmailVerified)
                {
                    // Generate new verification token
                    var verificationToken = Guid.NewGuid().ToString();
                    user.EmailVerificationToken = verificationToken;
                    user.EmailVerificationExpiry = DateTime.UtcNow.AddDays(3);
                    await _context.SaveChangesAsync();

                    // Send new verification email
                    await SendVerificationEmail(user.Email, verificationToken);
                    _logger.LogInformation("New verification email sent to {Email}", model.Email);
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification for {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(model);
            }
        }

        private async Task SendVerificationEmail(string email, string verificationToken)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                string smtpHost = emailSettings["Host"];
                int smtpPort = int.Parse(emailSettings["Port"]);
                string fromEmail = emailSettings["FromEmail"];
                string password = emailSettings["Password"];
                bool enableSsl = bool.Parse(emailSettings["EnableSsl"]);
                string senderName = emailSettings["SenderName"];

                string baseUrl = $"{Request.Scheme}://{Request.Host}";
                string verificationUrl = $"{baseUrl}/Account/VerifyEmail?token={verificationToken}&email={WebUtility.UrlEncode(email)}";

                using (var client = new SmtpClient())
                {
                    client.Host = smtpHost;
                    client.Port = smtpPort;
                    client.EnableSsl = enableSsl;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(fromEmail, password);

                    var message = new MailMessage
                    {
                        From = new MailAddress(fromEmail, senderName),
                        Subject = "Verify Your Email Address",
                        Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
                                <h2 style='color: #2c3e50; text-align: center;'>Email Verification</h2>
                                <p>Hello,</p>
                                <p>Thank you for registering with our Home Subdivision System. To complete your registration, please verify your email address by clicking the button below:</p>
                                <div style='text-align: center; margin: 30px 0;'>
                                    <a href='{verificationUrl}' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold;'>Verify Email Address</a>
                                </div>
                                <p>If the button doesn't work, you can also copy and paste this link into your browser:</p>
                                <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 4px;'>{verificationUrl}</p>
                                <p>This link will expire in 3 days.</p>
                                <p>If you did not create an account, please ignore this email.</p>
                                <p>Thank you,<br>Home Subdivision Management Team</p>
                            </div>
                        </body>
                        </html>",
                        IsBodyHtml = true
                    };
                    message.To.Add(email);

                    await client.SendMailAsync(message);
                    _logger.LogInformation("Verification email sent to {Email}", email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}: {Message}", email, ex.Message);
                // We're swallowing the exception here to prevent the user from knowing 
                // about email sending failures, but it's logged for debugging
            }
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

            try
            {
                var user = await _context.Users
                    .Where(x => x.Username.ToLower() == model.UsernameOrEmail.ToLower()
                             || x.Email.ToLower() == model.UsernameOrEmail.ToLower())
                    .FirstOrDefaultAsync();

                if (user == null || string.IsNullOrEmpty(user.Password) || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    // Implement account lockout logic
                    if (user != null)
                    {
                        user.LoginAttempts = (user.LoginAttempts ?? 0) + 1;

                        // Lock account after 5 failed attempts
                        if (user.LoginAttempts >= 5)
                        {
                            user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                            _logger.LogWarning("Account {UserId} has been locked due to multiple failed login attempts", user.Id);
                        }

                        await _context.SaveChangesAsync();
                    }

                    // Generic error message for security
                    ModelState.AddModelError("", "Invalid login attempt.");
                    _logger.LogWarning("Failed login attempt for {UsernameOrEmail}", model.UsernameOrEmail);
                    return View(model);
                }

                // Check if account is locked out
                if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                {
                    ModelState.AddModelError("", "This account is temporarily locked. Please try again later.");
                    return View(model);
                }

                // Check if email is verified
                if (!user.EmailVerified)
                {
                    ModelState.AddModelError("", "Please verify your email before logging in.");
                    ViewBag.UnverifiedEmail = user.Email; // This can be used in the view to show a resend link
                    return View(model);
                }

                // Reset login attempts on successful login
                user.LoginAttempts = 0;
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} logged in successfully", user.Id);

                var role = user.Role?.ToLower() ?? "homeowner";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username ?? "UnknownUser"),
                    new Claim("Name", $"{user.FirstName ?? "Unknown"} {user.LastName ?? "User"}"),
                    new Claim(ClaimTypes.Role, role)
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

                return role switch
                {
                    "admin" => RedirectToAction("SecurePage", "Dashboard"),
                    "staff" => RedirectToAction("Dashboard", "Staff"),
                    _ => RedirectToAction("Index", "Home"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(model);
            }
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
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
                // Always show success message even if email not found (security)
                TempData["Message"] = "If your email exists in our system, you will receive password reset instructions.";
                if (user != null)
                {
                    // Generate reset token
                    var resetToken = Guid.NewGuid().ToString();
                    user.PasswordResetToken = resetToken;
                    user.PasswordResetExpiry = DateTime.UtcNow.AddHours(24);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Password reset requested for {Email}", model.Email);

                    // Send the password reset email directly
                    await SendPasswordResetEmail(user.Email, resetToken);
                }
                else
                {
                    _logger.LogWarning("Password reset requested for non-existent email {Email}", model.Email);
                }
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing password reset for {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(model);
            }
        }

        private async Task SendPasswordResetEmail(string email, string resetToken)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                string smtpHost = emailSettings["Host"];
                int smtpPort = int.Parse(emailSettings["Port"]);
                string fromEmail = emailSettings["FromEmail"];
                string password = emailSettings["Password"];
                bool enableSsl = bool.Parse(emailSettings["EnableSsl"]);
                string senderName = emailSettings["SenderName"];

                // Create the base URL for your application
                string baseUrl = $"{Request.Scheme}://{Request.Host}";
                string resetUrl = $"{baseUrl}/Account/ResetPassword?token={resetToken}&email={WebUtility.UrlEncode(email)}";

                using (var client = new SmtpClient())
                {
                    client.Host = smtpHost;
                    client.Port = smtpPort;
                    client.EnableSsl = enableSsl;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(fromEmail, password);

                    var message = new MailMessage
                    {
                        From = new MailAddress(fromEmail, senderName),
                        Subject = "Password Reset Instructions",
                        Body = $@"
                    <html>
                    <body>
                        <h2>Password Reset</h2>
                        <p>Hello,</p>
                        <p>You recently requested to reset your password. Please click the link below to reset your password:</p>
                        <p><a href=""{resetUrl}"">Reset Password</a></p>
                        <p>This link will expire in 24 hours.</p>
                        <p>If you did not request a password reset, please ignore this email.</p>
                        <p>Thank you,<br>Your Application Team</p>
                    </body>
                    </html>",
                        IsBodyHtml = true
                    };
                    message.To.Add(email);

                    await client.SendMailAsync(message);
                    _logger.LogInformation("Password reset email sent to {Email}", email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}: {Message}", email, ex.Message);
            }
        }
    }
}