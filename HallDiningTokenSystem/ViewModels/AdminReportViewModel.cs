namespace HallDiningTokenSystem.Models
{
    public class AdminReportViewModel
    {
        // Summary Metrics
        public int TotalStudents { get; set; }
        public int TotalCashAdmins { get; set; }
        public int TotalMeals { get; set; }
        public int TotalTokensSold { get; set; }
        public decimal TotalCashCollected { get; set; }
        public decimal TotalCashSettled { get; set; }
        public decimal PendingCash { get; set; }
        public int PendingSettlementRequests { get; set; }

        // Detail Collections
        public List<CashAdminSummaryViewModel> CashAdminSummary { get; set; } = new();
        public List<RecentCashCollectionViewModel> RecentCollections { get; set; } = new();
        public List<RecentSettlementRequestViewModel> RecentSettlementRequests { get; set; } = new();
        public List<RecentSettlementViewModel> RecentSettlements { get; set; } = new();
        public List<MealSummaryViewModel> MealSummary { get; set; } = new();
    }

    public class CashAdminSummaryViewModel
    {
        public string CashAdminName { get; set; } = string.Empty;
        public string CashAdminEmail { get; set; } = string.Empty;
        public decimal TotalCollected { get; set; }
        public decimal TotalSettled { get; set; }
        public decimal PendingCash { get; set; }
        public decimal PendingRequestsAmount { get; set; }
    }

    public class RecentCashCollectionViewModel
    {
        public DateTime CreatedAt { get; set; }
        public string CashAdminName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class RecentSettlementRequestViewModel
    {
        public DateTime RequestedAt { get; set; }
        public string CashAdminName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class RecentSettlementViewModel
    {
        public DateTime SettlementDate { get; set; }
        public string CashAdminName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string ReceivedByAdminName { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class MealSummaryViewModel
    {
        public string MealName { get; set; } = string.Empty;
        public string MealType { get; set; } = string.Empty;
        public DateTime MealDate { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public int SoldCount { get; set; }
    }
}
