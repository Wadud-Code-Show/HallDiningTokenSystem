using HallDiningTokenSystem.Services;
using HallDiningTokenSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HallDiningTokenSystem.Models;
using HallDiningTokenSystem.Data;

namespace HallDiningTokenSystem.Controllers
{
    /// <summary>
    /// Settlement management for SuperAdmin users
    /// Handles approval/rejection of settlement requests and audit trails
    /// </summary>
    [Authorize(Roles = "SuperAdmin")]
    public class AdminSettlementController : Controller
    {
        private readonly ISettlementService _settlementService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminSettlementController(
            ISettlementService settlementService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _settlementService = settlementService;
            _userManager = userManager;
            _context = context;
        }

        // ========== Settlement Review Pages ==========

        /// <summary>
        /// View all pending settlement requests for SuperAdmin review
        /// GET: /AdminSettlement/PendingRequests
        /// </summary>
        public async Task<IActionResult> PendingRequests()
        {
            var pendingRequests = await _settlementService.GetPendingRequestsAsync();

            var viewModel = new PendingSettlementRequestsListViewModel
            {
                Requests = pendingRequests
                    .Select(req => new PendingSettlementRequestViewModel
                    {
                        Id = req.Id,
                        CashAdminId = req.CashAdminId,
                        CashAdminName = req.CashAdmin?.FullName ?? req.CashAdmin?.UserName ?? "Unknown",
                        CashAdminEmail = req.CashAdmin?.Email ?? "N/A",
                        RequestedAmount = req.Amount,
                        RequestedAt = req.RequestedAt,
                        RequestNote = req.RequestNote,
                        TotalCollectedByAdmin = 0, // Will calculate below
                        CollectionCount = 0,
                        TotalSettledByAdmin = 0
                    })
                    .ToList(),
                TotalPendingCount = pendingRequests.Count,
                TotalPendingAmount = pendingRequests.Sum(r => r.Amount),
                AsOfDate = DateTime.Now
            };

            // Calculate additional metrics for each request
            foreach (var item in viewModel.Requests)
            {
                item.TotalCollectedByAdmin = await _settlementService.GetTotalCollectedAsync(item.CashAdminId);
                item.TotalSettledByAdmin = await _settlementService.GetTotalSettledAsync(item.CashAdminId);
                item.CollectionCount = await _settlementService.GetCollectionCountAsync(item.CashAdminId);
            }

            return View(viewModel);
        }

        /// <summary>
        /// Approve a pending settlement request
        /// GET: /AdminSettlement/ApproveRequest/{id}
        /// </summary>
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var request = await _settlementService.GetRequestDetailsAsync(id);
            if (request == null || request.Status != "Pending")
            {
                TempData["Error"] = "Request not found or cannot be approved.";
                return RedirectToAction("PendingRequests");
            }

            var viewModel = new ApprovSettlementRequestViewModel
            {
                RequestId = request.Id,
                CashAdminName = request.CashAdmin?.FullName ?? request.CashAdmin?.UserName ?? "Unknown",
                Amount = request.Amount,
                RequestedAt = request.RequestedAt,
                RequestNote = request.RequestNote
            };

            return View(viewModel);
        }

        /// <summary>
        /// Submit settlement approval
        /// POST: /AdminSettlement/ApproveRequest
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequest(ApprovSettlementRequestViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["Error"] = "Current user not found.";
                return RedirectToAction("PendingRequests");
            }

            var result = await _settlementService.ApproveSettlementRequestAsync(
                model.RequestId,
                currentUser.Id,
                model.ApprovalNotes);

            if (result.Success)
            {
                TempData["Success"] = $"Settlement request of ৳{model.Amount:N2} approved successfully.";
                return RedirectToAction("PendingRequests");
            }
            else
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("ApproveRequest", new { id = model.RequestId });
            }
        }

        /// <summary>
        /// Reject a pending settlement request
        /// GET: /AdminSettlement/RejectRequest/{id}
        /// </summary>
        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _settlementService.GetRequestDetailsAsync(id);
            if (request == null || request.Status != "Pending")
            {
                TempData["Error"] = "Request not found or cannot be rejected.";
                return RedirectToAction("PendingRequests");
            }

            var viewModel = new RejectSettlementRequestViewModel
            {
                RequestId = request.Id,
                CashAdminName = request.CashAdmin?.FullName ?? request.CashAdmin?.UserName ?? "Unknown",
                Amount = request.Amount,
                RequestedAt = request.RequestedAt,
                RequestNote = request.RequestNote
            };

            return View(viewModel);
        }

        /// <summary>
        /// Submit settlement rejection (with mandatory reason)
        /// POST: /AdminSettlement/RejectRequest
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(RejectSettlementRequestViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.RejectionReason))
            {
                TempData["Error"] = "Rejection reason is required.";
                return RedirectToAction("RejectRequest", new { id = model.RequestId });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["Error"] = "Current user not found.";
                return RedirectToAction("PendingRequests");
            }

            var result = await _settlementService.RejectSettlementRequestAsync(
                model.RequestId,
                currentUser.Id,
                model.RejectionReason);

            if (result.Success)
            {
                TempData["Success"] = $"Settlement request of ৳{model.Amount:N2} rejected.";
                return RedirectToAction("PendingRequests");
            }
            else
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("RejectRequest", new { id = model.RequestId });
            }
        }

        // ========== Audit & History Pages ==========

        /// <summary>
        /// View detailed audit trail for a specific settlement request
        /// GET: /AdminSettlement/RequestAudit/{id}
        /// </summary>
        public async Task<IActionResult> RequestAudit(int id)
        {
            var request = await _settlementService.GetRequestDetailsAsync(id);
            if (request == null)
            {
                TempData["Error"] = "Settlement request not found.";
                return RedirectToAction("SettlementHistory");
            }

            var viewModel = new SettlementRequestAuditViewModel
            {
                RequestId = request.Id,
                CashAdminName = request.CashAdmin?.FullName ?? request.CashAdmin?.UserName ?? "Unknown",
                CashAdminEmail = request.CashAdmin?.Email ?? "N/A",
                Amount = request.Amount,
                Status = request.Status,
                RequestedAt = request.RequestedAt,
                RequestNote = request.RequestNote,
                ReviewedByAdminName = request.ReviewedByAdmin?.FullName ?? request.ReviewedByAdmin?.UserName,
                ReviewedAt = request.ReviewedAt,
                AdminNote = request.AdminNote,
                LinkedSettlementId = request.CashSettlementId
            };

            return View(viewModel);
        }

        /// <summary>
        /// View settlement history with optional filtering
        /// GET: /AdminSettlement/SettlementHistory
        /// </summary>
        public async Task<IActionResult> SettlementHistory(string? cashAdminId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var settlements = await _settlementService.GetSettlementHistoryAsync(cashAdminId, fromDate, toDate);

            var viewModel = new SettlementHistoryViewModel
            {
                Settlements = settlements
                    .Select(s => new SettlementRecordViewModel
                    {
                        Id = s.Id,
                        CashAdminName = s.CashAdmin?.FullName ?? s.CashAdmin?.UserName ?? "Unknown",
                        SettlementDate = s.SettlementDate,
                        Amount = s.Amount,
                        ReceivedByAdminName = s.ReceivedByAdmin?.FullName ?? s.ReceivedByAdmin?.UserName ?? "Unknown",
                        Note = s.Note
                    })
                    .ToList(),
                TotalCount = settlements.Count,
                TotalAmount = settlements.Sum(s => s.Amount),
                FilterCashAdminId = cashAdminId,
                FilterFromDate = fromDate,
                FilterToDate = toDate
            };

            // Get available CashAdmins for filter dropdown
            var cashAdmins = _context.CashCollections
                .Select(cc => new { cc.CashAdminId, cc.CashAdmin.FullName, cc.CashAdmin.UserName })
                .Distinct()
                .ToList();

            viewModel.AvailableCashAdmins = cashAdmins
                .Select(ca => new CashAdminOption
                {
                    Id = ca.CashAdminId,
                    Name = ca.FullName ?? ca.UserName ?? "Unknown"
                })
                .OrderBy(o => o.Name)
                .ToList();

            return View(viewModel);
        }

        /// <summary>
        /// View detailed settlement with all audit information
        /// GET: /AdminSettlement/SettlementDetails/{id}
        /// </summary>
        public async Task<IActionResult> SettlementDetails(int id)
        {
            var settlement = await _settlementService.GetSettlementDetailsAsync(id);
            if (settlement == null)
            {
                TempData["Error"] = "Settlement not found.";
                return RedirectToAction("SettlementHistory");
            }

            var viewModel = new SettlementRecordViewModel
            {
                Id = settlement.Id,
                CashAdminName = settlement.CashAdmin?.FullName ?? settlement.CashAdmin?.UserName ?? "Unknown",
                SettlementDate = settlement.SettlementDate,
                Amount = settlement.Amount,
                ReceivedByAdminName = settlement.ReceivedByAdmin?.FullName ?? settlement.ReceivedByAdmin?.UserName ?? "Unknown",
                Note = settlement.Note
            };

            return View(viewModel);
        }

        // ========== Financial Consistency Dashboard ==========

        /// <summary>
        /// System-wide financial consistency dashboard
        /// Shows: TotalCollected = TotalSettled + PendingSettlement for all CashAdmins
        /// GET: /AdminSettlement/ConsistencyDashboard
        /// </summary>
        public async Task<IActionResult> ConsistencyDashboard()
        {
            var summaries = await _settlementService.GetCashAdminSummariesAsync();

            var viewModel = new SettlementConsistencyDashboardViewModel
            {
                CashAdminSummaries = summaries
                    .Select(s => new CashAdminConsistencyItemViewModel
                    {
                        CashAdminId = s.CashAdminId,
                        CashAdminName = s.CashAdminName,
                        TotalCollected = s.TotalCollected,
                        TotalSettled = s.TotalSettled,
                        PendingSettlement = s.PendingSettlement,
                        CollectionCount = s.CollectionCount,
                        SettlementCount = s.SettlementCount,
                        IsConsistent = s.IsConsistent
                    })
                    .ToList(),
                SystemTotalCollected = summaries.Sum(s => s.TotalCollected),
                SystemTotalSettled = summaries.Sum(s => s.TotalSettled),
                SystemTotalPending = summaries.Sum(s => s.PendingSettlement),
                CashAdminCount = summaries.Count,
                AnomalyCount = summaries.Count(s => !s.IsConsistent),
                AsOfDate = DateTime.Now
            };

            return View(viewModel);
        }

        // ========== Admin Dashboard ==========

        /// <summary>
        /// Settlement management dashboard (high-level overview)
        /// GET: /AdminSettlement/Dashboard
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            var pendingRequests = await _settlementService.GetPendingRequestsAsync();
            var summaries = await _settlementService.GetCashAdminSummariesAsync();

            var dashboardModel = new
            {
                PendingRequestCount = pendingRequests.Count,
                PendingRequestAmount = pendingRequests.Sum(r => r.Amount),
                TotalCollected = summaries.Sum(s => s.TotalCollected),
                TotalSettled = summaries.Sum(s => s.TotalSettled),
                TotalPending = summaries.Sum(s => s.PendingSettlement),
                CashAdminCount = summaries.Count,
                AnomalyCount = summaries.Count(s => !s.IsConsistent),
                IsSystemBalanced = Math.Abs(summaries.Sum(s => s.TotalCollected) - (summaries.Sum(s => s.TotalSettled) + summaries.Sum(s => s.PendingSettlement))) < 0.01m
            };

            return View(dashboardModel);
        }
    }
}
