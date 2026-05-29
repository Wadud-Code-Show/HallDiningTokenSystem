namespace HallDiningTokenSystem.Models
{
    public class MealSession
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // e.g., "Breakfast", "Lunch", "Dinner"
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }

        // Token Purchase Phase
        public DateTime TokenSellStartDateTime { get; set; }
        public DateTime TokenSellEndDateTime { get; set; }

        // Meal Serving Phase
        public DateTime MealServingStartDateTime { get; set; }
        public DateTime MealServingEndDateTime { get; set; }

        // Archive flag and audit
        public bool IsArchived { get; set; } = false; // Soft archive - meal is hidden but data preserved
        public DateTime CreatedAt { get; set; }

        // Computed property for sold count (from Orders collection)
        public int SoldCount { get; set; } = 0;

        // Navigation property
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
