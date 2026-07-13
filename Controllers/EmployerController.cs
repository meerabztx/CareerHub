using JobPortalSystem.Data;
using JobPortalSystem.Models;
using JobPortalSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobPortalSystem.Controllers
{
    [Authorize(Roles = "Employer")]
    public class EmployerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // DASHBOARD
        // =====================================================

        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var employer = await _context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.EmployerId == userId);

            if (employer == null)
                return RedirectToAction(nameof(CompanyProfile));

            var jobs = await _context.Jobs
                .Where(j => j.EmployerId == employer.EmployerProfileId)
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            var jobIds = jobs.Select(j => j.JobId).ToList();

            var applications = await _context.Applications
                .Where(a => jobIds.Contains(a.JobId))
                .ToListAsync();

            var model = new EmployerDashboardViewModel
            {
                TotalJobs = jobs.Count,
                TotalApplications = applications.Count,
                PendingApplications = applications.Count(a => a.Status == "Pending"),
                AcceptedApplications = applications.Count(a => a.Status == "Accepted"),
                RejectedApplications = applications.Count(a => a.Status == "Rejected"),
                RecentJobs = jobs.Take(5).ToList()
            };

            return View(model);
        }

        // =====================================================
        // COMPANY PROFILE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> CompanyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.EmployerId == userId);

            if (profile == null)
            {
                profile = new EmployerProfile
                {
                    EmployerId = userId!
                };
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompanyProfile(EmployerProfile profile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            profile.EmployerId = userId;

            if (!ModelState.IsValid)
                return View(profile);

            var existing = await _context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.EmployerId == userId);

            if (existing == null)
            {
                _context.EmployerProfiles.Add(profile);
            }
            else
            {
                existing.CompanyName = profile.CompanyName;
                existing.Industry = profile.Industry;
                existing.Location = profile.Location;
                existing.Phone = profile.Phone;
                existing.Website = profile.Website;
                existing.Description = profile.Description;

                _context.Update(existing);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Company profile saved successfully.";

            return RedirectToAction(nameof(CompanyProfile));
        }

        // =====================================================
        // CREATE JOB
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> CreateJob()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var employer = await _context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.EmployerId == userId);

            if (employer == null)
            {
                TempData["Error"] = "Please complete your company profile first.";
                return RedirectToAction(nameof(CompanyProfile));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJob(Job job)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var employer = await _context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.EmployerId == userId);

            if (employer == null)
            {
                TempData["Error"] = "Please complete your company profile first.";
                return RedirectToAction(nameof(CompanyProfile));
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

            TempData["Success"] = "Job submitted successfully. Waiting for admin approval.";

            return RedirectToAction(nameof(MyJobs));
        }

        // =====================================================
        // MY JOBS
        // =====================================================

        public async Task<IActionResult> MyJobs()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var employer = await _context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.EmployerId == userId);

            if (employer == null)
                return RedirectToAction(nameof(CompanyProfile));

            var jobs = await _context.Jobs
                .Where(j => j.EmployerId == employer.EmployerProfileId)
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            return View(jobs);
        }
        // =====================================================
        // VIEW APPLICANTS
        // =====================================================

        public async Task<IActionResult> Applicants(int id)
        {
            var applicants = await _context.Applications
                .Include(a => a.Job)
                .Include(a => a.JobSeeker)
                .Where(a => a.JobId == id)
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();

            ViewBag.JobTitle = applicants.FirstOrDefault()?.Job?.Title ?? "Applicants";

            return View(applicants);
        }

        // =====================================================
        // ACCEPT APPLICANT
        // =====================================================

        public async Task<IActionResult> AcceptApplicant(int id)
        {
            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null)
                return NotFound();

            application.Status = "Accepted";

            // Reject everyone else for the same job
            var otherApplications = await _context.Applications
                .Where(a => a.JobId == application.JobId &&
                            a.ApplicationId != application.ApplicationId)
                .ToListAsync();

            foreach (var item in otherApplications)
            {
                item.Status = "Rejected";

                _context.Notifications.Add(new Notification
                {
                    JobSeekerId = item.JobSeekerId,
                    Title = "Application Update",
                    Message = $"Unfortunately, your application for '{application.Job!.Title}' at {application.Job.CompanyName} was not selected.",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
            }

            _context.Notifications.Add(new Notification
            {
                JobSeekerId = application.JobSeekerId,
                Title = "Congratulations!",
                Message = $"Congratulations! You have been selected for '{application.Job!.Title}' at {application.Job.CompanyName}.",
                CreatedAt = DateTime.Now,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Applicant accepted successfully.";

            return RedirectToAction(nameof(Applicants), new { id = application.JobId });
        }

        // =====================================================
        // REJECT APPLICANT
        // =====================================================

        public async Task<IActionResult> RejectApplicant(int id)
        {
            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null)
                return NotFound();

            application.Status = "Rejected";

            _context.Notifications.Add(new Notification
            {
                JobSeekerId = application.JobSeekerId,
                Title = "Application Update",
                Message = $"Your application for '{application.Job!.Title}' at {application.Job.CompanyName} was not selected.",
                CreatedAt = DateTime.Now,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Applicant rejected.";

            return RedirectToAction(nameof(Applicants), new { id = application.JobId });
        }
    }
}