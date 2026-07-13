using JobPortalSystem.Data;
using JobPortalSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobPortalSystem.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // Browse Jobs
        // =========================
        public async Task<IActionResult> Index()
        {
            var jobs = await _context.Jobs
                .Where(j => j.Status == "Approved")
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                var seeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (seeker != null)
                {
                    ViewBag.AppliedJobs = await _context.Applications
                        .Where(a => a.JobSeekerId == seeker.JobSeekerId)
                        .Select(a => a.JobId)
                        .ToListAsync();

                    ViewBag.NotificationCount = await _context.Notifications
                        .CountAsync(n => n.JobSeekerId == seeker.JobSeekerId && !n.IsRead);
                }
            }

            return View(jobs);
        }

        // =========================
        // Job Details
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var job = await _context.Jobs
                .Include(j => j.Employer)
                .FirstOrDefaultAsync(j => j.JobId == id);

            if (job == null)
                return NotFound();

            return View(job);
        }

        // =========================
        // Employer Create Job
        // =========================
        [Authorize(Roles = "Employer")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Employer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Job job)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var employer = await _context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.EmployerId == userId);

            if (employer == null)
            {
                TempData["Error"] = "Please complete your company profile first.";
                return RedirectToAction("CompanyProfile", "Employer");
            }

            job.CompanyName = employer.CompanyName;
            job.EmployerId = employer.EmployerProfileId;
            job.PostedDate = DateTime.Now;
            job.Status = "Pending";

            ModelState.Remove(nameof(Job.CompanyName));
            ModelState.Remove(nameof(Job.EmployerId));
            ModelState.Remove(nameof(Job.PostedDate));
            ModelState.Remove(nameof(Job.Status));

            if (!ModelState.IsValid)
                return View(job);

            _context.Jobs.Add(job);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Job submitted successfully. Waiting for Admin approval.";

            return RedirectToAction("MyJobs", "Employer");
        }

        // =========================
        // Employer Edit Job
        // =========================
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
                return NotFound();

            return View(job);
        }

        [HttpPost]
        [Authorize(Roles = "Employer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Job job)
        {
            if (id != job.JobId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(job);

            try
            {
                var existingJob = await _context.Jobs.FindAsync(id);

                if (existingJob == null)
                    return NotFound();

                existingJob.Title = job.Title;
                existingJob.Description = job.Description;
                existingJob.RequiredSkills = job.RequiredSkills;
                existingJob.Location = job.Location;
                existingJob.Salary = job.Salary;

                // Every edit goes back for approval
                existingJob.Status = "Pending";

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Job updated successfully and sent for approval.";
            }
            catch
            {
                TempData["Error"] =
                    "Unable to update job.";
            }

            return RedirectToAction("MyJobs", "Employer");
        }

        // =========================
        // Delete Job
        // =========================
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == id);

            if (job == null)
                return NotFound();

            return View(job);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Employer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job != null)
            {
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Job deleted.";

            return RedirectToAction("MyJobs", "Employer");
        }

        // =========================
        // Apply Job
        // =========================
        [HttpPost]
        [Authorize(Roles = "JobSeeker")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int jobId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Challenge();

            var seeker = await _context.JobSeekers
                .FirstOrDefaultAsync(j => j.UserId == userId);

            if (seeker == null)
            {
                TempData["Error"] =
                    "Please complete your profile before applying.";

                return RedirectToAction("Create", "JobSeekers");
            }

            bool alreadyApplied = await _context.Applications.AnyAsync(a =>
                a.JobId == jobId &&
                a.JobSeekerId == seeker.JobSeekerId);

            if (alreadyApplied)
            {
                TempData["Error"] =
                    "You have already applied.";

                return RedirectToAction(nameof(Index));
            }

            _context.Applications.Add(new Application
            {
                JobId = jobId,
                JobSeekerId = seeker.JobSeekerId,
                AppliedDate = DateTime.Now,
                Status = "Pending"
            });

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Application submitted successfully.";

            return RedirectToAction("MyApplications", "JobSeekers");
        }

        // =========================
        // Notifications
        // =========================
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Notifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var seeker = await _context.JobSeekers
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (seeker == null)
                return RedirectToAction("Create", "JobSeekers");

            var notifications = await _context.Notifications
                .Where(n => n.JobSeekerId == seeker.JobSeekerId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            foreach (var notification in notifications)
                notification.IsRead = true;

            await _context.SaveChangesAsync();

            return View(notifications);
        }

        private bool JobExists(int id)
        {
            return _context.Jobs.Any(e => e.JobId == id);
        }
    }
}