namespace HallDiningTokenSystem.ViewModels
{
    public class MyTokensViewModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public List<TokenItem> Tokens { get; set; } = new List<TokenItem>();

        public class TokenItem
        {
            public int TokenId { get; set; }
            public string MealName { get; set; } = string.Empty;
            public string OtpCode { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty; // Active, Redeemed, Expired
            public DateTime PurchaseDate { get; set; }
            public DateTime MealServingStart { get; set; }
            public DateTime MealServingEnd { get; set; }
            public DateTime? ExpiresAt { get; set; }
            public DateTime CreatedAt { get; set; }

            // Helper to determine if token is expired
            public bool IsExpired => DateTime.Now > (ExpiresAt ?? DateTime.MaxValue);

            // Helper to get badge CSS class
            public string GetStatusBadgeClass()
            {
                return Status switch
                {
                    "Active" => "bg-success",
                    "Redeemed" => "bg-primary",
                    "Expired" => "bg-danger",
                    _ => "bg-secondary"
                };
            }

            // Helper to get status display
            public string GetStatusDisplay()
            {
                if (Status == "Active" && IsExpired)
                    return "Expired";
                return Status;
            }
        }
    }
}
