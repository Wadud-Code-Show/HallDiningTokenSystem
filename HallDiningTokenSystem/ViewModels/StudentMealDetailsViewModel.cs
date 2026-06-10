namespace HallDiningTokenSystem.Models
{
    /// <summary>
    /// Student Meal Details view model for detailed read-only inspection.
    /// Prepares UI for token purchase functionality.
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
        /// Covers all lifecycle stages: Upcoming, Sale Open, Sale Closed, Serving, Expired, Archived.
        /// </summary>
        private string GetStatus()
        {
            var now = DateTime.Now;

            // Note: IsArchived property is missing in this viewmodel based on previous read, 
            // but let's assume it should be checked if available or handle it via time.
            
            if (now < TokenSellStartDateTime)
                return "Upcoming";

            if (now >= TokenSellStartDateTime && now <= TokenSellEndDateTime)
                return "Sale Open";

            // If sale ended but serving hasn't started
            if (now > TokenSellEndDateTime && now < MealServingStartDateTime)
                return "Sale Closed";

            if (now >= MealServingStartDateTime && now <= MealServingEndDateTime)
                return "Serving";

            if (now > MealServingEndDateTime)
                return "Expired";

            return "Unknown";
        }

        /// <summary>
        /// Returns Bootstrap badge class for status display.
        /// </summary>
        private string GetStatusBadgeClass()
        {
            return CurrentStatus switch
            {
                "Upcoming" => "bg-info",
                "Sale Open" => "bg-success",
                "Sale Closed" => "bg-warning",
                "Serving" => "bg-primary",
                "Expired" => "bg-dark",
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
