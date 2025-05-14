using Home_Sbdv.Models;
using Home_Sbdv.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace Home_Sbdv.Controllers
{
    public class ContactDirectoryController : Controller
    {
        private readonly IContactDirectoryService _contactService;
        public ContactDirectoryController(IContactDirectoryService contactService)
        {
            _contactService = contactService;
        }

        // List all contacts (open to all)
        [Authorize(Roles = "homeowner, admin, staff")]
        public async Task<IActionResult> Index(string? search, string? department, string? sort)
        {
            var contacts = await _contactService.GetAllContactsAsync();
            // Get unique departments for filter dropdown
            var departments = contacts
                .Where(c => !string.IsNullOrWhiteSpace(c.Department))
                .Select(c => c.Department!)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
            ViewBag.Departments = departments;
            // Filter by department
            if (!string.IsNullOrWhiteSpace(department))
            {
                contacts = contacts.Where(c => c.Department == department).ToList();
            }
            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                contacts = contacts.Where(c =>
                    (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.Department) && c.Department.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.Phone) && c.Phone.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(c.Email) && c.Email.ToLower().Contains(search))
                ).ToList();
            }
            // Sort
            sort = sort?.ToLower();
            contacts = sort switch
            {
                "name_desc" => contacts.OrderByDescending(c => c.Name).ToList(),
                "department_az" => contacts.OrderBy(c => c.Department).ThenBy(c => c.Name).ToList(),
                "department_za" => contacts.OrderByDescending(c => c.Department).ThenBy(c => c.Name).ToList(),
                _ => contacts.OrderBy(c => c.Name).ToList(), // Default: Name A-Z
            };
            ViewBag.Search = search;
            ViewBag.SelectedDepartment = department;
            ViewBag.Sort = sort;
            return View("/Views/Pages/User/ContactDirectory/Index.cshtml", contacts);
        }
            
        // View details (open to all)
        [Authorize(Roles = "admin,staff,homeowner")]
        public async Task<IActionResult> Details(int id)
        {
            var contact = await _contactService.GetContactByIdAsync(id);
            if (contact == null) return NotFound();
            return View("/Views/Pages/User/ContactDirectory/Details.cshtml", contact);
        }

        // Create (admin only)
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View("/Views/Pages/Admin/ContactDirectory/Create.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContactDirectoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View("/Views/Pages/Admin/ContactDirectory/Create.cshtml", model);
            await _contactService.CreateContactAsync(model);
            return RedirectToAction("Index");
        }

        // Edit (admin only)
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var contact = await _contactService.GetContactByIdAsync(id);
            if (contact == null) return NotFound();
            return View("/Views/Pages/Admin/ContactDirectory/Edit.cshtml", contact);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContactDirectoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View("/Views/Pages/Admin/ContactDirectory/Edit.cshtml", model);
            await _contactService.UpdateContactAsync(id, model);
            return RedirectToAction("Index");
        }

        // Delete (admin only)
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _contactService.GetContactByIdAsync(id);
            if (contact == null) return NotFound();
            return View("/Views/Pages/Admin/ContactDirectory/Delete.cshtml", contact);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _contactService.DeleteContactAsync(id);
            return RedirectToAction("Index");
        }
    }
}