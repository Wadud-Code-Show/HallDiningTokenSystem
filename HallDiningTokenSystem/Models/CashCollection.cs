namespace HallDiningTokenSystem.Models
{
    public class CashCollection
    {
        public int Id { get; set; }
        public string CashAdminId { get; set; } = null!;
        public ApplicationUser CashAdmin { get; set; } = null!;
        public string StudentId { get; set; } = null!;
        public ApplicationUser Student { get; set; } = null!;
        public decimal Amount { get; set; }
        public int? WalletTransactionId { get; set; }
        public WalletTransaction? WalletTransaction { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
