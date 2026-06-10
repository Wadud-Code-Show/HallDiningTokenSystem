namespace HallDiningTokenSystem.ViewModels
{
    // ========== SuperAdmin Settlement Review ViewModels ==========

    /// <summary>
    /// Display a pending settlement request for SuperAdmin review
    /// </summary>
    public class PendingSettlementRequestViewModel
    {
        public int Id { get; set; }
        public string CashAdminId { get; set; } = string.Empty;
        public string CashAdminName { get; set; } = string.Empty;
        public string CashAdminEmail { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? RequestNote { get; set; }
        public decimal TotalCollectedByAdmin { get; set; }
        public int CollectionCount { get; set; }
        public decimal TotalSettledByAdmin { get; set; }

        // Computed
        public string TimeAgoRequested => GetTimeAgo(RequestedAt);
        public string StatusBadge => "bg-warning"; // Pending

        private string GetTimeAgo(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.TotalSeconds < 60) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 30) return $"{(int)span.TotalDays}d ago";
            return dt.ToString("MMM dd, yyyy");
        }
    }

    /// <summary>
    /// List of pending settlement requests for SuperAdmin review page
    /// </summary>
    public class PendingSettlementRequestsListViewModel
    {
        public List<PendingSettlementRequestViewModel> Requests { get; set; } = new();
        public int TotalPendingCount { get; set; }
        public decimal TotalPendingAmount { get; set; }
        public DateTime? AsOfDate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Model for approving a settlement request
    /// </summary>
    public class ApprovSettlementRequestViewModel
    {
        public int RequestId { get; set; }
        public string CashAdminName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? RequestNote { get; set; }
        public string? ApprovalNotes { get; set; }
    }

    /// <summary>
    /// Model for rejecting a settlement request
    /// </summary>
    public class RejectSettlementRequestViewModel
    {
        public int RequestId { get; set; }
        public string CashAdminName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? RequestNote { get; set; }
        public string RejectionReason { get; set; } = string.Empty; // Required
    }

    /// <summary>
    /// Display detailed settlement request with full audit trail
    /// </summary>
    public class SettlementRequestAuditViewModel
    {
        // Request Details
        public int RequestId { get; set; }
        public string CashAdminName { get; set; } = string.Empty;
        public string CashAdminEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string? RequestNote { get; set; }

        // Audit Trail
        public string? ReviewedByAdminName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? AdminNote { get; set; }

        // Related Settlement
        public int? LinkedSettlementId { get; set; }

        // Computed
        public string StatusBadge => Status switch
        {
            "Pending" => "bg-warning",
            "Approved" => "bg-success",
            "Rejected" => "bg-danger",
            _ => "bg-secondary"
        };

        public bool CanApprove => Status == "Pending";
        public bool CanReject => Status == "Pending";
    }

    /// <summary>
    /// Display a settlement record with full details
    /// </summary>
    public class SettlementRecordViewModel
    {
        public int Id { get; set; }
        public string CashAdminName { get; set; } = string.Empty;
        public DateTime SettlementDate { get; set; }
        public decimal Amount { get; set; }
        public string ReceivedByAdminName { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }

        public string TimeAgoSettled => GetTimeAgo(SettlementDate);

        private string GetTimeAgo(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.TotalSeconds < 60) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 30) return $"{(int)span.TotalDays}d ago";
            return dt.ToString("MMM dd, yyyy");
        }
    }

    /// <summary>
    /// Settlement history with filtering for SuperAdmin
    /// </summary>
    public class SettlementHistoryViewModel
    {
        public List<SettlementRecordViewModel> Settlements { get; set; } = new();
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }

        // Filter metadata
        public string? FilterCashAdminId { get; set; }
        public DateTime? FilterFromDate { get; set; }
        public DateTime? FilterToDate { get; set; }

        // Available filter options
        public List<CashAdminOption> AvailableCashAdmins { get; set; } = new();
    }

    public class CashAdminOption
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    // ========== Financial Consistency Dashboard ==========

    /// <summary>
    /// System-wide cash settlement audit dashboard
    /// Shows total collected = settled + pending for all CashAdmins
    /// </summary>
    public class SettlementConsistencyDashboardViewModel
    {
        public List<CashAdminConsistencyItemViewModel> CashAdminSummaries { get; set; } = new();
        public decimal SystemTotalCollected { get; set; }
        public decimal SystemTotalSettled { get; set; }
        public decimal SystemTotalPending { get; set; }
        public int CashAdminCount { get; set; }
        public int AnomalyCount { get; set; }
        public bool AllConsistent => AnomalyCount == 0;
        public DateTime AsOfDate { get; set; } = DateTime.Now;

        // Computed property
        public bool IsBalanced => Math.Abs(SystemTotalCollected - (SystemTotalSettled + SystemTotalPending)) < 0.01m;
    }

    public class CashAdminConsistencyItemViewModel
    {
        public string CashAdminId { get; set; } = string.Empty;
        public string CashAdminName { get; set; } = string.Empty;
        public decimal TotalCollected { get; set; }
        public decimal TotalSettled { get; set; }
        public decimal PendingSettlement { get; set; }
        public int CollectionCount { get; set; }
        public int SettlementCount { get; set; }
        public bool IsConsistent { get; set; }

        // Computed
        public string ConsistencyStatus => IsConsistent ? "✓ OK" : "✗ MISMATCH";
        public string ConsistencyBadge => IsConsistent ? "bg-success" : "bg-danger";
    }

    // ========== CashAdmin Enhanced TopUp ViewModels ==========

    /// <summary>
    /// Enhanced TopUp wallet form with remarks
    /// </summary>
    public class EnhancedWalletTopUpViewModel
    {
        // Student search
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public string? StudentUserId { get; set; }
        public decimal CurrentBalance { get; set; }

        // TopUp details
        public decimal? Amount { get; set; }
        public string? Remarks { get; set; } // NEW: Capture reason for topup
        public string? Note { get; set; }

        // Display
        public decimal NewBalance { get; set; }
    }

    // ========== CashAdmin Settlement Request History ==========

    /// <summary>
    /// Display a settlement request in CashAdmin history
    /// </summary>
    public class SettlementRequestHistoryItemViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RequestNote { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedByAdminName { get; set; }
        public string? AdminNote { get; set; }

        // Computed
        public string StatusBadge => Status switch
        {
            "Pending" => "bg-warning",
            "Approved" => "bg-success",
            "Rejected" => "bg-danger",
            _ => "bg-secondary"
        };

        public string TimeAgoRequested => GetTimeAgo(RequestedAt);

        private string GetTimeAgo(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.TotalSeconds < 60) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 30) return $"{(int)span.TotalDays}d ago";
            return dt.ToString("MMM dd, yyyy");
        }
    }

    /// <summary>
    /// CashAdmin settlement request history page
    /// </summary>
    public class CashAdminSettlementHistoryViewModel
    {
        public List<SettlementRequestHistoryItemViewModel> Requests { get; set; } = new();
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public decimal TotalRequested { get; set; }
        public decimal TotalApproved { get; set; }
    }

    // ========== Enhanced CashAdmin Dashboard ==========

    /// <summary>
    /// Enhanced CashAdmin dashboard with financial consistency check
    /// </summary>
    public class EnhancedCashAdminDashboardViewModel
    {
        public decimal TotalCollected { get; set; }
        public decimal TodayCollected { get; set; }
        public decimal TotalSettled { get; set; }
        public decimal PendingSettlement { get; set; }
        public decimal PendingRequested { get; set; }
        public decimal AvailableForRequest { get; set; }
        public int TotalTopUps { get; set; }
        public int PendingRequestCount { get; set; }

        public List<RecentTopUpViewModel> RecentTopUps { get; set; } = new();
        public List<RecentSettlementViewModel> RecentSettlements { get; set; } = new();

        // Financial consistency check
        public bool IsConsistent => Math.Abs(TotalCollected - (TotalSettled + PendingSettlement)) < 0.01m;
        public string ConsistencyStatus => IsConsistent ? "✓ In Balance" : "✗ Mismatch Detected";
        public string ConsistencyAlert => IsConsistent ? "financial-ok" : "financial-alert";
    }

    public class RecentSettlementViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime SettlementDate { get; set; }
        public string ReceivedByAdminName { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class RecentTopUpViewModel
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
