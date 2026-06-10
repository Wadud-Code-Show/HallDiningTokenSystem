namespace HallDiningTokenSystem.Models
{
    public class MealToken
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
        public string Status { get; set; } = "Active"; // Active, Redeemed, Expired
        public DateTime CreatedAt { get; set; }
        public DateTime? RedeemedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? RedeemedById { get; set; } // DeliveryMan who redeemed the token
        public ApplicationUser? RedeemedBy { get; set; }
    }
}
