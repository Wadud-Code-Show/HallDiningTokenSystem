namespace HallDiningTokenSystem.Models
{
    public class CashAdminReportViewModel
    {
        // Summary Metrics
        public int TotalTopUps { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TodayCollected { get; set; }
        public int TotalStudentsServed { get; set; }

        // Detailed Report Data
        public List<CashCollectionDetailViewModel> CollectionDetails { get; set; } = new();
    }

    public class CashCollectionDetailViewModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StudentName { get; set; } = null!;
        public string StudentId { get; set; } = null!;
        public string StudentEmail { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}
