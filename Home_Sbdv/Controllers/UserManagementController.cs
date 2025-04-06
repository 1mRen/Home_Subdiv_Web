using Home_Sbdv.Entities;
using Home_Sbdv.Services;
using Home_Sbdv.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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

        // List all users with pagination, sorting, and filtering
        public async Task<IActionResult> ListUsers(string searchTerm = "", string sortColumn = "LastName",
            string sortOrder = "asc", string roleFilter = "all", int page = 1, int pageSize = 10)
        {
            // Get all users
            var users = await _userService.GetAllUsersAsync();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                users = users.Where(u =>
                    u.FirstName?.ToLower().Contains(searchTerm) == true ||
                    u.LastName?.ToLower().Contains(searchTerm) == true ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.Username?.ToLower().Contains(searchTerm) == true
                ).ToList();
            }

            // Apply role filter if not "all"
            if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter.ToLower() != "all")
            {
                users = users.Where(u => u.Role?.ToLower() == roleFilter.ToLower()).ToList();
            }

            // Apply sorting
            users = sortOrder.ToLower() == "asc"
                ? SortUsersAscending(users, sortColumn)
                : SortUsersDescending(users, sortColumn);

            // Apply pagination
            var totalItems = users.Count;
            var pagedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Create view model
            var viewModel = new UserListViewModel
            {
                Users = pagedUsers.Select(u => UserViewModel.FromEntity(u)).ToList(),
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                },
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                RoleFilter = roleFilter
            };

            return View(viewModel);
        }

        private List<Users> SortUsersAscending(List<Users> users, string sortColumn)
        {
            return sortColumn.ToLower() switch
            {
                "firstname" => users.OrderBy(u => u.FirstName).ToList(),
                "lastname" => users.OrderBy(u => u.LastName).ToList(),
                "email" => users.OrderBy(u => u.Email).ToList(),
                "username" => users.OrderBy(u => u.Username).ToList(),
                "role" => users.OrderBy(u => u.Role).ToList(),
                "createdat" => users.OrderBy(u => u.CreatedAt).ToList(),
                _ => users.OrderBy(u => u.LastName).ToList() // Default sort
            };
        }

        private List<Users> SortUsersDescending(List<Users> users, string sortColumn)
        {
            return sortColumn.ToLower() switch
            {
                "firstname" => users.OrderByDescending(u => u.FirstName).ToList(),
                "lastname" => users.OrderByDescending(u => u.LastName).ToList(),
                "email" => users.OrderByDescending(u => u.Email).ToList(),
                "username" => users.OrderByDescending(u => u.Username).ToList(),
                "role" => users.OrderByDescending(u => u.Role).ToList(),
                "createdat" => users.OrderByDescending(u => u.CreatedAt).ToList(),
                _ => users.OrderByDescending(u => u.LastName).ToList() // Default sort
            };
        }

        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        // Process form submission with view model
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel viewModel)
        {
            // Log user info for debugging
            Console.WriteLine($"First Name: {viewModel.FirstName}");
            Console.WriteLine($"Last Name: {viewModel.LastName}");
            Console.WriteLine($"Email: {viewModel.Email}");

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
                return View(viewModel);
            }

            // Convert to entity
            var userEntity = viewModel.ToEntity();

            if (await _userService.CreateUserAsync(userEntity))
            {
                Console.WriteLine("User created successfully!");
                return RedirectToAction(nameof(ListUsers));
            }
            else
            {
                Console.WriteLine("Error creating user!");
                ModelState.AddModelError("", "Could not create user. Email or username may already be in use.");
                return View(viewModel);
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

            var viewModel = UserDetailsViewModel.FromEntity(user);
            return View(viewModel);
        }

        // Show the edit form
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = EditUserViewModel.FromEntity(user);
            return View(viewModel);
        }

        // Process edit form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel viewModel)
        {
            Console.WriteLine($"Received ID: {id}");
            Console.WriteLine($"Model ID: {viewModel.Id}");
            Console.WriteLine($"First Name: {viewModel.FirstName}");
            Console.WriteLine($"Last Name: {viewModel.LastName}");

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
                return View(viewModel);
            }

            // Get the existing user
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Update only the fields from the view model
            viewModel.UpdateEntity(user);

            if (await _userService.UpdateUserAsync(id, user))
            {
                Console.WriteLine("Changes saved successfully!");
                return RedirectToAction(nameof(ListUsers));
            }
            else
            {
                Console.WriteLine("Error updating user!");
                ModelState.AddModelError("", "Could not update user.");
                return View(viewModel);
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

            var viewModel = UserDetailsViewModel.FromEntity(user);
            return View(viewModel);
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