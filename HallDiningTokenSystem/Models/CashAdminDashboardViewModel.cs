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

        // Recent Top-ups
        public List<RecentTopUpViewModel> RecentTopUps { get; set; } = new();
    }

    public class RecentTopUpViewModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StudentName { get; set; } = null!;
        public string StudentId { get; set; } = null!;
        public decimal Amount { get; set; }
    }
}
