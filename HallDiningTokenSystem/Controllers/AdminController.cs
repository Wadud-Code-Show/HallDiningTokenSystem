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
                SoldCount = meal.SoldCount,
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
                .OrderByDescending(m => m.TokenSellStartDateTime)
                .Select(m => new MealSummaryViewModel
                {
                    MealName = m.Name,
                    MealType = m.Description ?? "N/A",
                    TokenSellStartDateTime = m.TokenSellStartDateTime,
                    Price = m.Price,
                    Capacity = m.Capacity,
                    SoldCount = m.SoldCount
                })
                .ToListAsync();

            return View(report);
        }
    }
}

