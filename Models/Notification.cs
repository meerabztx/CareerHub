using System.ComponentModel.DataAnnotations;

namespace JobPortalSystem.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public int JobSeekerId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        // Navigation Property
        public JobSeeker? JobSeeker { get; set; }
    }
}