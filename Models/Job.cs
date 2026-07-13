using System.ComponentModel.DataAnnotations;

namespace JobPortalSystem.Models
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }

        public int EmployerId { get; set; }

        [Required]
        [Display(Name = "Job Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Required Skills")]
        public string RequiredSkills { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        public decimal Salary { get; set; }

        public DateTime PostedDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";

        // Navigation Properties
        public EmployerProfile? Employer { get; set; }

        public ICollection<Application>? Applications { get; set; }
    }
}