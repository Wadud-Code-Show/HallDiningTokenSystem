using HallDiningTokenSystem.Data;
using HallDiningTokenSystem.Models;
using HallDiningTokenSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HallDiningTokenSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard - Summary Overview
        public async Task<IActionResult> Dashboard()
        {
            var now = DateTime.Now;

            // Count total students
            var totalStudents = await _context.Users
                .AsNoTracking()
                .Where(u => u.StudentId != null)
                .CountAsync();

            // Count total meals
            var totalMeals = await _context.MealSessions
                .AsNoTracking()
                .CountAsync();

            // Count tokens sold (from Orders with Status = Completed)
            var tokensSold = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == "Completed")
                .CountAsync();

            // Count tokens redeemed (MealTokens with Status = Redeemed)
            var tokensRedeemed = await _context.MealTokens
                .AsNoTracking()
                .Where(mt => mt.Status == "Redeemed")
                .CountAsync();

            // Calculate total revenue (Sum of Order.Amount for completed orders)
            var totalRevenue = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == "Completed")
                .SumAsync(o => o.Amount);

            // Calculate total wallet balance across system
            var totalWalletBalance = await _context.Wallets
                .AsNoTracking()
                .SumAsync(w => w.Balance);

            var dashboard = new ReportDashboardViewModel
            {
                TotalStudents = totalStudents,
                TotalMeals = totalMeals,
                TokensSold = tokensSold,
                TokensRedeemed = tokensRedeemed,
                TotalRevenue = totalRevenue,
                CurrentWalletBalance = totalWalletBalance,
                GeneratedAt = now
            };

            return View(dashboard);
        }

        // Meals Report
        public async Task<IActionResult> Meals()
        {
            // Load sessions with orders and their tokens first to perform calculations in memory
            var mealSessions = await _context.MealSessions
                .Include(m => m.Orders)
                    .ThenInclude(o => o.MealTokens)
                .AsNoTracking()
                .OrderByDescending(m => m.MealServingEndDateTime)
                .ToListAsync();

            // Perform projection in memory
            var meals = mealSessions.Select(m => new MealReportViewModel.MealItem
            {
                MealId = m.Id,
                MealName = m.Name,
                Status = GetMealStatus(m),
                Price = m.Price,
                Capacity = m.Capacity,
                SoldCount = m.Orders.Count(o => o.Status == "Completed"),
                RemainingCapacity = m.Capacity - m.Orders.Count(o => o.Status == "Completed"),
                RedeemedCount = m.Orders
                    .SelectMany(o => o.MealTokens)
                    .Count(mt => mt.Status == "Redeemed"),
                Revenue = m.Orders
                    .Where(o => o.Status == "Completed")
                    .Sum(o => o.Amount),
                TokenSaleStart = m.TokenSellStartDateTime,
                TokenSaleEnd = m.TokenSellEndDateTime,
                ServingStart = m.MealServingStartDateTime,
                ServingEnd = m.MealServingEndDateTime,
                IsArchived = m.IsArchived
            })
            .ToList();

            var viewModel = new MealReportViewModel
            {
                Meals = meals,
                TotalMeals = meals.Count,
                TotalRevenue = meals.Sum(m => m.Revenue),
                GeneratedAt = DateTime.Now
            };

            return View(viewModel);
        }

        // Meal Details Report
        public async Task<IActionResult> MealDetails(int id)
        {
            var meal = await _context.MealSessions
                .AsNoTracking()
                .Include(m => m.Orders)
                .ThenInclude(o => o.User)
                .Include(m => m.Orders)
                .ThenInclude(o => o.MealTokens)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meal == null)
                return NotFound();

            var studentParticipation = meal.Orders
                .Where(o => o.Status == "Completed")
                .Select(o => new MealDetailsReportViewModel.StudentParticipationItem
                {
                    StudentName = o.User.FullName ?? o.User.UserName ?? "Unknown",
                    StudentId = o.User.StudentId ?? "N/A",
                    PurchaseDate = o.OrderDate,
                    IsRedeemed = o.MealTokens.Any(mt => mt.Status == "Redeemed"),
                    RedeemedTime = o.MealTokens
                        .Where(mt => mt.Status == "Redeemed")
                        .Select(mt => mt.RedeemedAt)
                        .FirstOrDefault()
                })
                .OrderByDescending(s => s.PurchaseDate)
                .ToList();

            var viewModel = new MealDetailsReportViewModel
            {
                MealId = meal.Id,
                MealName = meal.Name,
                MealDescription = meal.Description,
                Price = meal.Price,
                Capacity = meal.Capacity,
                TotalSold = meal.Orders.Count(o => o.Status == "Completed"),
                TotalRedeemed = meal.Orders
                    .SelectMany(o => o.MealTokens)
                    .Count(mt => mt.Status == "Redeemed"),
                TotalRevenue = meal.Orders
                    .Where(o => o.Status == "Completed")
                    .Sum(o => o.Amount),
                Status = GetMealStatus(meal),
                TokenSaleStart = meal.TokenSellStartDateTime,
                TokenSaleEnd = meal.TokenSellEndDateTime,
                ServingStart = meal.MealServingStartDateTime,
                ServingEnd = meal.MealServingEndDateTime,
                StudentParticipation = studentParticipation,
                GeneratedAt = DateTime.Now
            };

            return View(viewModel);
        }

        // Students Report
        public async Task<IActionResult> Students()
        {
            // Get all students with wallets
            var studentWallets = await _context.Users
                .AsNoTracking()
                .Where(u => u.StudentId != null)
                .Join(
                    _context.Wallets,
                    u => u.Id,
                    w => w.UserId,
                    (u, w) => new { User = u, Wallet = w })
                .ToListAsync();

            var students = new List<StudentReportViewModel.StudentItem>();

            foreach (var item in studentWallets)
            {
                var purchasedCount = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.UserId == item.User.Id && o.Status == "Completed")
                    .CountAsync();

                var redeemedCount = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.UserId == item.User.Id && o.Status == "Completed")
                    .SelectMany(o => o.MealTokens)
                    .Where(mt => mt.Status == "Redeemed")
                    .CountAsync();

                var totalSpending = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.UserId == item.User.Id && o.Status == "Completed")
                    .SumAsync(o => o.Amount);

                students.Add(new StudentReportViewModel.StudentItem
                {
                    UserId = item.User.Id,
                    StudentName = item.User.FullName ?? item.User.UserName ?? "Unknown",
                    StudentId = item.User.StudentId ?? "N/A",
                    CurrentWalletBalance = item.Wallet.Balance,
                    PurchasedTokensCount = purchasedCount,
                    RedeemedTokensCount = redeemedCount,
                    TotalSpending = totalSpending
                });
            }

            students = students.OrderByDescending(s => s.TotalSpending).ToList();

            var viewModel = new StudentReportViewModel
            {
                Students = students,
                TotalStudents = students.Count,
                TotalWalletBalance = students.Sum(s => s.CurrentWalletBalance),
                TotalSpending = students.Sum(s => s.TotalSpending),
                GeneratedAt = DateTime.Now
            };

            return View(viewModel);
        }

        // Student Details Report
        public async Task<IActionResult> StudentDetails(string id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null || user.StudentId == null)
                return NotFound();

            var wallet = await _context.Wallets
                .AsNoTracking()
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == id);

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == id && o.Status == "Completed")
                .Include(o => o.MealSession)
                .Include(o => o.MealTokens)
                .ThenInclude(mt => mt.RedeemedBy)
                .ToListAsync();

            var purchaseHistory = orders
                .Select(o => new StudentDetailsReportViewModel.PurchaseItem
                {
                    MealName = o.MealSession.Name,
                    Amount = o.Amount,
                    PurchaseDate = o.OrderDate,
                    Status = "Completed"
                })
                .OrderByDescending(p => p.PurchaseDate)
                .ToList();

            var redemptionHistory = orders
                .SelectMany(o => o.MealTokens)
                .Where(mt => mt.Status == "Redeemed")
                .Select(mt => new StudentDetailsReportViewModel.RedemptionItem
                {
                    MealName = mt.Order.MealSession.Name,
                    OtpCode = mt.OtpCode,
                    RedeemedAt = mt.RedeemedAt ?? DateTime.Now,
                    RedeemedBy = mt.RedeemedBy != null ? mt.RedeemedBy.FullName ?? mt.RedeemedBy.UserName : "Unknown"
                })
                .OrderByDescending(r => r.RedeemedAt)
                .ToList();

            var walletTransactions = wallet?.Transactions
                .Select(wt => new StudentDetailsReportViewModel.WalletTransactionItem
                {
                    TransactionDate = wt.CreatedAt,
                    Type = wt.TransactionType,
                    Amount = wt.Amount,
                    BalanceAfter = 0 // We'll calculate this in the view if needed
                })
                .OrderByDescending(wt => wt.TransactionDate)
                .ToList() ?? new List<StudentDetailsReportViewModel.WalletTransactionItem>();

            var viewModel = new StudentDetailsReportViewModel
            {
                StudentName = user.FullName ?? user.UserName ?? "Unknown",
                StudentId = user.StudentId,
                StudentEmail = user.Email,
                CurrentWalletBalance = wallet?.Balance ?? 0,
                TotalTopups = wallet?.Transactions
                    .Where(wt => wt.TransactionType == "Credit")
                    .Sum(wt => wt.Amount) ?? 0,
                TotalSpending = orders.Sum(o => o.Amount),
                PurchaseHistory = purchaseHistory,
                RedemptionHistory = redemptionHistory,
                WalletTransactions = walletTransactions,
                GeneratedAt = DateTime.Now
            };

            return View(viewModel);
        }

        // Redemptions Report
        public async Task<IActionResult> Redemptions(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? mealId = null,
            string? studentId = null,
            string? deliveryManId = null)
        {
            startDate = startDate ?? DateTime.Today.AddDays(-30);
            endDate = endDate ?? DateTime.Now.AddDays(1);

            var redemptions = await _context.MealTokens
                .AsNoTracking()
                .Include(mt => mt.Order)
                .ThenInclude(o => o.User)
                .Include(mt => mt.Order.MealSession)
                .Include(mt => mt.RedeemedBy)
                .Where(mt => mt.Status == "Redeemed" &&
                             mt.RedeemedAt >= startDate &&
                             mt.RedeemedAt <= endDate)
                .Where(mt => mealId == null || mt.Order.MealSessionId == mealId)
                .Where(mt => studentId == null || mt.Order.UserId == studentId)
                .Where(mt => deliveryManId == null || mt.RedeemedById == deliveryManId)
                .Select(mt => new RedemptionReportViewModel.RedemptionItem
                {
                    MealName = mt.Order.MealSession.Name,
                    StudentName = mt.Order.User.FullName ?? mt.Order.User.UserName ?? "Unknown",
                    StudentId = mt.Order.User.StudentId ?? "N/A",
                    OtpCode = mt.OtpCode,
                    RedeemedBy = mt.RedeemedBy != null ? mt.RedeemedBy.FullName ?? mt.RedeemedBy.UserName : "Unknown",
                    RedeemedAt = mt.RedeemedAt ?? DateTime.Now
                })
                .OrderByDescending(r => r.RedeemedAt)
                .ToListAsync();

            var allMeals = (await _context.MealSessions
                .AsNoTracking()
                .Select(m => new { m.Id, m.Name })
                .ToListAsync())
                .Cast<dynamic>()
                .ToList();

            var allDeliveryMen = (await _context.Users
                .AsNoTracking()
                .Where(u => _context.MealTokens.Any(mt => mt.RedeemedById == u.Id))
                .Select(u => new { u.Id, u.FullName, u.UserName })
                .ToListAsync())
                .Cast<dynamic>()
                .ToList();

            var viewModel = new RedemptionReportViewModel
            {
                Redemptions = redemptions,
                TotalRedemptions = redemptions.Count,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                SelectedMealId = mealId,
                SelectedStudentId = studentId,
                SelectedDeliveryManId = deliveryManId,
                AvailableMeals = allMeals,
                AvailableDeliveryMen = allDeliveryMen,
                GeneratedAt = DateTime.Now
            };

            return View(viewModel);
        }

        // Wallets Report
        public async Task<IActionResult> Wallets()
        {
            var wallets = await _context.Wallets
                .AsNoTracking()
                .Include(w => w.User)
                .Include(w => w.Transactions)
                .Select(w => new WalletReportViewModel.WalletItem
                {
                    StudentName = w.User.FullName ?? w.User.UserName ?? "Unknown",
                    StudentId = w.User.StudentId ?? "N/A",
                    CurrentBalance = w.Balance,
                    TotalTopups = w.Transactions.Where(t => t.TransactionType == "Credit").Sum(t => t.Amount),
                    TotalSpending = w.Transactions.Where(t => t.TransactionType == "Debit").Sum(t => t.Amount),
                    NetPosition = w.Balance
                })
                .OrderByDescending(w => w.NetPosition)
                .ToListAsync();

            var viewModel = new WalletReportViewModel
            {
                Wallets = wallets,
                TotalWallets = wallets.Count,
                TotalBalance = wallets.Sum(w => w.CurrentBalance),
                TotalTopups = wallets.Sum(w => w.TotalTopups),
                TotalSpending = wallets.Sum(w => w.TotalSpending),
                GeneratedAt = DateTime.Now
            };

            return View(viewModel);
        }

        // Settlements Report
        public async Task<IActionResult> Settlements()
        {
            var settlements = await _context.CashSettlements
                .AsNoTracking()
                .Include(cs => cs.CashAdmin)
                .Include(cs => cs.ReceivedByAdmin)
                .Select(cs => new SettlementReportViewModel.SettlementItem
                {
                    CashAdminName = cs.CashAdmin.FullName ?? cs.CashAdmin.UserName ?? "Unknown",
                    CollectionDate = cs.SettlementDate,
                    Amount = cs.Amount,
                    SettlementStatus = "Completed",
                    SettlementDate = cs.SettlementDate,
                    ReceivedByAdminName = cs.ReceivedByAdmin != null ? cs.ReceivedByAdmin.FullName ?? cs.ReceivedByAdmin.UserName : "Pending",
                    SettlementNotes = cs.Note
                })
                .OrderByDescending(s => s.CollectionDate)
                .ToListAsync();

            var viewModel = new SettlementReportViewModel
            {
                Settlements = settlements,
                TotalSettlements = settlements.Count,
                TotalAmountSettled = settlements.Sum(s => s.Amount),
                GeneratedAt = DateTime.Now
            };

            return View(viewModel);
        }

        // Helper method to determine meal status
        private string GetMealStatus(MealSession meal)
        {
            var now = DateTime.Now;

            if (meal.IsArchived)
                return "Archived";

            if (now < meal.TokenSellStartDateTime)
                return "Upcoming";

            if (now >= meal.TokenSellStartDateTime && now < meal.TokenSellEndDateTime)
                return "Sale Open";

            if (now >= meal.TokenSellEndDateTime && now < meal.MealServingStartDateTime)
                return "Ready for Service";

            if (now >= meal.MealServingStartDateTime && now < meal.MealServingEndDateTime)
                return "Serving";

            return "Completed";
        }
    }
}
