using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortalSystem.Models;
using JobPortalSystem.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace JobPortalSystem.Controllers
{
    [Authorize]
    public class JobSeekersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public JobSeekersController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        // GET: JobSeekers
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.JobSeekers
                .FirstOrDefaultAsync(j => j.UserId == userId);

            if (profile == null)
            {
                return RedirectToAction(nameof(Create));
            }

            return RedirectToAction(nameof(Edit), new
            {
                id = profile.JobSeekerId
            });
        }
        // GET: JobSeekers
        public async Task<IActionResult> MyApplications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var jobSeeker = await _context.JobSeekers
                .FirstOrDefaultAsync(j => j.UserId == userId);

            if (jobSeeker == null)
                return RedirectToAction("Create");

            var applications = await _context.Applications
                .Include(a => a.Job)
                .Where(a => a.JobSeekerId == jobSeeker.JobSeekerId)
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();

            return View(applications);
        }

        // GET: Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var jobSeeker = await _context.JobSeekers
                .FirstOrDefaultAsync(m => m.JobSeekerId == id);

            if (jobSeeker == null) return NotFound();

            return View(jobSeeker);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create (WITH RESUME UPLOAD)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobSeeker jobSeeker, IFormFile resumeFile)
        {
            if (ModelState.IsValid)
            {
                // save file
                if (resumeFile != null && resumeFile.Length > 0)
                {
                    string fileName = Path.GetFileName(resumeFile.FileName);
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/resumes");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await resumeFile.CopyToAsync(stream);
                    }

                    jobSeeker.ResumePath = "/resumes/" + fileName;
                }
                jobSeeker.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                _context.Add(jobSeeker);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(EditProfile));

            }

            return View(jobSeeker);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var jobSeeker = await _context.JobSeekers.FindAsync(id);

            if (jobSeeker == null) return NotFound();

            return View(jobSeeker);
        }
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.JobSeekers
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (profile == null)
                return RedirectToAction(nameof(Create));

            return RedirectToAction(nameof(Edit), new { id = profile.JobSeekerId });
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JobSeeker jobSeeker)
        {
            if (id != jobSeeker.JobSeekerId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // keep original user id
                    var existing = await _context.JobSeekers.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.JobSeekerId == id);

                    jobSeeker.UserId = existing.UserId;

                    _context.Update(jobSeeker);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobSeekerExists(jobSeeker.JobSeekerId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(jobSeeker);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var jobSeeker = await _context.JobSeekers
                .FirstOrDefaultAsync(m => m.JobSeekerId == id);

            if (jobSeeker == null) return NotFound();

            return View(jobSeeker);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jobSeeker = await _context.JobSeekers.FindAsync(id);

            if (jobSeeker != null)
            {
                _context.JobSeekers.Remove(jobSeeker);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool JobSeekerExists(int id)
        {
            return _context.JobSeekers.Any(e => e.JobSeekerId == id);
        }
        // =========================
        // Notifications
        // =========================


        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Notifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var jobSeeker = await _context.JobSeekers
                .FirstOrDefaultAsync(j => j.UserId == userId);

            if (jobSeeker == null)
                return RedirectToAction(nameof(Create));

            var notifications = await _context.Notifications
                .Where(n => n.JobSeekerId == jobSeeker.JobSeekerId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> ReadNotification(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var jobSeeker = await _context.JobSeekers
                .FirstOrDefaultAsync(j => j.UserId == userId);

            if (jobSeeker == null)
                return RedirectToAction(nameof(Create));

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.NotificationId == id &&
                    n.JobSeekerId == jobSeeker.JobSeekerId);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Notifications));
        }
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> ReadAllNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var jobSeeker = await _context.JobSeekers
                .FirstOrDefaultAsync(j => j.UserId == userId);

            if (jobSeeker == null)
                return RedirectToAction(nameof(Create));

            var notifications = await _context.Notifications
                .Where(n => n.JobSeekerId == jobSeeker.JobSeekerId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "All notifications marked as read.";

            return RedirectToAction(nameof(Notifications));
        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var jobSeeker = await _context.JobSeekers
                .FirstOrDefaultAsync(j => j.UserId == userId);

            if (jobSeeker == null)
                return RedirectToAction(nameof(Create));

            var notifications = await _context.Notifications
                .Where(n => n.JobSeekerId == jobSeeker.JobSeekerId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "All notifications marked as read.";

            return RedirectToAction(nameof(Notifications));
        }
    }
}