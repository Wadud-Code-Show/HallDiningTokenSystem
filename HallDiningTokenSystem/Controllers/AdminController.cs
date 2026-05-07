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

        // Meals List
        public async Task<IActionResult> Meals()
        {
            var meals = await _mealService.GetAllMealsAsync();
            return View(meals);
        }

        // Settlement Requests GET
        public async Task<IActionResult> SettlementRequests()
        {
            // Get all pending settlement requests with cash admin details
            var requests = await _context.CashSettlementRequests
                .Where(csr => csr.Status == "Pending")
                .Include(csr => csr.CashAdmin)
                .OrderByDescending(csr => csr.RequestedAt)
                .ToListAsync();

            var requestItems = requests
                .Select(csr => new SettlementRequestItemViewModel
                {
                    Id = csr.Id,
                    CashAdminName = csr.CashAdmin?.FullName ?? "Unknown",
                    CashAdminEmail = csr.CashAdmin?.Email ?? "N/A",
                    Amount = csr.Amount,
                    RequestedAt = csr.RequestedAt,
                    RequestNote = csr.RequestNote,
                    Status = csr.Status
                })
                .ToList();

            return View(requestItems);
        }

        // Approve Settlement Request POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveSettlement(int requestId, string? adminNote)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["Error"] = "Current user not found.";
                return RedirectToAction("SettlementRequests");
            }

            var request = await _context.CashSettlementRequests.FindAsync(requestId);
            if (request == null)
            {
                TempData["Error"] = "Settlement request not found.";
                return RedirectToAction("SettlementRequests");
            }

            if (request.Status != "Pending")
            {
                TempData["Error"] = "This request has already been reviewed.";
                return RedirectToAction("SettlementRequests");
            }

            // Create CashSettlement record
            var settlement = new CashSettlement
            {
                CashAdminId = request.CashAdminId,
                Amount = request.Amount,
                ReceivedByAdminId = currentUser.Id,
                Note = adminNote ?? request.RequestNote,
                SettlementDate = DateTime.Now
            };

            _context.CashSettlements.Add(settlement);
            await _context.SaveChangesAsync();

            // Update request to Approved
            request.Status = "Approved";
            request.ReviewedByAdminId = currentUser.Id;
            request.ReviewedAt = DateTime.Now;
            request.AdminNote = adminNote;
            request.CashSettlementId = settlement.Id;

            _context.CashSettlementRequests.Update(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Settlement request of ৳{request.Amount:N2} approved and settlement created.";
            return RedirectToAction("SettlementRequests");
        }

        // Reject Settlement Request POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectSettlement(int requestId, string? adminNote)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["Error"] = "Current user not found.";
                return RedirectToAction("SettlementRequests");
            }

            var request = await _context.CashSettlementRequests.FindAsync(requestId);
            if (request == null)
            {
                TempData["Error"] = "Settlement request not found.";
                return RedirectToAction("SettlementRequests");
            }

            if (request.Status != "Pending")
            {
                TempData["Error"] = "This request has already been reviewed.";
                return RedirectToAction("SettlementRequests");
            }

            // Update request to Rejected
            request.Status = "Rejected";
            request.ReviewedByAdminId = currentUser.Id;
            request.ReviewedAt = DateTime.Now;
            request.AdminNote = adminNote;

            _context.CashSettlementRequests.Update(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Settlement request of ৳{request.Amount:N2} rejected.";
            return RedirectToAction("SettlementRequests");
        }

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var report = new AdminReportViewModel();

            // Calculate summary metrics
            var studentUsers = await _userManager.GetUsersInRoleAsync("Student");
            var cashAdminUsers = await _userManager.GetUsersInRoleAsync("CashAdmin");

            report.TotalStudents = studentUsers.Count;
            report.TotalCashAdmins = cashAdminUsers.Count;
            report.TotalMeals = _context.MealSessions.Count();
            report.TotalTokensSold = _context.MealSessions.Sum(m => m.Orders.Count);

            // Calculate cash flow metrics
            report.TotalCashCollected = _context.CashCollections.Sum(cc => cc.Amount);
            report.TotalCashSettled = _context.CashSettlements.Sum(cs => cs.Amount);
            report.PendingCash = report.TotalCashCollected - report.TotalCashSettled;
            report.PendingSettlementRequests = _context.CashSettlementRequests.Count(csr => csr.Status == "Pending");

            // Build CashAdmin-wise summary
            foreach (var admin in cashAdminUsers)
            {
                var collected = _context.CashCollections.Where(cc => cc.CashAdminId == admin.Id).Sum(cc => cc.Amount);
                var settled = _context.CashSettlements.Where(cs => cs.CashAdminId == admin.Id).Sum(cs => cs.Amount);
                var pendingRequested = _context.CashSettlementRequests
                    .Where(csr => csr.CashAdminId == admin.Id && csr.Status == "Approved")
                    .Sum(csr => csr.Amount);
                var pendingRequestsAmount = _context.CashSettlementRequests
                    .Where(csr => csr.CashAdminId == admin.Id && csr.Status == "Pending")
                    .Sum(csr => csr.Amount);

                report.CashAdminSummary.Add(new CashAdminSummaryViewModel
                {
                    CashAdminName = admin.FullName ?? admin.UserName ?? "Unknown",
                    CashAdminEmail = admin.Email ?? string.Empty,
                    TotalCollected = collected,
                    TotalSettled = settled,
                    PendingCash = collected - settled,
                    PendingRequestsAmount = pendingRequestsAmount
                });
            }

            // Recent cash collections (last 10)
            report.RecentCollections = await _context.CashCollections
                .Include(cc => cc.CashAdmin)
                .Include(cc => cc.WalletTransaction)
                .Include(cc => cc.WalletTransaction.Wallet)
                .Include(cc => cc.WalletTransaction.Wallet.User)
                .OrderByDescending(cc => cc.CreatedAt)
                .Take(10)
                .Select(cc => new RecentCashCollectionViewModel
                {
                    CreatedAt = cc.CreatedAt,
                    CashAdminName = cc.CashAdmin.FullName ?? cc.CashAdmin.UserName ?? "Unknown",
                    StudentName = cc.WalletTransaction.Wallet.User.FullName ?? cc.WalletTransaction.Wallet.User.UserName ?? "Unknown",
                    StudentId = cc.WalletTransaction.Wallet.User.StudentId ?? "N/A",
                    Amount = cc.Amount
                })
                .ToListAsync();

            // Recent settlement requests (last 10)
            report.RecentSettlementRequests = await _context.CashSettlementRequests
                .Include(csr => csr.CashAdmin)
                .OrderByDescending(csr => csr.RequestedAt)
                .Take(10)
                .Select(csr => new RecentSettlementRequestViewModel
                {
                    RequestedAt = csr.RequestedAt,
                    CashAdminName = csr.CashAdmin.FullName ?? csr.CashAdmin.UserName ?? "Unknown",
                    Amount = csr.Amount,
                    Status = csr.Status
                })
                .ToListAsync();

            // Recent confirmed settlements (last 10)
            report.RecentSettlements = await _context.CashSettlements
                .Include(cs => cs.CashAdmin)
                .Include(cs => cs.ReceivedByAdmin)
                .OrderByDescending(cs => cs.SettlementDate)
                .Take(10)
                .Select(cs => new RecentSettlementViewModel
                {
                    SettlementDate = cs.SettlementDate,
                    CashAdminName = cs.CashAdmin.FullName ?? cs.CashAdmin.UserName ?? "Unknown",
                    Amount = cs.Amount,
                    ReceivedByAdminName = cs.ReceivedByAdmin != null ? (cs.ReceivedByAdmin.FullName ?? cs.ReceivedByAdmin.UserName ?? "Unknown") : "N/A",
                    Note = cs.Note
                })
                .ToListAsync();

            // Meal summary
            report.MealSummary = await _context.MealSessions
                .OrderByDescending(m => m.StartTime)
                .Select(m => new MealSummaryViewModel
                {
                    MealName = m.Name,
                    MealType = m.Description ?? "N/A",
                    MealDate = m.StartTime,
                    Price = m.Price,
                    Capacity = 0, // MealSession doesn't have Capacity field
                    SoldCount = m.Orders.Count
                })
                .ToListAsync();

            return View(report);
        }
    }
}

