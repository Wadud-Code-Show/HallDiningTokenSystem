using Microsoft.AspNetCore.Identity;

namespace HallDiningTokenSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? StudentId { get; set; }
    }
}
