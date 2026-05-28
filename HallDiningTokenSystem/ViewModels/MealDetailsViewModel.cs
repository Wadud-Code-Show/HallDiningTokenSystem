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

        // Timeline
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedAt { get; set; }

        // Status
        public bool IsArchived { get; set; }
        public string CurrentStatus { get; set; } = null!; // Upcoming, Active, Expired, Archived

        // Computed Properties - Read Only
        public int RemainingCapacity => Capacity - SoldCount;
        public decimal Revenue => Price * SoldCount;

        /// <summary>
        /// Gets the dynamic status based on time and archive state.
        /// </summary>
        public static string GetMealStatus(DateTime startTime, DateTime endTime, bool isArchived)
        {
            if (isArchived)
                return "Archived";

            if (DateTime.Now < startTime)
                return "Upcoming";

            if (DateTime.Now >= startTime && DateTime.Now <= endTime)
                return "Active";

            return "Expired";
        }

        /// <summary>
        /// Gets the Bootstrap badge class for the status.
        /// </summary>
        public string GetStatusBadgeClass()
        {
            return CurrentStatus switch
            {
                "Upcoming" => "bg-secondary",
                "Active" => "bg-success",
                "Expired" => "bg-danger",
                "Archived" => "bg-dark",
                _ => "bg-secondary"
            };
        }

        /// <summary>
        /// Determines if this meal can be edited.
        /// Archived and expired meals are read-only.
        /// </summary>
        public bool CanEdit => !IsArchived && DateTime.Now < EndTime;
    }
}
