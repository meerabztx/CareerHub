using Microsoft.AspNetCore.Identity;

namespace JobPortalSystem.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}