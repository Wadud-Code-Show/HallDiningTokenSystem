using HallDiningTokenSystem.Data;
using HallDiningTokenSystem.Models;
using HallDiningTokenSystem.ViewModels;
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

            // Count purchased and redeemed tokens
            var totalPurchased = await _context.MealTokens
                .Where(mt => mt.Order.UserId == currentUser.Id)
                .CountAsync();

            var totalRedeemed = await _context.MealTokens
                .Where(mt => mt.Order.UserId == currentUser.Id && mt.Status == "Redeemed")
                .CountAsync();

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
                TotalPurchasedTokens = totalPurchased,
                TotalRedeemedTokens = totalRedeemed,
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

        // My Tokens
        public async Task<IActionResult> MyTokens()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var tokens = await _context.MealTokens
                .Include(mt => mt.Order)
                .ThenInclude(o => o.MealSession)
                .Where(mt => mt.Order.UserId == currentUser.Id)
                .OrderByDescending(mt => mt.CreatedAt)
                .ToListAsync();

            var viewModel = new MyTokensViewModel
            {
                StudentId = currentUser.StudentId ?? "N/A",
                StudentName = currentUser.FullName ?? currentUser.UserName ?? "Student",
                Tokens = tokens.Select(mt => new MyTokensViewModel.TokenItem
                {
                    TokenId = mt.Id,
                    MealName = mt.Order.MealSession.Name,
                    OtpCode = mt.OtpCode,
                    Status = mt.Status,
                    PurchaseDate = mt.Order.OrderDate,
                    MealServingStart = mt.Order.MealSession.MealServingStartDateTime,
                    MealServingEnd = mt.Order.MealSession.MealServingEndDateTime,
                    ExpiresAt = mt.ExpiresAt,
                    CreatedAt = mt.CreatedAt
                }).ToList()
            };

            return View(viewModel);
        }

        // Wallet History
        public async Task<IActionResult> WalletHistory()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var wallet = await _context.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == currentUser.Id);

            if (wallet == null)
            {
                // Create wallet if it doesn't exist
                wallet = new Wallet { UserId = currentUser.Id, Balance = 0, CreatedAt = DateTime.Now };
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            var viewModel = new WalletHistoryViewModel
            {
                StudentId = currentUser.StudentId ?? "N/A",
                StudentName = currentUser.FullName ?? currentUser.UserName ?? "Student",
                CurrentBalance = wallet.Balance,
                Transactions = wallet.Transactions
                    .OrderByDescending(t => t.CreatedAt)
                    .Select((t, index) =>
                    {
                        var balance = wallet.Balance;
                        var transactions = wallet.Transactions.OrderByDescending(x => x.CreatedAt).ToList();
                        for (int i = 0; i < index; i++)
                        {
                            var prev = transactions[i];
                            balance -= (prev.TransactionType == "Credit" ? prev.Amount : -prev.Amount);
                        }
                        return new WalletHistoryViewModel.TransactionItem
                        {
                            TransactionId = t.Id,
                            Date = t.CreatedAt,
                            Type = t.TransactionType,
                            Amount = t.Amount,
                            Description = t.Description,
                            RunningBalance = balance
                        };
                    }).ToList()
            };

            return View(viewModel);
        }
        // Buy Token POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyToken(int mealId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Validate meal exists and is available
                    var meal = await _context.MealSessions
                        .Include(m => m.Orders)
                        .FirstOrDefaultAsync(m => m.Id == mealId);
                    
                    if (meal == null || meal.IsArchived)
                        return NotFound();

                    var status = MealDetailsViewModel.GetMealStatus(
                        meal.TokenSellStartDateTime,
                        meal.TokenSellEndDateTime,
                        meal.MealServingStartDateTime,
                        meal.MealServingEndDateTime,
                        meal.IsArchived);

                    if (status != "Sale Open")
                    {
                        TempData["Error"] = "This meal is no longer available for purchase.";
                        return RedirectToAction(nameof(MealDetails), new { id = mealId });
                    }

                    // 2. Validate wallet balance
                    var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == currentUser.Id);
                    if (wallet == null || wallet.Balance < meal.Price)
                    {
                        TempData["Error"] = "Insufficient wallet balance.";
                        return RedirectToAction(nameof(MealDetails), new { id = mealId });
                    }

                    // 3. Create Order
                    var order = new Order
                    {
                        UserId = currentUser.Id,
                        MealSessionId = meal.Id,
                        Amount = meal.Price,
                        Status = "Completed", // Successful purchase
                        OrderDate = DateTime.Now
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // 4. Create MealToken
                    var mealToken = new MealToken
                    {
                        OrderId = order.Id,
                        OtpCode = GenerateOTP(),
                        Status = "Active",
                        CreatedAt = DateTime.Now,
                        ExpiresAt = meal.MealServingEndDateTime
                    };
                    _context.MealTokens.Add(mealToken);

                    // 5. Update Wallet
                    wallet.Balance -= meal.Price;
                    _context.Wallets.Update(wallet);

                    // 6. Create WalletTransaction
                    var walletTransaction = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Amount = meal.Price,
                        TransactionType = "Debit",
                        Description = $"Token purchase for {meal.Name}",
                        CreatedAt = DateTime.Now
                    };
                    _context.WalletTransactions.Add(walletTransaction);

                    // 7. Update MealSession.SoldCount
                    meal.SoldCount += 1;
                    _context.MealSessions.Update(meal);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = $"Token purchased successfully! Your OTP: {mealToken.OtpCode}";
                    return RedirectToAction(nameof(MyTokens));
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Purchase failed. Please try again.";
                    return RedirectToAction(nameof(MealDetails), new { id = mealId });
                }
            }
        }

        private string GenerateOTP()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }
}

