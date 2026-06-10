namespace HallDiningTokenSystem.ViewModels
{
    public class ServingHistoryViewModel
    {
        public string DeliveryManId { get; set; } = string.Empty;
        public string DeliveryManName { get; set; } = string.Empty;
        public DateTime FilterDate { get; set; }
        public List<RedemptionItem> Redemptions { get; set; } = new List<RedemptionItem>();
        public int TotalRedemptions { get; set; }

        public class RedemptionItem
        {
            public int TokenId { get; set; }
            public int OrderId { get; set; }
            public string OtpCode { get; set; } = string.Empty;
            public string StudentName { get; set; } = string.Empty;
            public string StudentId { get; set; } = string.Empty;
            public string MealName { get; set; } = string.Empty;
            public DateTime RedeemedAt { get; set; }

            // Helper properties
            public string FormattedTime => RedeemedAt.ToString("hh:mm tt");
            public string FormattedDate => RedeemedAt.ToString("dd MMM yyyy");
            public string TimeAgo
            {
                get
                {
                    var span = DateTime.Now - RedeemedAt;
                    if (span.TotalMinutes < 1)
                        return "Just now";
                    if (span.TotalMinutes < 60)
                        return $"{(int)span.TotalMinutes}m ago";
                    if (span.TotalHours < 24)
                        return $"{(int)span.TotalHours}h ago";
                    return span.Days + "d ago";
                }
            }
        }
    }
}
