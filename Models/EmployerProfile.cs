using System.ComponentModel.DataAnnotations;

namespace JobPortalSystem.Models
{
    public class EmployerProfile
    {
        [Key]
        public int EmployerProfileId { get; set; }

       
        public string EmployerId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public string Industry { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Company Description")]
        public string? Description { get; set; }

        public string? Website { get; set; }

        public string? Phone { get; set; }

        public string? LogoPath { get; set; }

        public ICollection<Job>? Jobs { get; set; }
    }
}