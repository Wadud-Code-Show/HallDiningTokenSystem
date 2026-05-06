using HallDiningTokenSystem.Data;
using HallDiningTokenSystem.Models;

namespace HallDiningTokenSystem.Services
{
    public interface IMealService
    {
        Task<ServiceResult> CreateMealAsync(CreateMealViewModel model);
        Task<List<MealSession>> GetAllMealsAsync();
        Task<MealSession?> GetMealByIdAsync(int id);
    }

    public class MealService : IMealService
    {
        private readonly ApplicationDbContext _context;

        public MealService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> CreateMealAsync(CreateMealViewModel model)
        {
            try
            {
                // Validate that SaleEndTime is in the future
                if (model.SaleEndTime <= DateTime.Now)
                {
                    return ServiceResult.ErrorResult("Sale end time must be in the future.");
                }

                // Validate that MealDate is today or in the future
                if (model.MealDate.Date < DateTime.Now.Date)
                {
                    return ServiceResult.ErrorResult("Meal date cannot be in the past.");
                }

                var mealSession = new MealSession
                {
                    Name = model.MealName,
                    Description = model.Description,
                    Price = model.Price,
                    StartTime = model.MealDate,
                    EndTime = model.MealDate.AddHours(3), // Default 3-hour window
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.MealSessions.Add(mealSession);
                await _context.SaveChangesAsync();

                return ServiceResult.SuccessResult($"Meal '{model.MealName}' created successfully.", mealSession);
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Error creating meal: {ex.Message}");
            }
        }

        public async Task<List<MealSession>> GetAllMealsAsync()
        {
            return await Task.FromResult(_context.MealSessions.OrderByDescending(m => m.CreatedAt).ToList());
        }

        public async Task<MealSession?> GetMealByIdAsync(int id)
        {
            return await Task.FromResult(_context.MealSessions.FirstOrDefault(m => m.Id == id));
        }
    }
}
