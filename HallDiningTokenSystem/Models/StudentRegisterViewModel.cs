using System.ComponentModel.DataAnnotations;

namespace HallDiningTokenSystem.Models
{
    public class StudentRegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Student ID is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Student ID must be 6 digits, for example 220130.")]
        [StringLength(50)]
        [Display(Name = "Student ID")]
        public string StudentId { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(256)]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
