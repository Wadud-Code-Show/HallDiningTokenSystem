using System.ComponentModel.DataAnnotations;

namespace HallDiningTokenSystem.Models
{
    public class CreateMealViewModel
    {
        [Required(ErrorMessage = "Meal name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Meal name must be between 2 and 100 characters.")]
        public string MealName { get; set; } = null!;

        [Required(ErrorMessage = "Meal type is required.")]
        [StringLength(50, ErrorMessage = "Meal type cannot exceed 50 characters.")]
        public string MealType { get; set; } = null!;

        [Required(ErrorMessage = "Token sale start is required.")]
        public DateTime TokenSellStartDateTime { get; set; }

        [Required(ErrorMessage = "Token sale end is required.")]
        public DateTime TokenSellEndDateTime { get; set; }

        [Required(ErrorMessage = "Meal serving start is required.")]
        public DateTime MealServingStartDateTime { get; set; }

        [Required(ErrorMessage = "Meal serving end is required.")]
        public DateTime MealServingEndDateTime { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0.")]
        public int Capacity { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
    }
}
