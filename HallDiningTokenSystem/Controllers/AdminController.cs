using HallDiningTokenSystem.Models;
using HallDiningTokenSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HallDiningTokenSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly IMealService _mealService;

        public AdminController(IMealService mealService)
        {
            _mealService = mealService;
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

        // Meals List
        public async Task<IActionResult> Meals()
        {
            var meals = await _mealService.GetAllMealsAsync();
            return View(meals);
        }
    }
}
