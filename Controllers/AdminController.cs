using JobPortalSystem.Data;
using JobPortalSystem.Models;
using JobPortalSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortalSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================
        // Dashboard
        // ==========================
        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalEmployers = await _context.EmployerProfiles.CountAsync(),
                TotalJobSeekers = await _context.JobSeekers.CountAsync(),
                TotalJobs = await _context.Jobs.CountAsync(),
                TotalApplications = await _context.Applications.CountAsync(),

                ApprovedJobs = await _context.Jobs.CountAsync(j => j.Status == "Approved"),
                PendingJobs = await _context.Jobs.CountAsync(j => j.Status == "Pending"),
                RejectedJobs = await _context.Jobs.CountAsync(j => j.Status == "Rejected"),

                RecentJobs = await _context.Jobs
                    .OrderByDescending(j => j.PostedDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        //users

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();

            return View(users);
        }

        // ==========================
        // Employers
        // ==========================
        public async Task<IActionResult> Employers()
        {
            var employers = await _context.EmployerProfiles
                .OrderBy(e => e.CompanyName)
                .ToListAsync();

            return View(employers);
        }

        public async Task<IActionResult> DeleteEmployer(int id)
        {
            var employer = await _context.EmployerProfiles.FindAsync(id);

            if (employer == null)
                return NotFound();

            _context.EmployerProfiles.Remove(employer);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Employer deleted successfully.";

            return RedirectToAction(nameof(Employers));
        }

        // ==========================
        // Job Seekers
        // ==========================
        public async Task<IActionResult> JobSeekers()
        {
            var seekers = await _context.JobSeekers
                .OrderBy(j => j.FullName)
                .ToListAsync();

            return View(seekers);
        }

        public async Task<IActionResult> DeleteJobSeeker(int id)
        {
            var seeker = await _context.JobSeekers.FindAsync(id);

            if (seeker == null)
                return NotFound();

            _context.JobSeekers.Remove(seeker);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Job seeker deleted successfully.";

            return RedirectToAction(nameof(JobSeekers));
        }

        // ==========================
        // Jobs
        // ==========================
        public async Task<IActionResult> Jobs()
        {
            var jobs = await _context.Jobs
                .Include(j => j.Employer)
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            return View(jobs);
        }

        public async Task<IActionResult> ApproveJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
                return NotFound();

            job.Status = "Approved";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Job approved successfully.";

            return RedirectToAction(nameof(Jobs));
        }

        public async Task<IActionResult> RejectJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
                return NotFound();

            job.Status = "Rejected";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Job rejected successfully.";

            return RedirectToAction(nameof(Jobs));
        }

        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
                return NotFound();

            _context.Jobs.Remove(job);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Job deleted successfully.";

            return RedirectToAction(nameof(Jobs));
        }

        // ==========================
        // Applications
        // ==========================
        public async Task<IActionResult> Applications()
        {
            var applications = await _context.Applications
                .Include(a => a.Job)
                .Include(a => a.JobSeeker)
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();

            return View(applications);
        }
    }
}