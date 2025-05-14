using Home_Sbdv.Models;
using Home_Sbdv.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Home_Sbdv.Controllers
{
    [Authorize]
    public class VisitorPassController : Controller
    {
        private readonly IVisitorPassService _visitorPassService;
        public VisitorPassController(IVisitorPassService visitorPassService)
        {
            _visitorPassService = visitorPassService;
        }

        // Admin: View all requests
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminList(string search, string status, DateTime? date)
        {
            var requests = await _visitorPassService.GetAllRequestsAsync();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                requests = requests.Where(r =>
                    r.VisitorName.ToLower().Contains(search) ||
                    r.VisitorContact.ToLower().Contains(search) ||
                    r.Purpose.ToLower().Contains(search)
                ).ToList();
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                requests = requests.Where(r => r.Status == status).ToList();
            }

            // Apply date filter
            if (date.HasValue)
            {
                requests = requests.Where(r => r.VisitDate.Date == date.Value.Date).ToList();
            }

            // Sort by most recent first
            requests = requests.OrderByDescending(r => r.RequestedAt).ToList();

            return View("/Views/Pages/admin/VisitorPass/AdminList.cshtml", requests);
        }

        // Staff: View all approved for today
        [Authorize(Roles = "staff")]
        public async Task<IActionResult> StaffList(string search, string status)
        {
            var requests = (await _visitorPassService.GetAllAsync())
                .Where(v => v.VisitDate.Date == DateTime.Today)
                .ToList();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                requests = requests.Where(r =>
                    r.VisitorName.ToLower().Contains(search) ||
                    r.VisitorContact.ToLower().Contains(search) ||
                    r.Purpose.ToLower().Contains(search)
                ).ToList();
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                requests = requests.Where(r => r.Status == status).ToList();
            }

            // Sort by most recent first
            requests = requests.OrderByDescending(r => r.RequestedAt).ToList();

            return View("/Views/Pages/Staff/VisitorPass/StaffList.cshtml", requests);
        }

        // Homeowner: View own requests
        [Authorize(Roles = "homeowner")]
        public async Task<IActionResult> MyRequests(string search, string status, DateTime? date)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var requests = await _visitorPassService.GetByUserAsync(userId);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                requests = requests.Where(r =>
                    r.VisitorName.ToLower().Contains(search) ||
                    r.VisitorContact.ToLower().Contains(search) ||
                    r.Purpose.ToLower().Contains(search)
                ).ToList();
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                requests = requests.Where(r => r.Status == status).ToList();
            }

            // Apply date filter
            if (date.HasValue)
            {
                requests = requests.Where(r => r.VisitDate.Date == date.Value.Date).ToList();
            }

            // Sort by most recent first
            requests = requests.OrderByDescending(r => r.RequestedAt).ToList();

            return View("/Views/Pages/User/VisitorPass/MyRequests.cshtml", requests);
        }

        // Homeowner: Create request
        [Authorize(Roles = "homeowner")]
        public IActionResult Create()
        {
            return View("/Views/Pages/User/VisitorPass/Create.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "homeowner")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VisitorPassRequestViewModel model)
        {
            try
            {
                // Set user ID from claims
                model.RequestedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                // Set default values for fields that aren't typically submitted by the form but required by validation
                if (string.IsNullOrEmpty(model.Status))
                    model.Status = "Pending";

                if (string.IsNullOrEmpty(model.AuditTrail))
                    model.AuditTrail = string.Empty;

                if (string.IsNullOrEmpty(model.RequestedByUserName))
                    model.RequestedByUserName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

                // These search/filter fields should be nullable in the model (fixed in updated ViewModel)
                model.SearchTerm = model.SearchTerm ?? string.Empty;
                model.StatusFilter = model.StatusFilter ?? string.Empty;

                Console.WriteLine($"RequestedByUserId: {model.RequestedByUserId}");

                if (!ModelState.IsValid)
                {
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors;
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"ModelState error for {key}: {error.ErrorMessage}");
                        }
                    }
                    return View("/Views/Pages/User/VisitorPass/Create.cshtml", model);
                }

                await _visitorPassService.CreateAsync(model);
                return RedirectToAction("MyRequests");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Create: {ex.Message}\n{ex.StackTrace}");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request.");
                return View("/Views/Pages/User/VisitorPass/Create.cshtml", model);
            }
        }

        // Admin: Approve/Decline
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _visitorPassService.ApproveAsync(id, userId);
            return RedirectToAction("AdminList");
        }
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Decline(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _visitorPassService.DeclineAsync(id, userId);
            return RedirectToAction("AdminList");
        }

        // Staff: Check-in/Check-out
        [Authorize(Roles = "staff")]
        public async Task<IActionResult> CheckIn(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _visitorPassService.CheckInAsync(id, userId);
            return RedirectToAction("StaffList");
        }
        [Authorize(Roles = "staff")]
        public async Task<IActionResult> CheckOut(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _visitorPassService.CheckOutAsync(id, userId);
            return RedirectToAction("StaffList");
        }

        // Homeowner: Cancel
        [Authorize(Roles = "homeowner")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _visitorPassService.CancelAsync(id, userId);
            return RedirectToAction("MyRequests");
        }

        // Homeowner: Edit request
        [Authorize(Roles = "homeowner")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var request = await _visitorPassService.GetByIdAsync(id);
            
            if (request == null || request.RequestedByUserId != userId || request.Status != "Pending")
            {
                return NotFound();
            }

            return View("/Views/Pages/User/VisitorPass/Edit.cshtml", request);
        }

        [HttpPost]
        [Authorize(Roles = "homeowner")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VisitorPassRequestViewModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                // Verify the request exists and belongs to the user
                var existingRequest = await _visitorPassService.GetByIdAsync(model.Id);
                if (existingRequest == null || existingRequest.RequestedByUserId != userId || existingRequest.Status != "Pending")
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    return View("/Views/Pages/User/VisitorPass/Edit.cshtml", model);
                }

                // Update the request
                var success = await _visitorPassService.UpdateAsync(model);
                if (success)
                {
                    return RedirectToAction("MyRequests");
                }

                ModelState.AddModelError(string.Empty, "Failed to update the request.");
                return View("/Views/Pages/User/VisitorPass/Edit.cshtml", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Edit: {ex.Message}\n{ex.StackTrace}");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request.");
                return View("/Views/Pages/User/VisitorPass/Edit.cshtml", model);
            }
        }

        // Details (all roles)
        public async Task<IActionResult> Details(int id)
        {
            var model = await _visitorPassService.GetByIdAsync(id);
            if (model == null) return NotFound();
            return View("/Views/Pages/Shared/VisitorPass/Details.cshtml", model);
        }

        // CSV Export Actions
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ExportAdminListToCsv(string search, string status, DateTime? date)
        {
            var requests = await _visitorPassService.GetAllRequestsAsync();
            return ExportToCsv(requests, "admin_visitor_passes");
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> ExportStaffListToCsv(string search, string status)
        {
            var requests = (await _visitorPassService.GetAllAsync())
                .Where(v => v.VisitDate.Date == DateTime.Today)
                .ToList();
            return ExportToCsv(requests, "staff_visitor_passes");
        }

        [Authorize(Roles = "homeowner")]
        public async Task<IActionResult> ExportMyRequestsToCsv(string search, string status, DateTime? date)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var requests = await _visitorPassService.GetByUserAsync(userId);
            return ExportToCsv(requests, "my_visitor_passes");
        }

        private IActionResult ExportToCsv(IEnumerable<VisitorPassRequestViewModel> requests, string fileName)
        {
            // Apply filters if needed
            if (!string.IsNullOrWhiteSpace(Request.Query["search"]))
            {
                var search = Request.Query["search"].ToString().ToLower();
                requests = requests.Where(r =>
                    r.VisitorName.ToLower().Contains(search) ||
                    r.VisitorContact.ToLower().Contains(search) ||
                    r.Purpose.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["status"]))
            {
                requests = requests.Where(r => r.Status == Request.Query["status"].ToString());
            }

            if (DateTime.TryParse(Request.Query["date"], out DateTime date))
            {
                requests = requests.Where(r => r.VisitDate.Date == date.Date);
            }

            // Create CSV content
            var csv = new StringBuilder();
            csv.AppendLine("Visitor Name,Contact,Purpose,Visit Date,Status,Requested By,Requested At");

            foreach (var req in requests)
            {
                csv.AppendLine($"\"{req.VisitorName}\",\"{req.VisitorContact}\",\"{req.Purpose}\"," +
                             $"\"{req.VisitDate:yyyy-MM-dd}\",\"{req.Status}\",\"{req.RequestedByUserName}\"," +
                             $"\"{req.RequestedAt:yyyy-MM-dd HH:mm}\"");
            }

            // Return CSV file
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"{fileName}_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
} 