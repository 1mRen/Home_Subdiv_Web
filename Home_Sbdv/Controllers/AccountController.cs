﻿using System.Security.Claims;
using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                // Check if the email or username already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email || u.Username == model.Username);

                if (existingUser != null)
                {
                    if (existingUser.Email == model.Email)
                    {
                        ModelState.AddModelError("Email", "This email is already registered.");
                        ViewBag.Message = "This email is already registered. Please use a different email.";
                    }

                    if (existingUser.Username == model.Username)
                    {
                        ModelState.AddModelError("Username", "This username is already taken.");
                        ViewBag.Message = "This username is already taken. Please choose a different username.";
                    }

                    return View(model); // Return the view with validation messages
                }

                // Hash the password before storing it
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                // Create new user account with hashed password
                Home_Sbdv.Entities.Users account = new Home_Sbdv.Entities.Users
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    Email = model.Email,
                    ContactNumber = model.ContactNumber,
                    Username = model.Username,
                    Password = hashedPassword, // Store the hashed password
                    Role = ExtractRole(model.Username),
                    Address = model.Address,
                    OwnershipStatus = model.OwnershipStatus
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
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users
    .FirstOrDefault(x => x.Username == model.UserNameorEmail || x.Email == model.UserNameorEmail);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    var claims = new List<Claim>
{
                    new Claim(ClaimTypes.Name, user.Username ?? "UnknownUser"),
                    new Claim("Name", (user.FirstName ?? "Unknown") + " " + (user.LastName ?? "User")),
                    new Claim(ClaimTypes.Role, user.Role ?? "HomeOwner") // Provide a default role if null
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
                // Ensure Role is not null before calling .ToLower()
                if (!string.IsNullOrEmpty(newUser.Role) && newUser.Role != "HomeOwner")
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
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult SecurePage()
        {
            ViewBag.Name = HttpContext.User.Identity?.Name ?? "Guest";
            ViewBag.Role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "HomeOwner";
            return View();
        }
    }   
}
