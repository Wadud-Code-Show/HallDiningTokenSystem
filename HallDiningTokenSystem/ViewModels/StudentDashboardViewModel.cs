namespace HallDiningTokenSystem.Models
{
    /// <summary>
    /// Student Dashboard view model containing wallet summary, token status, and meal counts.
    /// </summary>
    public class StudentDashboardViewModel
    {
        // Wallet Information
        public decimal CurrentBalance { get; set; }

        // Token Summary (safe defaults if system not fully implemented)
        public int TotalPurchasedTokens { get; set; } = 0;
        public int TotalRedeemedTokens { get; set; } = 0;
        public int ActiveTokens => TotalPurchasedTokens - TotalRedeemedTokens;

        // Meal Summary
        public int UpcomingMealsCount { get; set; }
        public int ActiveMealsCount { get; set; }
        public int TotalAvailableMeals => UpcomingMealsCount + ActiveMealsCount;

        // Student Info
        public string StudentId { get; set; } = null!;
        public string StudentName { get; set; } = null!;
    }
}
