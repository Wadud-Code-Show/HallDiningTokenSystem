using HallDiningTokenSystem.Data;
using HallDiningTokenSystem.Models;
using HallDiningTokenSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HallDiningTokenSystem.Controllers
{
    [Authorize(Roles = "CashAdmin")]
    public class CashAdminController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CashAdminController(IWalletService walletService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _walletService = walletService;
            _userManager = userManager;
            _context = context;
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["Error"] = "Current user not found.";
                return RedirectToAction("Index", "Home");
            }

            // Get all cash collections for current CashAdmin
            var collections = await _context.CashCollections
                .Where(cc => cc.CashAdminId == currentUser.Id)
                .Include(cc => cc.Student)
                .ToListAsync();

            // Calculate metrics
            var totalCollected = collections.Sum(cc => cc.Amount);
            var todayCollected = collections
                .Where(cc => cc.CreatedAt.Date == DateTime.Now.Date)
                .Sum(cc => cc.Amount);
            var totalTopUps = collections.Count;

            // Calculate TotalSettled from CashSettlements where CashAdminId == current user
            var totalSettled = await _context.CashSettlements
                .Where(cs => cs.CashAdminId == currentUser.Id)
                .SumAsync(cs => cs.Amount);

            var pendingCash = totalCollected - totalSettled;

            // Get recent top-ups (last 5)
            var recentTopUps = collections
                .OrderByDescending(cc => cc.CreatedAt)
                .Take(5)
                .Select(cc => new RecentTopUpViewModel
                {
                    Id = cc.Id,
                    CreatedAt = cc.CreatedAt,
                    StudentName = cc.Student?.FullName ?? "Unknown",
                    StudentId = cc.Student?.StudentId ?? "N/A",
                    Amount = cc.Amount
                })
                .ToList();

            // Build view model
            var dashboardModel = new CashAdminDashboardViewModel
            {
                TotalCollected = totalCollected,
                TodayCollected = todayCollected,
                TotalTopUps = totalTopUps,
                TotalSettled = totalSettled,
                PendingCash = pendingCash,
                RecentTopUps = recentTopUps
            };

            return View(dashboardModel);
        }

        // Top Up Wallet GET
        public IActionResult TopUpWallet()
        {
            return View(new WalletTopUpViewModel());
        }

        // Search Student POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchStudent(WalletTopUpViewModel model)
        {
            // Remove Amount validation during search (only validate StudentId)
            ModelState.Remove(nameof(model.Amount));

            // Validate StudentId field only
            if (string.IsNullOrEmpty(model.StudentId) || !System.Text.RegularExpressions.Regex.IsMatch(model.StudentId, @"^\d{6}$"))
            {
                TempData["Error"] = "Please enter a valid 6-digit Student ID.";
                return View("TopUpWallet", model);
            }

            var student = await _walletService.FindStudentByStudentIdAsync(model.StudentId);
            if (student == null)
            {
                TempData["Error"] = $"Student with ID {model.StudentId} not found.";
                return View("TopUpWallet", model);
            }

            var wallet = await _walletService.GetWalletByUserIdAsync(student.Id);
            if (wallet == null)
            {
                TempData["Error"] = "Student wallet not found.";
                return View("TopUpWallet", model);
            }

            // Populate model with student details
            model.StudentUserId = student.Id;
            model.StudentName = student.FullName;
            model.StudentEmail = student.Email;
            model.CurrentBalance = wallet.Balance;

            return View("TopUpWallet", model);
        }

        // Confirm Top-up POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopUpWallet(WalletTopUpViewModel model)
        {
            // Validate StudentUserId is set (student was searched)
            if (string.IsNullOrEmpty(model.StudentUserId))
            {
                TempData["Error"] = "Invalid student. Please search again.";
                return View(model);
            }

            // Validate Amount has a value and is within range
            if (!model.Amount.HasValue || model.Amount <= 0 || model.Amount > 999999999)
            {
                TempData["Error"] = "Top-up amount must be a whole number greater than 0.";
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["Error"] = "Current user not found.";
                return View(model);
            }

            // Convert int to decimal for service call
            var result = await _walletService.TopUpWalletAsync(model.StudentUserId, (decimal)model.Amount.Value, currentUser.Id, model.Note);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction("TopUpWallet");
            }
            else
            {
                TempData["Error"] = result.Message;
                return View(model);
            }
        }

        // Reports GET
        public async Task<IActionResult> Reports()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["Error"] = "Current user not found.";
                return RedirectToAction("Dashboard");
            }

            // Get all cash collections for current CashAdmin with student and transaction details
            var collections = await _context.CashCollections
                .Where(cc => cc.CashAdminId == currentUser.Id)
                .Include(cc => cc.Student)
                .Include(cc => cc.WalletTransaction)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();

            // Build view model
            var reportModel = new CashAdminReportViewModel
            {
                TotalTopUps = collections.Count,
                TotalCollected = collections.Sum(cc => cc.Amount),
                TodayCollected = collections
                    .Where(cc => cc.CreatedAt.Date == DateTime.Now.Date)
                    .Sum(cc => cc.Amount),
                TotalStudentsServed = collections
                    .Select(cc => cc.StudentId)
                    .Distinct()
                    .Count(),
                CollectionDetails = collections
                    .Select(cc => new CashCollectionDetailViewModel
                    {
                        Id = cc.Id,
                        CreatedAt = cc.CreatedAt,
                        StudentName = cc.Student?.FullName ?? "Unknown",
                        StudentId = cc.Student?.StudentId ?? "N/A",
                        StudentEmail = cc.Student?.Email ?? "N/A",
                        Amount = cc.Amount,
                        Note = cc.WalletTransaction?.Description
                    })
                    .ToList()
            };

            return View(reportModel);
        }
    }
}
