using JobPortalSystem.Data;
using JobPortalSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize(Roles = "JobSeeker")]
public class ApplicationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ApplicationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Applications (ONLY current user)
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var apps = await _context.Applications
            .Include(a => a.Job)
            .Include(a => a.JobSeeker)
            .Where(a => a.JobSeeker.UserId == userId)
            .ToListAsync();

        return View(apps);
    }

    // GET: Create
    public IActionResult Create(int jobId)
    {
        var application = new Application
        {
            JobId = jobId,
            AppliedDate = DateTime.Now,
            Status = "Pending"
        };

        return View(application);
    }

    // POST: Create (FIXED + DUPLICATE CHECK ADDED)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Application application)
    {
        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var jobSeeker = await _context.JobSeekers
                .FirstOrDefaultAsync(j => j.UserId == userId);

            if (jobSeeker == null)
            {
                return Content("JobSeeker profile not found.");
            }

            // ✅ DUPLICATE CHECK (FIXED PLACE)
            var exists = await _context.Applications
                .AnyAsync(a => a.JobId == application.JobId &&
                               a.JobSeekerId == jobSeeker.JobSeekerId);

            if (exists)
            {
                return Content("You already applied for this job.");
            }

            application.JobSeekerId = jobSeeker.JobSeekerId;
            application.AppliedDate = DateTime.Now;
            application.Status = "Pending";

            _context.Add(application);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Jobs");
        }

        return View(application);
    }

    // GET: Details
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var app = await _context.Applications
            .Include(a => a.Job)
            .Include(a => a.JobSeeker)
            .FirstOrDefaultAsync(m => m.ApplicationId == id);

        if (app == null) return NotFound();

        return View(app);
    }

    // GET: Delete
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var app = await _context.Applications
            .Include(a => a.Job)
            .Include(a => a.JobSeeker)
            .FirstOrDefaultAsync(m => m.ApplicationId == id);

        if (app == null) return NotFound();

        return View(app);
    }

    // POST: Delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var app = await _context.Applications.FindAsync(id);

        if (app != null)
        {
            _context.Applications.Remove(app);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // My Applications
    public async Task<IActionResult> MyApplications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var apps = await _context.Applications
            .Include(a => a.Job)
            .Include(a => a.JobSeeker)
            .Where(a => a.JobSeeker.UserId == userId)
            .ToListAsync();

        return View(apps);
    }
}