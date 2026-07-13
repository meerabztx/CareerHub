using System.ComponentModel.DataAnnotations;

namespace JobPortalSystem.Models
{
    public class JobSeeker
    {
        [Key]
        public int JobSeekerId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Skills { get; set; } = string.Empty;

        [Required]
        public string Experience { get; set; } = string.Empty;

        [Required]
        public string Education { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;


        public string? ResumePath { get; set; }
    }
}