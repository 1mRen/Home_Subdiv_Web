using System.Diagnostics;
using Home_Subdiv_Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Home_Subdiv_Web.Controllers
{
    // Home Controller managing the main pages of the application
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // Logger instance for logging information

        // Constructor to inject logger dependency
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Action method for the home page
        public IActionResult Index()
        {
            return View();
        }

        // Action method for the privacy policy page
        public IActionResult Privacy()
        {
            return View();
        }

        // Error handling action with response caching disabled
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
