namespace HallDiningTokenSystem.Models
{
    /// <summary>
    /// Read-only ViewModel for displaying meal details.
    /// Used for inspection, history, and audit purposes only.
    /// </summary>
    public class MealDetailsViewModel
    {
        // Basic Information
        public int Id { get; set; }
        public string MealName { get; set; } = null!;
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public int SoldCount { get; set; }

        // Token Purchase Phase
        public DateTime TokenSellStartDateTime { get; set; }
        public DateTime TokenSellEndDateTime { get; set; }

        // Meal Serving Phase
        public DateTime MealServingStartDateTime { get; set; }
        public DateTime MealServingEndDateTime { get; set; }

        // Status
        public DateTime CreatedAt { get; set; }
        public bool IsArchived { get; set; }
        public string CurrentStatus { get; set; } = null!; // Archived, Upcoming, Sale Open, Sale Closed, Serving, Completed

        // Computed Properties - Read Only
        public int RemainingCapacity => Capacity - SoldCount;
        public decimal Revenue => Price * SoldCount;

        /// <summary>
        /// Gets the dynamic status based on time and archive state.
        /// Hall Dining has 6 meal statuses:
        /// - Archived: meal was manually archived
        /// - Upcoming: token sale hasn't started yet
        /// - Sale Open: students can currently buy tokens
        /// - Sale Closed: token sale ended, meal serving not started
        /// - Serving: students can collect their meals
        /// - Completed: meal lifecycle finished
        /// </summary>
        public static string GetMealStatus(
            DateTime tokenSellStartDateTime,
            DateTime tokenSellEndDateTime,
            DateTime mealServingStartDateTime,
            DateTime mealServingEndDateTime,
            bool isArchived)
        {
            if (isArchived)
                return "Archived";

            var now = DateTime.Now;

            if (now < tokenSellStartDateTime)
                return "Upcoming";

            if (now >= tokenSellStartDateTime && now <= tokenSellEndDateTime)
                return "Sale Open";

            if (now > tokenSellEndDateTime && now < mealServingStartDateTime)
                return "Sale Closed";

            if (now >= mealServingStartDateTime && now <= mealServingEndDateTime)
                return "Serving";

            return "Completed";
        }

        /// <summary>
        /// Gets the Bootstrap badge class for the status.
        /// </summary>
        public string GetStatusBadgeClass()
        {
            return CurrentStatus switch
            {
                "Upcoming" => "bg-secondary",
                "Sale Open" => "bg-success",
                "Sale Closed" => "bg-warning",
                "Serving" => "bg-primary",
                "Completed" => "bg-danger",
                "Archived" => "bg-dark",
                _ => "bg-secondary"
            };
        }

        /// <summary>
        /// Determines if this meal can be edited.
        /// Archived meals are never editable.
        /// Non-archived meals can only be edited before serving starts.
        /// </summary>
        public bool CanEdit => !IsArchived && DateTime.Now < MealServingStartDateTime;
    }
}
