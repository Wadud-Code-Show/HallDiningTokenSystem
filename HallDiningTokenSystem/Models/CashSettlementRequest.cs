namespace HallDiningTokenSystem.Models
{
    public class CashSettlementRequest
    {
        public int Id { get; set; }

        // CashAdmin who requested settlement
        public string CashAdminId { get; set; } = null!;
        public ApplicationUser CashAdmin { get; set; } = null!;

        // Amount requested
        public decimal Amount { get; set; }

        // Request status: Pending, Approved, Rejected
        public string Status { get; set; } = "Pending";

        // CashAdmin's note for the request
        public string? RequestNote { get; set; }

        // When was this requested
        public DateTime RequestedAt { get; set; } = DateTime.Now;

        // SuperAdmin who reviewed this request
        public string? ReviewedByAdminId { get; set; }
        public ApplicationUser? ReviewedByAdmin { get; set; }

        // When was this reviewed
        public DateTime? ReviewedAt { get; set; }

        // SuperAdmin's note on approval/rejection
        public string? AdminNote { get; set; }

        // Link to the settlement created from this request (if approved)
        public int? CashSettlementId { get; set; }
        public CashSettlement? CashSettlement { get; set; }
    }
}
