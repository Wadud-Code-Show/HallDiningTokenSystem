using HallDiningTokenSystem.Models;

namespace HallDiningTokenSystem.Services
{
    /// <summary>
    /// Service for managing cash settlement workflow:
    /// - Calculate pending settlements
    /// - Create settlement requests
    /// - Approve/reject settlements with audit trail
    /// - Query settlement history
    /// </summary>
    public interface ISettlementService
    {
        /// <summary>
        /// Calculate total cash collected for a CashAdmin
        /// </summary>
        Task<decimal> GetTotalCollectedAsync(string cashAdminId);

        /// <summary>
        /// Calculate total settled amount for a CashAdmin
        /// </summary>
        Task<decimal> GetTotalSettledAsync(string cashAdminId);

        /// <summary>
        /// Calculate pending settlement amount (collected - settled - pending requests)
        /// </summary>
        Task<decimal> GetPendingSettlementAsync(string cashAdminId);

        /// <summary>
        /// Get collection count for a CashAdmin
        /// </summary>
        Task<int> GetCollectionCountAsync(string cashAdminId);

        /// <summary>
        /// Create a settlement request with validation
        /// </summary>
        Task<ServiceResult> CreateSettlementRequestAsync(string cashAdminId, decimal amount, string? requestNote);

        /// <summary>
        /// Approve a settlement request (creates CashSettlement, updates request status, creates audit trail)
        /// Uses EF Core transaction for atomicity
        /// </summary>
        Task<ServiceResult> ApproveSettlementRequestAsync(int requestId, string reviewedByAdminId, string? approvalNotes);

        /// <summary>
        /// Reject a settlement request with mandatory reason
        /// </summary>
        Task<ServiceResult> RejectSettlementRequestAsync(int requestId, string reviewedByAdminId, string rejectionReason);

        /// <summary>
        /// Get pending requests for review by SuperAdmin
        /// </summary>
        Task<List<CashSettlementRequest>> GetPendingRequestsAsync();

        /// <summary>
        /// Get settlement history for a CashAdmin with optional filters
        /// </summary>
        Task<List<CashSettlement>> GetSettlementHistoryAsync(
            string? cashAdminId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        /// <summary>
        /// Get settlement requests for a CashAdmin with optional status filter
        /// </summary>
        Task<List<CashSettlementRequest>> GetSettlementRequestsAsync(
            string cashAdminId,
            string? status = null);

        /// <summary>
        /// Get request details with full audit information
        /// </summary>
        Task<CashSettlementRequest?> GetRequestDetailsAsync(int requestId);

        /// <summary>
        /// Get settlement details with full audit information
        /// </summary>
        Task<CashSettlement?> GetSettlementDetailsAsync(int settlementId);

        /// <summary>
        /// Verify financial consistency: TotalCollected = TotalSettled + PendingSettlement
        /// Returns true if formula holds for given CashAdmin
        /// </summary>
        Task<bool> VerifyFinancialConsistencyAsync(string cashAdminId);

        /// <summary>
        /// Get all CashAdmins with their collection/settlement summary for system-wide audit
        /// </summary>
        Task<List<CashAdminSettlementSummary>> GetCashAdminSummariesAsync();
    }

    /// <summary>
    /// Summary for system-wide cash admin audit
    /// </summary>
    public class CashAdminSettlementSummary
    {
        public string CashAdminId { get; set; } = string.Empty;
        public string CashAdminName { get; set; } = string.Empty;
        public decimal TotalCollected { get; set; }
        public decimal TotalSettled { get; set; }
        public decimal PendingSettlement { get; set; }
        public int CollectionCount { get; set; }
        public int SettlementCount { get; set; }
        public bool IsConsistent { get; set; }
    }
}
