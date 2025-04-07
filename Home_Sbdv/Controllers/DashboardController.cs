using System.Security.Claims;
using Home_Sbdv.Data;
using Home_Sbdv.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Home_Sbdv.Controllers
{
    public class DashboardController : Controller
    {

        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> SecurePage()
        {
            ViewBag.Name = HttpContext.User.Identity?.Name ?? "Guest";
            ViewBag.Role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "HomeOwner";

            var users = await _context.Users.ToListAsync();

            // Create a UserListViewModel
            var viewModel = new UserListViewModel
            {
                Users = users.Select(u => UserViewModel.FromEntity(u)).ToList(),
                Pagination = new PaginationInfo
                {
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalItems = users.Count
                },
                SearchTerm = "",
                SortColumn = "LastName",
                SortOrder = "asc",
                RoleFilter = "all"
            };

            return View(viewModel);
        }

    }
}
