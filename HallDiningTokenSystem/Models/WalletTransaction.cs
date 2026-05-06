namespace HallDiningTokenSystem.Models
{
    public class WalletTransaction
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public Wallet Wallet { get; set; } = null!;
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = null!; // Credit or Debit
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
