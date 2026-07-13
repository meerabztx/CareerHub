using JobPortalSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobPortalSystem.Controllers
{
    [Authorize(Roles = "JobSeeker")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
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

            return View(notifications);
        }
    }
}