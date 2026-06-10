using HallDiningTokenSystem.Data;
using HallDiningTokenSystem.Models;
using HallDiningTokenSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HallDiningTokenSystem.Controllers
{
    [Authorize(Roles = "DeliveryMan")]
    public class DeliveryManController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeliveryManController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard - Show serving meals and stats
        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Get meals currently in serving window
            var now = DateTime.Now;
            var servingMeals = await _context.MealSessions
                .Where(m => !m.IsArchived && now >= m.MealServingStartDateTime && now <= m.MealServingEndDateTime)
                .OrderBy(m => m.MealServingEndDateTime)
                .ToListAsync();

            // For each meal, count redeemed tokens
            var mealStats = new List<ServingDashboardViewModel.MealServingItem>();
            foreach (var meal in servingMeals)
            {
                var redeemedCount = await _context.MealTokens
                    .Where(mt => mt.Order.MealSessionId == meal.Id && mt.Status == "Redeemed")
                    .CountAsync();

                mealStats.Add(new ServingDashboardViewModel.MealServingItem
                {
                    MealId = meal.Id,
                    MealName = meal.Name,
                    ServingStart = meal.MealServingStartDateTime,
                    ServingEnd = meal.MealServingEndDateTime,
                    TotalCapacity = meal.Capacity,
                    TotalSold = meal.SoldCount,
                    RedeemedCount = redeemedCount,
                    TimeRemaining = (meal.MealServingEndDateTime - now).TotalMinutes
                });
            }

            var viewModel = new ServingDashboardViewModel
            {
                DeliveryManId = currentUser.Id,
                DeliveryManName = currentUser.FullName ?? currentUser.UserName ?? "DeliveryMan",
                ServingMeals = mealStats,
                CurrentTime = now
            };

            return View(viewModel);
        }

        // Verify Token GET - Show input form
        public async Task<IActionResult> VerifyToken()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Get current serving meals for context
            var now = DateTime.Now;
            var servingMealsCount = await _context.MealSessions
                .Where(m => !m.IsArchived && now >= m.MealServingStartDateTime && now <= m.MealServingEndDateTime)
                .CountAsync();

            var viewModel = new VerifyTokenViewModel
            {
                ActiveServingMeals = servingMealsCount
            };

            return View(viewModel);
        }

        // Verify Token POST - Process verification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyToken(string? otpCode)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Initialize result view model
            var result = new TokenVerificationResultViewModel
            {
                IsSuccess = false,
                OtpEntered = otpCode ?? string.Empty
            };

            // Validation: OTP provided
            if (string.IsNullOrWhiteSpace(otpCode))
            {
                result.ErrorMessage = "Please enter an OTP code.";
                return View("VerifyTokenResult", result);
            }

            // STEP 1: Find MealToken using OTP Code
            var mealToken = await _context.MealTokens
                .Include(mt => mt.Order)
                .ThenInclude(o => o.User)
                .Include(mt => mt.Order.MealSession)
                .FirstOrDefaultAsync(mt => mt.OtpCode == otpCode);

            if (mealToken == null)
            {
                result.ErrorMessage = "Invalid Token: OTP code not found.";
                return View("VerifyTokenResult", result);
            }

            var now = DateTime.Now;

            // STEP 2: Token must be Active
            if (mealToken.Status != "Active")
            {
                result.ErrorMessage = mealToken.Status switch
                {
                    "Redeemed" => "Token already redeemed.",
                    "Expired" => "Token has expired.",
                    "Cancelled" => "Token has been cancelled.",
                    _ => $"Token status is invalid: {mealToken.Status}"
                };
                result.TokenStatus = mealToken.Status;
                return View("VerifyTokenResult", result);
            }

            var meal = mealToken.Order.MealSession;
            var student = mealToken.Order.User;

            // STEP 3: Load related entities (already done via Include)

            // STEP 4: Meal must not be archived
            if (meal.IsArchived)
            {
                result.ErrorMessage = "This meal has been archived and cannot accept redemptions.";
                return View("VerifyTokenResult", result);
            }

            // STEP 5: Current time must be inside serving window
            if (now < meal.MealServingStartDateTime || now > meal.MealServingEndDateTime)
            {
                var timeStatusMessage = now < meal.MealServingStartDateTime
                    ? $"Serving hasn't started yet. Starts at {meal.MealServingStartDateTime:hh:mm tt}"
                    : $"Serving window has closed. Ended at {meal.MealServingEndDateTime:hh:mm tt}";

                result.ErrorMessage = $"Serving window closed: {timeStatusMessage}";
                return View("VerifyTokenResult", result);
            }

            // STEP 6: Prevent duplicate redemption (already checked in STEP 2)
            if (mealToken.Status == "Redeemed")
            {
                result.ErrorMessage = "Token has already been redeemed.";
                result.TokenStatus = mealToken.Status;
                return View("VerifyTokenResult", result);
            }

            // STEP 7: Mark token redeemed
            mealToken.Status = "Redeemed";
            mealToken.RedeemedAt = now;
            mealToken.RedeemedById = currentUser.Id;

            // STEP 8: Update Order Status
            mealToken.Order.Status = "Completed";
            mealToken.Order.CompletedAt = now;

            try
            {
                // STEP 9: Save Changes
                _context.MealTokens.Update(mealToken);
                _context.Orders.Update(mealToken.Order);
                await _context.SaveChangesAsync();

                // Success result
                result.IsSuccess = true;
                result.MealName = meal.Name;
                result.StudentName = student.FullName ?? student.UserName ?? "Unknown Student";
                result.StudentId = student.StudentId ?? "N/A";
                result.RedeemedAt = now;
                result.OtpCode = mealToken.OtpCode;
                result.TokenStatus = "Redeemed";

                return View("VerifyTokenResult", result);
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error processing redemption: {ex.Message}";
                return View("VerifyTokenResult", result);
            }
        }

        // Serving History - Show redemptions by current DeliveryMan
        public async Task<IActionResult> ServingHistory(DateTime? dateFilter = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var filterDate = dateFilter ?? DateTime.Today;

            // Get all tokens redeemed by current DeliveryMan on the filter date
            var redemptions = await _context.MealTokens
                .Where(mt => mt.RedeemedById == currentUser.Id &&
                             mt.Status == "Redeemed" &&
                             mt.RedeemedAt.HasValue &&
                             mt.RedeemedAt.Value.Date == filterDate.Date)
                .Include(mt => mt.Order)
                .ThenInclude(o => o.User)
                .Include(mt => mt.Order.MealSession)
                .OrderByDescending(mt => mt.RedeemedAt)
                .ToListAsync();

            var history = new List<ServingHistoryViewModel.RedemptionItem>();
            foreach (var token in redemptions)
            {
                history.Add(new ServingHistoryViewModel.RedemptionItem
                {
                    OtpCode = token.OtpCode,
                    StudentName = token.Order.User.FullName ?? token.Order.User.UserName ?? "Unknown",
                    StudentId = token.Order.User.StudentId ?? "N/A",
                    MealName = token.Order.MealSession.Name,
                    RedeemedAt = token.RedeemedAt ?? DateTime.Now,
                    OrderId = token.Order.Id,
                    TokenId = token.Id
                });
            }

            var viewModel = new ServingHistoryViewModel
            {
                DeliveryManId = currentUser.Id,
                DeliveryManName = currentUser.FullName ?? currentUser.UserName ?? "DeliveryMan",
                FilterDate = filterDate,
                Redemptions = history,
                TotalRedemptions = history.Count
            };

            return View(viewModel);
        }
    }
}
