using System.ComponentModel.DataAnnotations;

namespace HallDiningTokenSystem.Models
{
    public class WalletTopUpViewModel
    {
        [Required(ErrorMessage = "Student ID is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Student ID must be 6 digits, for example 220130.")]
        public string StudentId { get; set; } = null!;

        [Required(ErrorMessage = "Top-up amount is required.")]
        [Range(1, 999999999, ErrorMessage = "Top-up amount must be a whole number greater than 0.")]
        public int? Amount { get; set; }

        [StringLength(200, ErrorMessage = "Remarks cannot exceed 200 characters.")]
        public string? Remarks { get; set; }

        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
        public string? Note { get; set; }

        // Internal user ID (populated after search)
        public string? StudentUserId { get; set; }

        // Display only properties (populated after search)
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public decimal CurrentBalance { get; set; }
    }
}
