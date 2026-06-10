using HallDiningTokenSystem.Data;
using HallDiningTokenSystem.Models;
using HallDiningTokenSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HallDiningTokenSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly IMealService _mealService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(IMealService mealService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _mealService = mealService;
            _context = context;
            _userManager = userManager;
        }

        // Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // Create Meal GET
        public IActionResult CreateMeal()
        {
            return View(new CreateMealViewModel());
        }

        // Create Meal POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMeal(CreateMealViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fix the validation errors.";
                return View(model);
            }

            var result = await _mealService.CreateMealAsync(model);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction("Meals");
            }
            else
            {
                TempData["Error"] = result.Message;
                return View(model);
            }
        }

        // Meals List - Operational meals (Upcoming + Sale Open + Sale Closed + Serving)
        public async Task<IActionResult> Meals()
        {
            var meals = await _context.MealSessions
                .Where(m => !m.IsArchived && DateTime.Now <= m.MealServingEndDateTime)
                .OrderBy(m => m.TokenSellStartDateTime)
                .ToListAsync();
            return View(meals);
        }

        // Meal History - Historical meals (Completed + Archived) with filtering
        public async Task<IActionResult> MealHistory(string? filter = "All")
        {
            // Validate filter parameter
            var validFilters = new[] { "All", "Completed", "Archived" };
            if (string.IsNullOrEmpty(filter) || !validFilters.Contains(filter))
            {
                filter = "All";
            }

            IQueryable<MealSession> query = _context.MealSessions;

            // Apply filter logic
            if (filter == "Completed")
            {
                // Completed: meal serving has ended, not archived
                query = query.Where(m => DateTime.Now > m.MealServingEndDateTime && !m.IsArchived);
            }
            else if (filter == "Archived")
            {
                // Archived: meal was manually archived before operation
                query = query.Where(m => m.IsArchived);
            }
            else // "All"
            {
                // All historical meals: either completed or archived
                query = query.Where(m => DateTime.Now > m.MealServingEndDateTime || m.IsArchived);
            }

            var meals = await query
                .OrderByDescending(m => m.TokenSellStartDateTime)
                .ToListAsync();

            // Pass filter to view via ViewData
            ViewData["CurrentFilter"] = filter;

            return View(meals);
        }

        // Edit Meal GET
        public async Task<IActionResult> EditMeal(int id)
        {
            // Find meal by id
            var meal = await _context.MealSessions
                .Include(m => m.Orders)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meal == null)
            {
                TempData["Error"] = "Meal not found.";
                return RedirectToAction(nameof(Meals));
            }

            // Check if meal serving has started
            if (DateTime.Now > meal.MealServingStartDateTime)
            {
                TempData["Error"] = "Meals cannot be edited after serving starts.";
                return RedirectToAction(nameof(Meals));
            }

            // Map to view model
            var viewModel = new EditMealViewModel
            {
                Id = meal.Id,
                MealName = meal.Name,
                Price = meal.Price,
                TokenSellStartDateTime = meal.TokenSellStartDateTime,
                TokenSellEndDateTime = meal.TokenSellEndDateTime,
                MealServingStartDateTime = meal.MealServingStartDateTime,
                MealServingEndDateTime = meal.MealServingEndDateTime,
                Capacity = meal.Capacity,
                SoldCount = meal.SoldCount,
                Description = meal.Description
            };

            return View(viewModel);
        }

        // Edit Meal POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMeal(EditMealViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fix the validation errors.";
                return View(model);
            }

            // Find meal by id
            var meal = await _context.MealSessions
                .Include(m => m.Orders)
                .FirstOrDefaultAsync(m => m.Id == model.Id);

            if (meal == null)
            {
                TempData["Error"] = "Meal not found.";
                return RedirectToAction(nameof(Meals));
            }

            // Check if meal serving has started
            if (DateTime.Now > meal.MealServingStartDateTime)
            {
                TempData["Error"] = "Meals cannot be edited after serving starts.";
                return RedirectToAction(nameof(Meals));
            }

            // Get sold count
            var soldCount = meal.SoldCount;

            // Validate date sequence
            if (model.TokenSellStartDateTime >= model.TokenSellEndDateTime)
            {
                ModelState.AddModelError("TokenSellEndDateTime", "Token sale end must be after token sale start.");
            }
            if (model.TokenSellEndDateTime >= model.MealServingStartDateTime)
            {
                ModelState.AddModelError("MealServingStartDateTime", "Meal serving start must be after token sale end.");
            }
            if (model.MealServingStartDateTime >= model.MealServingEndDateTime)
            {
                ModelState.AddModelError("MealServingEndDateTime", "Meal serving end must be after meal serving start.");
            }

            // Business Rule: If tokens sold, prevent price and token sale start changes
            if (soldCount > 0)
            {
                // Revert to original values if user tried to edit locked fields
                if (model.Price != meal.Price)
                {
                    ModelState.AddModelError("Price", "Price cannot be changed when tokens are already sold.");
                }
                if (model.TokenSellStartDateTime != meal.TokenSellStartDateTime)
                {
                    ModelState.AddModelError("TokenSellStartDateTime", "Token sale start cannot be changed when tokens are already sold.");
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Some fields are locked because tokens were already sold.";
                    return View(model);
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fix the validation errors.";
                return View(model);
            }

            // Update meal properties
            meal.Name = model.MealName;
            meal.Price = model.Price;
            meal.TokenSellStartDateTime = model.TokenSellStartDateTime;
            meal.TokenSellEndDateTime = model.TokenSellEndDateTime;
            meal.MealServingStartDateTime = model.MealServingStartDateTime;
            meal.MealServingEndDateTime = model.MealServingEndDateTime;
            meal.Capacity = model.Capacity;
            meal.Description = model.Description;

            // Save changes
            _context.MealSessions.Update(meal);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Meal '{model.MealName}' updated successfully.";
            return RedirectToAction(nameof(Meals));
        }

        // Archive Meal POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveMeal(int id)
        {
            // Find meal by id
            var meal = await _context.MealSessions
                .Include(m => m.Orders)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meal == null)
            {
                TempData["Error"] = "Meal not found.";
                return RedirectToAction(nameof(Meals));
            }

            // Check if already archived
            if (meal.IsArchived)
            {
                TempData["Warning"] = "This meal is already archived.";
                return RedirectToAction(nameof(Meals));
            }

            // Check if meal is Upcoming (business rule: only archive upcoming meals)
            var status = MealDetailsViewModel.GetMealStatus(
                meal.TokenSellStartDateTime,
                meal.TokenSellEndDateTime,
                meal.MealServingStartDateTime,
                meal.MealServingEndDateTime,
                meal.IsArchived);

            if (status != "Upcoming")
            {
                TempData["Error"] = "Only upcoming meals can be archived.";
                return RedirectToAction(nameof(Meals));
            }

            // Check if any tokens have been sold (business rule: no sold tokens)
            if (meal.SoldCount > 0)
            {
                TempData["Error"] = "Meals with sold tokens cannot be archived.";
                return RedirectToAction(nameof(Meals));
            }

            // Archive the meal (soft archive - no delete)
            meal.IsArchived = true;
            _context.MealSessions.Update(meal);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Meal '{meal.Name}' archived successfully.";
            return RedirectToAction(nameof(Meals));
        }

        // Meal Details GET - Read-only inspection/audit page
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> MealDetails(int id)
        {
            // Fetch meal with orders
            var meal = await _context.MealSessions
                .Include(m => m.Orders)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meal == null)
            {
                TempData["Error"] = "Meal not found.";
                return RedirectToAction(nameof(Meals));
            }

            // Map to read-only ViewModel
            var viewModel = new MealDetailsViewModel
            {
                Id = meal.Id,
                MealName = meal.Name,
                Price = meal.Price,
                Capacity = meal.Capacity,
                SoldCount = meal.SoldCount, // Use static property as in MealHistory
                TokenSellStartDateTime = meal.TokenSellStartDateTime,
                TokenSellEndDateTime = meal.TokenSellEndDateTime,
                MealServingStartDateTime = meal.MealServingStartDateTime,
                MealServingEndDateTime = meal.MealServingEndDateTime,
                CreatedAt = meal.CreatedAt,
                IsArchived = meal.IsArchived,
                CurrentStatus = MealDetailsViewModel.GetMealStatus(
                    meal.TokenSellStartDateTime,
                    meal.TokenSellEndDateTime,
                    meal.MealServingStartDateTime,
                    meal.MealServingEndDateTime,
                    meal.IsArchived)
            };

            return View(viewModel);
        }

        // Settlement Requests - Redirected to AdminSettlementController
        public IActionResult SettlementRequests()
        {
            return RedirectToAction("PendingRequests", "AdminSettlement");
        }



        // Reports - Redirected to ReportsController
        [HttpGet]
        public IActionResult Reports()
        {
            return RedirectToAction("Dashboard", "Reports");
        }
    }
}

