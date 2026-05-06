using Microsoft.AspNetCore.Identity;

namespace HallDiningTokenSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? StudentId { get; set; }
        public string? Department { get; set; }
        public string? Batch { get; set; }
    }
}
