namespace HallDiningTokenSystem.Models
{
    public class MealSession
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // e.g., "Breakfast", "Lunch", "Dinner"
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchived { get; set; } = false; // Soft archive - meal is hidden but data preserved
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
