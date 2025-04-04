using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Home_Sbdv.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
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
