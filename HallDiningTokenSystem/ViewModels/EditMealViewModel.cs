using System.ComponentModel.DataAnnotations;

namespace HallDiningTokenSystem.Models
{
    public class EditMealViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Meal name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Meal name must be between 2 and 100 characters.")]
        public string MealName { get; set; } = null!;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0.")]
        public int Capacity { get; set; }

        // Read-only - used for validation and locking logic
        public int SoldCount { get; set; }
    }
}
