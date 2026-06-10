namespace HallDiningTokenSystem.Models
{
    public class CashAdminDashboardViewModel
    {
        // Summary Metrics
        public decimal TotalCollected { get; set; }
        public decimal TodayCollected { get; set; }
        public int TotalTopUps { get; set; }
        public decimal PendingCash { get; set; }

        // Supporting data for calculations
        public decimal TotalSettled { get; set; }
        public int PendingRequestCount { get; set; }
        public decimal PendingRequested { get; set; }

        // Recent Top-ups and Settlements
        public List<ViewModels.RecentTopUpViewModel> RecentTopUps { get; set; } = new();

        // Financial Consistency Check
        public bool IsFinanciallyConsistent { get; set; } = true;

        // Computed property for available amount for settlement request
        public decimal AvailableForRequest => TotalCollected - TotalSettled - PendingRequested;

        // Computed property for consistency status
        public string ConsistencyStatus => IsFinanciallyConsistent ? "✓ In Balance" : "✗ Mismatch Detected";
        public string ConsistencyAlert => IsFinanciallyConsistent ? "financial-ok" : "financial-alert";
    }
}
