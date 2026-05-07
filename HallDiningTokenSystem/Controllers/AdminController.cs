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
    }
}

