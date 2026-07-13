using JobPortalSystem.Models;
namespace JobPortalSystem.ViewModels

{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }

        public int TotalEmployers { get; set; }

        public int TotalJobSeekers { get; set; }

        public int TotalJobs { get; set; }

        public int TotalApplications { get; set; }

        public int PendingJobs { get; set; }

        public int ApprovedJobs { get; set; }

        public int RejectedJobs { get; set; }
        public List<Job> RecentJobs { get; set; } = new();
    }
}