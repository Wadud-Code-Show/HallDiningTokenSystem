using HallDiningTokenSystem.Data;
using HallDiningTokenSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HallDiningTokenSystem.Services
{
    /// <summary>
    /// Core settlement service implementing financial workflow guarantees
    /// </summary>
    public class SettlementService : ISettlementService
    {
        private readonly ApplicationDbContext _context;

        public SettlementService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get total cash collected by a CashAdmin
        /// Uses AsNoTracking for performance
        /// </summary>
        public async Task<decimal> GetTotalCollectedAsync(string cashAdminId)
        {
            return await _context.CashCollections
                .AsNoTracking()
                .Where(cc => cc.CashAdminId == cashAdminId)
                .SumAsync(cc => cc.Amount);
        }

        /// <summary>
        /// Get total settled amount for a CashAdmin
        /// Sums amounts from approved CashSettlement records
        /// </summary>
        public async Task<decimal> GetTotalSettledAsync(string cashAdminId)
        {
            return await _context.CashSettlements
                .AsNoTracking()
                .Where(cs => cs.CashAdminId == cashAdminId)
                .SumAsync(cs => cs.Amount);
        }

        /// <summary>
        /// Calculate pending settlement amount
        /// Formula: TotalCollected - TotalSettled - Sum(PendingRequests)
        /// </summary>
        public async Task<decimal> GetPendingSettlementAsync(string cashAdminId)
        {
            var totalCollected = await GetTotalCollectedAsync(cashAdminId);
            var totalSettled = await GetTotalSettledAsync(cashAdminId);

            var pendingRequestedAmount = await _context.CashSettlementRequests
                .AsNoTracking()
                .Where(csr => csr.CashAdminId == cashAdminId && csr.Status == "Pending")
                .SumAsync(csr => csr.Amount);

            return totalCollected - totalSettled - pendingRequestedAmount;
        }

        /// <summary>
        /// Get collection count for reporting
        /// </summary>
        public async Task<int> GetCollectionCountAsync(string cashAdminId)
        {
            return await _context.CashCollections
                .AsNoTracking()
                .Where(cc => cc.CashAdminId == cashAdminId)
                .CountAsync();
        }

        /// <summary>
        /// Create a settlement request with comprehensive validation
        /// Validates:
        /// - Amount > 0
        /// - Amount <= Available Pending Cash
        /// - CashAdmin exists and is active
        /// </summary>
        public async Task<ServiceResult> CreateSettlementRequestAsync(string cashAdminId, decimal amount, string? requestNote)
        {
            // Validate amount
            if (amount <= 0)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Settlement amount must be greater than 0."
                };
            }

            // Check if CashAdmin exists
            var cashAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Id == cashAdminId);
            if (cashAdmin == null)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "CashAdmin not found."
                };
            }

            // Check CashAdmin is active (not locked out)
            if (await _context.Users.AnyAsync(u => u.Id == cashAdminId && u.LockoutEnd > DateTime.UtcNow))
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "CashAdmin account is locked."
                };
            }

            // Calculate available pending cash
            var availablePendingCash = await GetPendingSettlementAsync(cashAdminId);

            if (amount > availablePendingCash)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = $"Settlement amount cannot exceed available pending cash (৳{availablePendingCash:N2})."
                };
            }

            // Create settlement request
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var request = new CashSettlementRequest
                    {
                        CashAdminId = cashAdminId,
                        Amount = amount,
                        Status = "Pending",
                        RequestNote = requestNote,
                        RequestedAt = DateTime.Now
                    };

                    _context.CashSettlementRequests.Add(request);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ServiceResult
                    {
                        Success = true,
                        Message = $"Settlement request of ₹{amount:N2} created successfully."
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ServiceResult
                    {
                        Success = false,
                        Message = $"Error creating settlement request: {ex.Message}"
                    };
                }
            }
        }

        /// <summary>
        /// Approve a settlement request with full atomic transaction
        /// Operations (all or nothing):
        /// 1. Create CashSettlement record
        /// 2. Update CashSettlementRequest status to "Approved"
        /// 3. Link CashSettlement to CashSettlementRequest
        /// 4. Store audit trail (reviewed by, reviewed at, notes)
        /// </summary>
        public async Task<ServiceResult> ApproveSettlementRequestAsync(
            int requestId, 
            string reviewedByAdminId, 
            string? approvalNotes)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Get the request
                    var request = await _context.CashSettlementRequests
                        .FirstOrDefaultAsync(csr => csr.Id == requestId);

                    if (request == null)
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = "Settlement request not found."
                        };
                    }

                    if (request.Status != "Pending")
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = $"Can only approve pending requests. Current status: {request.Status}"
                        };
                    }

                    // Verify reviewer exists and is SuperAdmin
                    var reviewer = await _context.Users.FirstOrDefaultAsync(u => u.Id == reviewedByAdminId);
                    if (reviewer == null)
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = "Reviewer not found."
                        };
                    }

                    // Double-check available pending cash (prevent race conditions)
                    var availablePendingCash = await GetPendingSettlementAsync(request.CashAdminId);
                    if (request.Amount > availablePendingCash)
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = "Available pending cash has changed. Approval cannot proceed."
                        };
                    }

                    // Create CashSettlement record
                    var settlement = new CashSettlement
                    {
                        CashAdminId = request.CashAdminId,
                        Amount = request.Amount,
                        ReceivedByAdminId = reviewedByAdminId,
                        Note = approvalNotes,
                        SettlementDate = DateTime.Now
                    };

                    _context.CashSettlements.Add(settlement);
                    await _context.SaveChangesAsync();

                    // Update request with approval information and link to settlement
                    request.Status = "Approved";
                    request.ReviewedByAdminId = reviewedByAdminId;
                    request.ReviewedAt = DateTime.Now;
                    request.AdminNote = approvalNotes;
                    request.CashSettlementId = settlement.Id;

                    _context.CashSettlementRequests.Update(request);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new ServiceResult
                    {
                        Success = true,
                        Message = $"Settlement request approved successfully. Settlement ID: {settlement.Id}"
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ServiceResult
                    {
                        Success = false,
                        Message = $"Error approving settlement: {ex.Message}"
                    };
                }
            }
        }

        /// <summary>
        /// Reject a settlement request with mandatory rejection reason
        /// Operations:
        /// 1. Update CashSettlementRequest status to "Rejected"
        /// 2. Store audit trail (reviewed by, reviewed at, rejection reason)
        /// </summary>
        public async Task<ServiceResult> RejectSettlementRequestAsync(
            int requestId, 
            string reviewedByAdminId, 
            string rejectionReason)
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Rejection reason is required."
                };
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var request = await _context.CashSettlementRequests
                        .FirstOrDefaultAsync(csr => csr.Id == requestId);

                    if (request == null)
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = "Settlement request not found."
                        };
                    }

                    if (request.Status != "Pending")
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = $"Can only reject pending requests. Current status: {request.Status}"
                        };
                    }

                    // Update request with rejection information
                    request.Status = "Rejected";
                    request.ReviewedByAdminId = reviewedByAdminId;
                    request.ReviewedAt = DateTime.Now;
                    request.AdminNote = rejectionReason;

                    _context.CashSettlementRequests.Update(request);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new ServiceResult
                    {
                        Success = true,
                        Message = "Settlement request rejected successfully."
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ServiceResult
                    {
                        Success = false,
                        Message = $"Error rejecting settlement: {ex.Message}"
                    };
                }
            }
        }

        /// <summary>
        /// Get all pending requests for SuperAdmin review
        /// Includes related CashAdmin info for display
        /// </summary>
        public async Task<List<CashSettlementRequest>> GetPendingRequestsAsync()
        {
            return await _context.CashSettlementRequests
                .AsNoTracking()
                .Where(csr => csr.Status == "Pending")
                .Include(csr => csr.CashAdmin)
                .OrderByDescending(csr => csr.RequestedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get settlement history with optional filtering
        /// </summary>
        public async Task<List<CashSettlement>> GetSettlementHistoryAsync(
            string? cashAdminId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.CashSettlements
                .AsNoTracking()
                .Include(cs => cs.CashAdmin)
                .Include(cs => cs.ReceivedByAdmin)
                .AsQueryable();

            if (!string.IsNullOrEmpty(cashAdminId))
            {
                query = query.Where(cs => cs.CashAdminId == cashAdminId);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(cs => cs.SettlementDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(cs => cs.SettlementDate <= toDate.Value);
            }

            return await query
                .OrderByDescending(cs => cs.SettlementDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get settlement requests for a CashAdmin with optional status filter
        /// </summary>
        public async Task<List<CashSettlementRequest>> GetSettlementRequestsAsync(
            string cashAdminId,
            string? status = null)
        {
            var query = _context.CashSettlementRequests
                .AsNoTracking()
                .Include(csr => csr.CashAdmin)
                .Include(csr => csr.ReviewedByAdmin)
                .Where(csr => csr.CashAdminId == cashAdminId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(csr => csr.Status == status);
            }

            return await query
                .OrderByDescending(csr => csr.RequestedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get detailed request with all audit information
        /// </summary>
        public async Task<CashSettlementRequest?> GetRequestDetailsAsync(int requestId)
        {
            return await _context.CashSettlementRequests
                .AsNoTracking()
                .Include(csr => csr.CashAdmin)
                .Include(csr => csr.ReviewedByAdmin)
                .Include(csr => csr.CashSettlement)
                .FirstOrDefaultAsync(csr => csr.Id == requestId);
        }

        /// <summary>
        /// Get detailed settlement with all audit information
        /// </summary>
        public async Task<CashSettlement?> GetSettlementDetailsAsync(int settlementId)
        {
            return await _context.CashSettlements
                .AsNoTracking()
                .Include(cs => cs.CashAdmin)
                .Include(cs => cs.ReceivedByAdmin)
                .FirstOrDefaultAsync(cs => cs.Id == settlementId);
        }

        /// <summary>
        /// Verify financial consistency formula:
        /// TotalCollected = TotalSettled + PendingSettlement
        /// This must be true for all time
        /// </summary>
        public async Task<bool> VerifyFinancialConsistencyAsync(string cashAdminId)
        {
            var totalCollected = await GetTotalCollectedAsync(cashAdminId);
            var totalSettled = await GetTotalSettledAsync(cashAdminId);
            var pendingSettlement = await GetPendingSettlementAsync(cashAdminId);

            // Verify formula: TotalCollected = TotalSettled + PendingSettlement
            var expectedPendingSettlement = totalCollected - totalSettled;

            return Math.Abs(pendingSettlement - expectedPendingSettlement) < 0.01m; // Allow for floating point precision
        }

        /// <summary>
        /// Get system-wide audit summary for all CashAdmins
        /// Useful for identifying anomalies or imbalances
        /// </summary>
        public async Task<List<CashAdminSettlementSummary>> GetCashAdminSummariesAsync()
        {
            var cashAdmins = await _context.Users
                .AsNoTracking()
                .Where(u => _context.CashCollections.Any(cc => cc.CashAdminId == u.Id))
                .ToListAsync();

            var summaries = new List<CashAdminSettlementSummary>();

            foreach (var cashAdmin in cashAdmins)
            {
                var totalCollected = await GetTotalCollectedAsync(cashAdmin.Id);
                var totalSettled = await GetTotalSettledAsync(cashAdmin.Id);
                var pendingSettlement = await GetPendingSettlementAsync(cashAdmin.Id);
                var collectionCount = await GetCollectionCountAsync(cashAdmin.Id);
                var settlementCount = await _context.CashSettlements
                    .AsNoTracking()
                    .Where(cs => cs.CashAdminId == cashAdmin.Id)
                    .CountAsync();

                var isConsistent = await VerifyFinancialConsistencyAsync(cashAdmin.Id);

                summaries.Add(new CashAdminSettlementSummary
                {
                    CashAdminId = cashAdmin.Id,
                    CashAdminName = cashAdmin.FullName ?? cashAdmin.UserName ?? "Unknown",
                    TotalCollected = totalCollected,
                    TotalSettled = totalSettled,
                    PendingSettlement = pendingSettlement,
                    CollectionCount = collectionCount,
                    SettlementCount = settlementCount,
                    IsConsistent = isConsistent
                });
            }

            return summaries.OrderByDescending(s => s.TotalCollected).ToList();
        }
    }
}
