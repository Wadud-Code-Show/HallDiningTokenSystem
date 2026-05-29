namespace HallDiningTokenSystem.Models
{
    /// <summary>
    /// Student Meal view model for list/grid display.
    /// Shows available meals with essential information.
    /// </summary>
    public class StudentMealViewModel
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
        public bool CanPurchase => CurrentStatus == "Sale Open" && RemainingCapacity > 0;

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

        public bool IsPurchaseReady => !string.IsNullOrEmpty(CurrentStatus) && CurrentStatus != "Unknown";
    }
}
