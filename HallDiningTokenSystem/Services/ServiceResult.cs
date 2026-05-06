namespace HallDiningTokenSystem.Services
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public static ServiceResult SuccessResult(string message = "Operation completed successfully.", object? data = null)
        {
            return new ServiceResult
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ServiceResult ErrorResult(string message = "An error occurred.")
        {
            return new ServiceResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
