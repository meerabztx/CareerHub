using JobPortalSystem.Models;

namespace JobPortalSystem.ViewModels
{
    public class EmployerDashboardViewModel
    {
        public int TotalJobs { get; set; }

        public int TotalApplications { get; set; }

        public int PendingApplications { get; set; }

        public int AcceptedApplications { get; set; }

        public int RejectedApplications { get; set; }

        public List<Job> RecentJobs { get; set; } = new();
    }
}