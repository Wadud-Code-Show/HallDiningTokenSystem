namespace HallDiningTokenSystem.ViewModels
{
    public class ReportDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalMeals { get; set; }
        public int TokensSold { get; set; }
        public int TokensRedeemed { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CurrentWalletBalance { get; set; }
        public DateTime GeneratedAt { get; set; }

        // Computed properties for display
        public int ActiveStudents => TotalStudents;
        public double RedemptionRate => TokensSold > 0 ? (double)TokensRedeemed / TokensSold * 100 : 0;
        public decimal AverageRevenuePerMeal => TotalMeals > 0 ? TotalRevenue / TotalMeals : 0;
    }

    public class MealReportViewModel
    {
        public List<MealItem> Meals { get; set; } = new();
        public int TotalMeals { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime GeneratedAt { get; set; }

        public class MealItem
        {
            public int MealId { get; set; }
            public string MealName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Capacity { get; set; }
            public int SoldCount { get; set; }
            public int RemainingCapacity { get; set; }
            public int RedeemedCount { get; set; }
            public decimal Revenue { get; set; }
            public DateTime TokenSaleStart { get; set; }
            public DateTime TokenSaleEnd { get; set; }
            public DateTime ServingStart { get; set; }
            public DateTime ServingEnd { get; set; }
            public bool IsArchived { get; set; }

            // Computed properties
            public double RedemptionRate => SoldCount > 0 ? (double)RedeemedCount / SoldCount * 100 : 0;
            public double CapacityUtilization => Capacity > 0 ? (double)SoldCount / Capacity * 100 : 0;
            public string StatusBadgeClass => Status switch
            {
                "Archived" => "bg-secondary",
                "Upcoming" => "bg-info",
                "Sale Open" => "bg-success",
                "Ready for Service" => "bg-warning",
                "Serving" => "bg-primary",
                "Completed" => "bg-dark",
                _ => "bg-light"
            };
        }
    }

    public class MealDetailsReportViewModel
    {
        public int MealId { get; set; }
        public string MealName { get; set; } = string.Empty;
        public string? MealDescription { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public int TotalSold { get; set; }
        public int TotalRedeemed { get; set; }
        public decimal TotalRevenue { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime TokenSaleStart { get; set; }
        public DateTime TokenSaleEnd { get; set; }
        public DateTime ServingStart { get; set; }
        public DateTime ServingEnd { get; set; }
        public List<StudentParticipationItem> StudentParticipation { get; set; } = new();
        public DateTime GeneratedAt { get; set; }

        // Computed properties
        public int RemainingCapacity => Capacity - TotalSold;
        public double RedemptionRate => TotalSold > 0 ? (double)TotalRedeemed / TotalSold * 100 : 0;
        public double CapacityUtilization => Capacity > 0 ? (double)TotalSold / Capacity * 100 : 0;

        public class StudentParticipationItem
        {
            public string StudentName { get; set; } = string.Empty;
            public string StudentId { get; set; } = string.Empty;
            public DateTime PurchaseDate { get; set; }
            public bool IsRedeemed { get; set; }
            public DateTime? RedeemedTime { get; set; }

            public string RedeemedStatus => IsRedeemed ? "Yes" : "No";
            public string RedemptionBadgeClass => IsRedeemed ? "bg-success" : "bg-warning";
        }
    }

    public class StudentReportViewModel
    {
        public List<StudentItem> Students { get; set; } = new();
        public int TotalStudents { get; set; }
        public decimal TotalWalletBalance { get; set; }
        public decimal TotalSpending { get; set; }
        public DateTime GeneratedAt { get; set; }

        public class StudentItem
        {
            public string UserId { get; set; } = string.Empty;
            public string StudentName { get; set; } = string.Empty;
            public string StudentId { get; set; } = string.Empty;
            public decimal CurrentWalletBalance { get; set; }
            public int PurchasedTokensCount { get; set; }
            public int RedeemedTokensCount { get; set; }
            public decimal TotalSpending { get; set; }

            // Computed properties
            public double RedemptionRate => PurchasedTokensCount > 0 ? (double)RedeemedTokensCount / PurchasedTokensCount * 100 : 0;
            public int UnredeemedTokens => PurchasedTokensCount - RedeemedTokensCount;
        }
    }

    public class StudentDetailsReportViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string? StudentEmail { get; set; }
        public decimal CurrentWalletBalance { get; set; }
        public decimal TotalTopups { get; set; }
        public decimal TotalSpending { get; set; }
        public List<PurchaseItem> PurchaseHistory { get; set; } = new();
        public List<RedemptionItem> RedemptionHistory { get; set; } = new();
        public List<WalletTransactionItem> WalletTransactions { get; set; } = new();
        public DateTime GeneratedAt { get; set; }

        // Computed properties
        public int TotalPurchases => PurchaseHistory.Count;
        public int TotalRedemptions => RedemptionHistory.Count;
        public double RedemptionRate => TotalPurchases > 0 ? (double)TotalRedemptions / TotalPurchases * 100 : 0;

        public class PurchaseItem
        {
            public string MealName { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public DateTime PurchaseDate { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        public class RedemptionItem
        {
            public string MealName { get; set; } = string.Empty;
            public string OtpCode { get; set; } = string.Empty;
            public DateTime RedeemedAt { get; set; }
            public string RedeemedBy { get; set; } = string.Empty;
        }

        public class WalletTransactionItem
        {
            public DateTime TransactionDate { get; set; }
            public string Type { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public decimal BalanceAfter { get; set; }

            public string TypeBadgeClass => Type switch
            {
                "Credit" => "bg-success",
                "Debit" => "bg-warning",
                _ => "bg-light"
            };
        }
    }

    public class RedemptionReportViewModel
    {
        public List<RedemptionItem> Redemptions { get; set; } = new();
        public int TotalRedemptions { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? SelectedMealId { get; set; }
        public string? SelectedStudentId { get; set; }
        public string? SelectedDeliveryManId { get; set; }
        public List<dynamic> AvailableMeals { get; set; } = new();
        public List<dynamic> AvailableDeliveryMen { get; set; } = new();
        public DateTime GeneratedAt { get; set; }

        public class RedemptionItem
        {
            public string MealName { get; set; } = string.Empty;
            public string StudentName { get; set; } = string.Empty;
            public string StudentId { get; set; } = string.Empty;
            public string OtpCode { get; set; } = string.Empty;
            public string RedeemedBy { get; set; } = string.Empty;
            public DateTime RedeemedAt { get; set; }

            public string TimeAgo
            {
                get
                {
                    var timeSpan = DateTime.Now - RedeemedAt;
                    if (timeSpan.TotalSeconds < 60) return "Just now";
                    if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
                    if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
                    return $"{(int)timeSpan.TotalDays}d ago";
                }
            }
        }
    }

    public class WalletReportViewModel
    {
        public List<WalletItem> Wallets { get; set; } = new();
        public int TotalWallets { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal TotalTopups { get; set; }
        public decimal TotalSpending { get; set; }
        public DateTime GeneratedAt { get; set; }

        public decimal AverageBalance => TotalWallets > 0 ? TotalBalance / TotalWallets : 0;

        public class WalletItem
        {
            public string StudentName { get; set; } = string.Empty;
            public string StudentId { get; set; } = string.Empty;
            public decimal CurrentBalance { get; set; }
            public decimal TotalTopups { get; set; }
            public decimal TotalSpending { get; set; }
            public decimal NetPosition { get; set; }

            public string BalanceStatus => CurrentBalance switch
            {
                < 0 => "Negative",
                0 => "Zero",
                > 0 => "Positive"
            };

            public string BalanceStatusColor => CurrentBalance switch
            {
                < 0 => "text-danger",
                0 => "text-warning",
                _ => "text-success"
            };
        }
    }

    public class SettlementReportViewModel
    {
        public List<SettlementItem> Settlements { get; set; } = new();
        public int TotalSettlements { get; set; }
        public decimal TotalAmountSettled { get; set; }
        public DateTime GeneratedAt { get; set; }

        public class SettlementItem
        {
            public string CashAdminName { get; set; } = string.Empty;
            public DateTime CollectionDate { get; set; }
            public decimal Amount { get; set; }
            public string SettlementStatus { get; set; } = string.Empty;
            public DateTime? SettlementDate { get; set; }
            public string ReceivedByAdminName { get; set; } = string.Empty;
            public string? SettlementNotes { get; set; }

            public string StatusBadgeClass => SettlementStatus switch
            {
                "Approved" => "bg-success",
                "Rejected" => "bg-danger",
                "Pending" => "bg-warning",
                "Completed" => "bg-primary",
                _ => "bg-light"
            };

            public string FormattedDate => CollectionDate.ToString("dd/MM/yyyy HH:mm");
        }
    }
}
