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

        [Required(ErrorMessage = "Token sale start is required.")]
        public DateTime TokenSellStartDateTime { get; set; }

        [Required(ErrorMessage = "Token sale end is required.")]
        public DateTime TokenSellEndDateTime { get; set; }

        [Required(ErrorMessage = "Meal serving start is required.")]
        public DateTime MealServingStartDateTime { get; set; }

        [Required(ErrorMessage = "Meal serving end is required.")]
        public DateTime MealServingEndDateTime { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0.")]
        public int Capacity { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        // Read-only - used for validation and locking logic
        public int SoldCount { get; set; }

        /// <summary>
        /// Determines if TokenSellStartDateTime can be edited.
        /// Locked if tokens are already sold.
        /// </summary>
        public bool CanEditTokenSellStart => SoldCount == 0;

        /// <summary>
        /// Determines if Price can be edited.
        /// Locked if tokens are already sold.
        /// </summary>
        public bool CanEditPrice => SoldCount == 0;
    }
}
