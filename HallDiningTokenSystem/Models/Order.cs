namespace HallDiningTokenSystem.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public int MealSessionId { get; set; }
        public MealSession MealSession { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled
        public DateTime OrderDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation property
        public ICollection<MealToken> MealTokens { get; set; } = new List<MealToken>();
    }
}
