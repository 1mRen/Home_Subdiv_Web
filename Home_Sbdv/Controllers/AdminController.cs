using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Home_Sbdv.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // List all users
        public async Task<IActionResult> ListUsers()
        {
            var users = await _context.Users.ToListAsync();
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

            try
            {
                // Hash the password (ensure you have a hashing method)
                newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                Console.WriteLine("User created successfully!");

                return RedirectToAction(nameof(ListUsers));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return View(newUser);
            }
        }

        // View user details
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // Show the edit form
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
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

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    Console.WriteLine("User not found!");
                    return NotFound();
                }

                // Update only necessary fields
                user.FirstName = updatedUser.FirstName;
                user.LastName = updatedUser.LastName;
                user.Role = updatedUser.Role;
                user.Address = updatedUser.Address;
                user.Gender = updatedUser.Gender;
                user.OwnershipStatus = updatedUser.OwnershipStatus;
                user.ContactNumber = updatedUser.ContactNumber;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                Console.WriteLine("Changes saved successfully!");

                return RedirectToAction(nameof(ListUsers));
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("Concurrency error occurred!");
                throw;
            }
        }




        // Show delete confirmation page
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
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
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ListUsers));
        }
    }
}
