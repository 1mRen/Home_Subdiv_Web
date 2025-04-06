using Home_Sbdv.Entities;
using Home_Sbdv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Home_Sbdv.Controllers
{
    [Authorize(Roles = "admin")]
    public class UserManagementController : Controller
    {
        private readonly IUserService _userService;

        public UserManagementController(IUserService userService)
        {
            _userService = userService;
        }

        // List all users
        public async Task<IActionResult> ListUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        // Process form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Users newUser)
        {
            // Log user info for debugging
            Console.WriteLine($"First Name: {newUser.FirstName}");
            Console.WriteLine($"Last Name: {newUser.LastName}");
            Console.WriteLine($"Email: {newUser.Email}");
            Console.WriteLine($"Contact Number: {newUser.ContactNumber}");
            Console.WriteLine($"Username: {newUser.Username}");
            Console.WriteLine($"Role: {newUser.Role}");
            Console.WriteLine($"Address: {newUser.Address}");
            Console.WriteLine($"Gender: {newUser.Gender}");
            Console.WriteLine($"Ownership Status: {newUser.OwnershipStatus}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid!");
                foreach (var error in ModelState)
                {
                    foreach (var errorMessage in error.Value.Errors)
                    {
                        Console.WriteLine($"Key: {error.Key}, Error: {errorMessage.ErrorMessage}");
                    }
                }
                return View(newUser);
            }

            if (await _userService.CreateUserAsync(newUser))
            {
                Console.WriteLine("User created successfully!");
                return RedirectToAction(nameof(ListUsers));
            }
            else
            {
                Console.WriteLine("Error creating user!");
                return View(newUser);
            }
        }

        // View user details
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // Show the edit form
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // Process edit form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Users updatedUser)
        {
            Console.WriteLine($"Received ID: {id}");
            Console.WriteLine($"Model ID: {updatedUser.Id}");
            Console.WriteLine($"First Name: {updatedUser.FirstName}");
            Console.WriteLine($"Last Name: {updatedUser.LastName}");
            Console.WriteLine($"Role: {updatedUser.Role}");
            Console.WriteLine($"Address: {updatedUser.Address}");

            ModelState.Remove("Password"); // Remove password validation
            ModelState.Remove("Username"); // Remove username validation

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid!");
                foreach (var error in ModelState)
                {
                    foreach (var errorMessage in error.Value.Errors)
                    {
                        Console.WriteLine($"Key: {error.Key}, Error: {errorMessage.ErrorMessage}");
                    }
                }
                return View(updatedUser);
            }

            if (await _userService.UpdateUserAsync(id, updatedUser))
            {
                Console.WriteLine("Changes saved successfully!");
                return RedirectToAction(nameof(ListUsers));
            }
            else
            {
                Console.WriteLine("Error updating user!");
                return View(updatedUser);
            }
        }

        // Show delete confirmation page
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // Process delete confirmation
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _userService.DeleteUserAsync(id);
            return RedirectToAction(nameof(ListUsers));
        }
    }
}