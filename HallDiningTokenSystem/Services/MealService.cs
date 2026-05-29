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
                // Validate date sequence
                if (model.TokenSellStartDateTime >= model.TokenSellEndDateTime)
                {
                    return ServiceResult.ErrorResult("Token sale end must be after token sale start.");
                }

                if (model.TokenSellEndDateTime >= model.MealServingStartDateTime)
                {
                    return ServiceResult.ErrorResult("Meal serving start must be after token sale end.");
                }

                if (model.MealServingStartDateTime >= model.MealServingEndDateTime)
                {
                    return ServiceResult.ErrorResult("Meal serving end must be after meal serving start.");
                }

                // Validate that TokenSellStartDateTime is not in the past
                if (model.TokenSellStartDateTime < DateTime.Now)
                {
                    return ServiceResult.ErrorResult("Token sale start cannot be in the past.");
                }

                var mealSession = new MealSession
                {
                    Name = model.MealName,
                    Description = model.Description,
                    Price = model.Price,
                    Capacity = model.Capacity,
                    TokenSellStartDateTime = model.TokenSellStartDateTime,
                    TokenSellEndDateTime = model.TokenSellEndDateTime,
                    MealServingStartDateTime = model.MealServingStartDateTime,
                    MealServingEndDateTime = model.MealServingEndDateTime,
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
