using HallDiningTokenSystem.Data;
using HallDiningTokenSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HallDiningTokenSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Student Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Get wallet balance
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == currentUser.Id);

            var currentBalance = wallet?.Balance ?? 0m;

            // Count Upcoming and Sale Open meals
            var upcomingMealsCount = await _context.MealSessions
                .Where(m => !m.IsArchived && DateTime.Now < m.TokenSellStartDateTime)
                .CountAsync();

            var saleOpenMealsCount = await _context.MealSessions
                .Where(m => !m.IsArchived && DateTime.Now >= m.TokenSellStartDateTime && DateTime.Now <= m.TokenSellEndDateTime)
                .CountAsync();

            // Build dashboard view model
            var dashboard = new StudentDashboardViewModel
            {
                CurrentBalance = currentBalance,
                TotalPurchasedTokens = 0, // Token system not yet implemented
                TotalRedeemedTokens = 0,  // Token system not yet implemented
                UpcomingMealsCount = upcomingMealsCount,
                ActiveMealsCount = saleOpenMealsCount,
                StudentId = currentUser.StudentId ?? "N/A",
                StudentName = currentUser.FullName ?? currentUser.UserName ?? "Student"
            };

            return View(dashboard);
        }

        // Available Meals - Students can only see Upcoming and Sale Open meals
        public async Task<IActionResult> Meals()
        {
            var meals = await _context.MealSessions
                .Include(m => m.Orders)
                .Where(m => !m.IsArchived && DateTime.Now <= m.MealServingEndDateTime)
                .OrderBy(m => m.TokenSellStartDateTime)
                .ToListAsync();

            // Map to StudentMealViewModel
            var mealViewModels = meals.Select(m => new StudentMealViewModel
            {
                Id = m.Id,
                MealName = m.Name,
                Price = m.Price,
                TokenSellStartDateTime = m.TokenSellStartDateTime,
                TokenSellEndDateTime = m.TokenSellEndDateTime,
                MealServingStartDateTime = m.MealServingStartDateTime,
                MealServingEndDateTime = m.MealServingEndDateTime,
                Capacity = m.Capacity,
                SoldCount = m.SoldCount
            }).ToList();

            return View(mealViewModels);
        }

        // Meal Details - Students can only view operational meals
        [HttpGet]
        public async Task<IActionResult> MealDetails(int id)
        {
            var meal = await _context.MealSessions
                .Include(m => m.Orders)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meal == null)
            {
                return NotFound();
            }

            // Get the meal status
            var status = MealDetailsViewModel.GetMealStatus(
                meal.TokenSellStartDateTime,
                meal.TokenSellEndDateTime,
                meal.MealServingStartDateTime,
                meal.MealServingEndDateTime,
                meal.IsArchived);

            // Ensure meal is operational (not archived and not completed)
            if (meal.IsArchived || status == "Completed")
            {
                return NotFound();
            }

            // Map to StudentMealDetailsViewModel
            var viewModel = new StudentMealDetailsViewModel
            {
                Id = meal.Id,
                MealName = meal.Name,
                Price = meal.Price,
                TokenSellStartDateTime = meal.TokenSellStartDateTime,
                TokenSellEndDateTime = meal.TokenSellEndDateTime,
                MealServingStartDateTime = meal.MealServingStartDateTime,
                MealServingEndDateTime = meal.MealServingEndDateTime,
                Capacity = meal.Capacity,
                SoldCount = meal.SoldCount
            };

            return View(viewModel);
        }
    }
}
