namespace HallDiningTokenSystem.Models
{
    /// <summary>
    /// Student Meal Details view model for detailed read-only inspection.
    /// Prepares UI for token purchase functionality (not yet implemented).
    /// </summary>
    public class StudentMealDetailsViewModel
    {
        public int Id { get; set; }
        public string MealName { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime TokenSellStartDateTime { get; set; }
        public DateTime TokenSellEndDateTime { get; set; }
        public DateTime MealServingStartDateTime { get; set; }
        public DateTime MealServingEndDateTime { get; set; }
        public int Capacity { get; set; }
        public int SoldCount { get; set; }

        // Computed Properties
        public int RemainingCapacity => Capacity > 0 ? Capacity - SoldCount : 0;
        public string CurrentStatus => GetStatus();
        public string StatusBadgeClass => GetStatusBadgeClass();

        /// <summary>
        /// Calculates current status based on time.
        /// Students only see Upcoming and Sale Open meals.
        /// </summary>
        private string GetStatus()
        {
            var now = DateTime.Now;

            if (now < TokenSellStartDateTime)
                return "Upcoming";

            if (now >= TokenSellStartDateTime && now <= TokenSellEndDateTime)
                return "Sale Open";

            // Should never reach here as non-operational meals are filtered server-side
            return "Unknown";
        }

        /// <summary>
        /// Returns Bootstrap badge class for status display.
        /// </summary>
        private string GetStatusBadgeClass()
        {
            return CurrentStatus switch
            {
                "Upcoming" => "bg-secondary",
                "Sale Open" => "bg-success",
                _ => "bg-secondary"
            };
        }

        /// <summary>
        /// Determines if student can purchase this meal.
        /// Must be in Sale Open phase and have remaining capacity.
        /// </summary>
        public bool CanPurchase => CurrentStatus == "Sale Open" && RemainingCapacity > 0;

        /// <summary>
        /// Indicates that purchase functionality is available for this meal.
        /// </summary>
        public bool IsPurchaseReady => CanPurchase;
    }
}
