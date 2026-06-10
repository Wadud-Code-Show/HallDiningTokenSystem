namespace HallDiningTokenSystem.ViewModels
{
    public class WalletHistoryViewModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public List<TransactionItem> Transactions { get; set; } = new List<TransactionItem>();

        public class TransactionItem
        {
            public int TransactionId { get; set; }
            public DateTime Date { get; set; }
            public string Type { get; set; } = string.Empty; // Credit or Debit
            public decimal Amount { get; set; }
            public string Description { get; set; } = string.Empty;
            public decimal RunningBalance { get; set; }

            // Helper to get badge CSS class
            public string GetTypeBadgeClass()
            {
                return Type switch
                {
                    "Credit" => "bg-success",
                    "Debit" => "bg-danger",
                    _ => "bg-secondary"
                };
            }

            // Helper to get formatted amount with sign
            public string GetFormattedAmount()
            {
                return Type == "Credit" ? $"+৳{Amount:N2}" : $"-৳{Amount:N2}";
            }
        }
    }
}
