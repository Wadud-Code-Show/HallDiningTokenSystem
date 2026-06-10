namespace HallDiningTokenSystem.ViewModels
{
    public class ServingDashboardViewModel
    {
        public string DeliveryManId { get; set; } = string.Empty;
        public string DeliveryManName { get; set; } = string.Empty;
        public DateTime CurrentTime { get; set; }
        public List<MealServingItem> ServingMeals { get; set; } = new List<MealServingItem>();

        public class MealServingItem
        {
            public int MealId { get; set; }
            public string MealName { get; set; } = string.Empty;
            public DateTime ServingStart { get; set; }
            public DateTime ServingEnd { get; set; }
            public int TotalCapacity { get; set; }
            public int TotalSold { get; set; }
            public int RedeemedCount { get; set; }
            public double TimeRemaining { get; set; }

            // Helper properties
            public int RemainingToRedeem => TotalSold - RedeemedCount;
            public int PercentRedeemed => TotalSold > 0 ? (int)((RedeemedCount * 100) / TotalSold) : 0;
            public bool IsUrgent => TimeRemaining < 15; // Less than 15 minutes remaining
            public string StatusClass => IsUrgent ? "text-danger" : "text-success";
            public string ProgressBarClass => IsUrgent ? "bg-danger" : "bg-success";

            public string GetTimeRemainingDisplay()
            {
                if (TimeRemaining < 0) return "Serving ended";
                if (TimeRemaining < 1) return "< 1 minute";
                if (TimeRemaining < 60) return $"{(int)TimeRemaining} minutes";
                var hours = TimeRemaining / 60;
                return $"{(int)hours}h {(int)(TimeRemaining % 60)}m";
            }
        }
    }
}
