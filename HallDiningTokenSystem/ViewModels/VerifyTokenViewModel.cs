namespace HallDiningTokenSystem.ViewModels
{
    public class VerifyTokenViewModel
    {
        public string? OtpCode { get; set; }
        public int ActiveServingMeals { get; set; }
    }

    public class TokenVerificationResultViewModel
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string OtpEntered { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string MealName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public DateTime RedeemedAt { get; set; }
        public string TokenStatus { get; set; } = string.Empty;

        // Helper properties
        public string ResultBadgeClass => IsSuccess ? "bg-success" : "bg-danger";
        public string ResultMessage => IsSuccess ? "✓ Token Redeemed Successfully" : $"✗ {ErrorMessage}";
        public string ResultIcon => IsSuccess ? "bi-check-circle" : "bi-exclamation-circle";
    }
}
