using System.ComponentModel.DataAnnotations;

namespace JobPortalSystem.Models
{
    public class Application
    {
        [Key]
        public int ApplicationId { get; set; }

        public int JobId { get; set; }

        public int JobSeekerId { get; set; }

        public DateTime AppliedDate { get; set; }

        public string Status { get; set; } = "Pending";
        public string? CoverLetter { get; set; }

        // Navigation Properties
        public Job? Job { get; set; }

        public JobSeeker? JobSeeker { get; set; }
    }
}