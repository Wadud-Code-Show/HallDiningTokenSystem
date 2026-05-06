namespace HallDiningTokenSystem.Models
{
    public class CashSettlement
    {
        public int Id { get; set; }
        public string CashAdminId { get; set; } = null!;
        public ApplicationUser CashAdmin { get; set; } = null!;
        public decimal Amount { get; set; }
        public string ReceivedByAdminId { get; set; } = null!;
        public ApplicationUser ReceivedByAdmin { get; set; } = null!;
        public string? Note { get; set; }
        public DateTime SettlementDate { get; set; }
    }
}
