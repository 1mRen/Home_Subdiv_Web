using System;
using System.Threading.Tasks;
using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using Home_Sbdv.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.IO;

namespace Home_Sbdv.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AccountService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;

        public AccountService(
            AppDbContext context,
            ILogger<AccountService> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        public async Task<ServiceResult> RegisterUserAsync(RegistrationViewModel model)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email || u.Username == model.Username);
                if (existingUser != null)
                {
                    if (existingUser.Email == model.Email)
                        return ServiceResult.FailureResult("This email is already registered.", "DuplicateEmail");
                    if (existingUser.Username == model.Username)
                        return ServiceResult.FailureResult("This username is already taken.", "DuplicateUsername");
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password, 12); // Explicitly set work factor
                var verificationToken = GenerateSecureToken();

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
                await SendVerificationEmailAsync(account.Email, verificationToken);
                _logger.LogInformation("User {Username} successfully registered. Verification email sent.", model.Username);
                return ServiceResult.SuccessResult($"{account.FirstName} {account.LastName} successfully registered. Please check your email to verify your account.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for {Email}", model.Email);
                return ServiceResult.FailureResult("An error occurred during registration. Please try again.");
            }
        }

        public async Task<ServiceResult> VerifyEmailAsync(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return ServiceResult.FailureResult("Invalid verification link.");
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() &&
                                       u.EmailVerificationToken == token);

                if (user == null)
                {
                    return ServiceResult.FailureResult("Invalid verification link.");
                }

                if (user.EmailVerified)
                {
                    return ServiceResult.SuccessResult("Your email has already been verified. Please login.");
                }

                if (user.EmailVerificationExpiry < DateTime.UtcNow)
                {
                    return ServiceResult.FailureResult("Your verification link has expired. Please request a new one.");
                }

                // Update user verification status
                user.EmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationExpiry = null;
                await _context.SaveChangesAsync();

                return ServiceResult.SuccessResult("Your email has been successfully verified. You can now login.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email {Email}", email);
                return ServiceResult.FailureResult("An error occurred during verification. Please try again.");
            }
        }

        public async Task<ServiceResult> ResendVerificationEmailAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                // Don't reveal if email exists (security)
                if (user != null && !user.EmailVerified)
                {
                    // Generate new verification token
                    var verificationToken = GenerateSecureToken();
                    user.EmailVerificationToken = verificationToken;
                    user.EmailVerificationExpiry = DateTime.UtcNow.AddDays(3);
                    await _context.SaveChangesAsync();

                    // Send new verification email
                    await SendVerificationEmailAsync(user.Email, verificationToken);
                    _logger.LogInformation("New verification email sent to {Email}", email);
                }

                return ServiceResult.SuccessResult("If your email exists in our system, a new verification link will be sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification for {Email}", email);
                return ServiceResult.FailureResult("An error occurred. Please try again.");
            }
        }

        public async Task<ServiceResult<string>> AuthenticateUserAsync(LoginViewModel model, bool rememberMe)
        {
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
                    _logger.LogWarning("Failed login attempt for {UsernameOrEmail}", model.UsernameOrEmail);
                    return ServiceResult<string>.FailureResult("Invalid login attempt.");
                }

                // Check if account is locked out
                if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                {
                    return ServiceResult<string>.FailureResult("This account is temporarily locked. Please try again later.");
                }

                // Check if email is verified
                if (!user.EmailVerified)
                {
                    return ServiceResult<string>.FailureResult("Please verify your email before logging in.", "UnverifiedEmail");
                }

                // Reset login attempts on successful login
                user.LoginAttempts = 0;
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} logged in successfully", user.Id);

                // Return user role for redirect determination in controller
                return ServiceResult<string>.SuccessResult(user.Role?.ToLower() ?? "homeowner");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt");
                return ServiceResult<string>.FailureResult("An error occurred during login. Please try again.");
            }
        }

        public async Task<ServiceResult> SendPasswordResetLinkAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                // Always show success message even if email not found (security)
                if (user != null)
                {
                    // Generate reset token
                    var resetToken = GenerateSecureToken();
                    user.PasswordResetToken = resetToken;
                    user.PasswordResetExpiry = DateTime.UtcNow.AddHours(24);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Password reset requested for {Email}", email);
                    // Send the password reset email
                    await SendPasswordResetEmailAsync(user.Email, resetToken);
                }
                else
                {
                    _logger.LogWarning("Password reset requested for non-existent email {Email}", email);
                }

                return ServiceResult.SuccessResult("If your email exists in our system, you will receive password reset instructions.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing password reset for {Email}", email);
                return ServiceResult.FailureResult("An error occurred. Please try again.");
            }
        }

        public async Task<ServiceResult> ValidatePasswordResetTokenAsync(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return ServiceResult.FailureResult("Token and email are required.");
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                {
                    return ServiceResult.FailureResult("Invalid request.");
                }

                // Validate token existence and expiration
                if (user.PasswordResetToken != token ||
                    !user.PasswordResetExpiry.HasValue ||
                    user.PasswordResetExpiry.Value < DateTime.UtcNow)
                {
                    return ServiceResult.FailureResult("Invalid or expired token.");
                }

                return ServiceResult.SuccessResult("Token validated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating reset token for {Email}", email);
                return ServiceResult.FailureResult("An error occurred. Please try again.");
            }
        }

        public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            if (model == null)
            {
                return ServiceResult.FailureResult("Model cannot be null.");
            }

            if (string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.Email))
            {
                return ServiceResult.FailureResult("Token and email are required.");
            }

            if (string.IsNullOrEmpty(model.Password) || model.Password != model.ConfirmPassword)
            {
                return ServiceResult.FailureResult("Passwords must match.");
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (user == null)
                {
                    // For security reasons, we return a generic message
                    return ServiceResult.FailureResult("Invalid request.");
                }

                // Validate token
                if (user.PasswordResetToken != model.Token ||
                    !user.PasswordResetExpiry.HasValue ||
                    user.PasswordResetExpiry.Value < DateTime.UtcNow)
                {
                    return ServiceResult.FailureResult("Invalid or expired token.");
                }

                // Update password
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password, 12);

                // Clear reset token
                user.PasswordResetToken = null;
                user.PasswordResetExpiry = null;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Password reset successful for user: {Email}", user.Email);
                return ServiceResult.SuccessResult("Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", model.Email);
                return ServiceResult.FailureResult("An error occurred. Please try again.");
            }
        }

        private async Task SendVerificationEmailAsync(string email, string verificationToken)
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

                var request = _httpContextAccessor.HttpContext.Request;
                string baseUrl = $"{request.Scheme}://{request.Host}";
                string verificationUrl = $"{baseUrl}/Account/VerifyEmail?token={WebUtility.UrlEncode(verificationToken)}&email={WebUtility.UrlEncode(email)}";

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

        private async Task SendPasswordResetEmailAsync(string email, string resetToken)
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

                var request = _httpContextAccessor.HttpContext.Request;
                string baseUrl = $"{request.Scheme}://{request.Host}";
                string resetUrl = $"{baseUrl}/Account/ResetPassword?token={WebUtility.UrlEncode(resetToken)}&email={WebUtility.UrlEncode(email)}";

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
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
                                <h2 style='color: #2c3e50; text-align: center;'>Password Reset</h2>
                                <p>Hello,</p>
                                <p>You recently requested to reset your password. Please click the button below to reset your password:</p>
                                <div style='text-align: center; margin: 30px 0;'>
                                    <a href='{resetUrl}' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold;'>Reset Password</a>
                                </div>
                                <p>If the button doesn't work, you can also copy and paste this link into your browser:</p>
                                <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 4px;'>{resetUrl}</p>
                                <p>This link will expire in 24 hours.</p>
                                <p>If you did not request a password reset, please ignore this email.</p>
                                <p>Thank you,<br>Home Subdivision Management Team</p>
                            </div>
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

        // Generate cryptographically secure token
        private string GenerateSecureToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var tokenData = new byte[32]; // 256 bits
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");
            }
        }

        public async Task<AccountViewModel> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;
            return new AccountViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ContactNumber = user.ContactNumber,
                Address = user.Address,
                Gender = user.Gender,
                OwnershipStatus = user.OwnershipStatus,
                ProfilePictureUrl = user.ProfilePictureUrl
            };
        }

        public async Task<bool> UpdateProfileAsync(int userId, AccountViewModel model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.ContactNumber = model.ContactNumber;
            user.Address = model.Address;
            user.Gender = model.Gender;
            user.OwnershipStatus = model.OwnershipStatus;

            // Handle profile picture upload
            if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "ProfilePictures");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePictureFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePictureFile.CopyToAsync(stream);
                }
                user.ProfilePictureUrl = "/Uploads/ProfilePictures/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}