using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Home_Sbdv.Controllers
{
    [Authorize]
    public class AnnouncementController : Controller
    {
        private readonly AppDbContext _context;

        public AnnouncementController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Announcement/AnnouncementList
        public async Task<IActionResult> AnnouncementList()
        {
            var announcements = await _context.Announcements
                .Include(a => a.User) // Fetch user details
                .ToListAsync();

            return View(announcements);
        }

        // GET: Announcement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.User) // Include user details
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // GET: Announcement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Announcement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content")] Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                // Ensure user is authenticated before proceeding
                if (User.Identity == null || string.IsNullOrEmpty(User.Identity.Name))
                {
                    return Unauthorized(); // User is not logged in
                }

                // Get the logged-in user's ID (modify this based on your authentication system)
                var userId = _context.Users
                    .Where(u => u.Username == User.Identity.Name)
                    .Select(u => u.Id)
                    .FirstOrDefault();

                if (userId == 0) // If no user is found
                {
                    return Unauthorized();
                }

                announcement.PostedBy = userId; // Assign the user's ID
                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(AnnouncementList));
            }
            return View(announcement);
        }



        // GET: Announcement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        // POST: Announcement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content")] Announcement updatedAnnouncement)
        {
            if (id != updatedAnnouncement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAnnouncement = await _context.Announcements
                        .FirstOrDefaultAsync(a => a.Id == id);

                    if (existingAnnouncement == null)
                    {
                        return NotFound();
                    }

                    // Update only the necessary fields
                    existingAnnouncement.Title = updatedAnnouncement.Title;
                    existingAnnouncement.Content = updatedAnnouncement.Content;
                    existingAnnouncement.UpdatedAt = DateTime.Now; // Use local time

                    _context.Update(existingAnnouncement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Announcements.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AnnouncementList));
            }
            return View(updatedAnnouncement);
        }



        // GET: Announcement/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.User) // Include User details
                .FirstOrDefaultAsync(m => m.Id == id);

            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }


        // POST: Announcement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(AnnouncementList));
        }
    }
}
