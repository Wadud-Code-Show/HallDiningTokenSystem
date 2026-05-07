namespace HallDiningTokenSystem.Models
{
    public class CashSettlementRequestViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? RequestNote { get; set; }
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; } = "Pending";
        public string? AdminNote { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    public class SettlementRequestItemViewModel
    {
        public int Id { get; set; }
        public string CashAdminName { get; set; } = string.Empty;
        public string CashAdminEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? RequestNote { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class RequestSettlementInputViewModel
    {
        public decimal Amount { get; set; }
        public string? RequestNote { get; set; }
    }
}
