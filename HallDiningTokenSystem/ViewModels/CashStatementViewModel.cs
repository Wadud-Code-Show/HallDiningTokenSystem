namespace HallDiningTokenSystem.ViewModels
{
    public class CashStatementViewModel
    {
        public decimal TotalCollected { get; set; }
        public decimal TotalSettled { get; set; }
        public decimal PendingRequested { get; set; }
        public decimal AvailablePendingCash { get; set; }
        public decimal PendingCash { get; set; }
        public int TotalTopUps { get; set; }

        public List<CashCollectionItemViewModel> RecentCollections { get; set; } = new();
        public List<SettlementHistoryItemViewModel> SettlementHistory { get; set; } = new();
        public List<SettlementRequestHistoryItemViewModel> SettlementRequests { get; set; } = new();
    }

    public class CashCollectionItemViewModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }

    public class SettlementHistoryItemViewModel
    {
        public int Id { get; set; }
        public DateTime SettlementDate { get; set; }
        public decimal Amount { get; set; }
        public string ReceivedByAdminName { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
