using HallDiningTokenSystem.Data;
using HallDiningTokenSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HallDiningTokenSystem.Services
{
    public interface IWalletService
    {
        Task<ApplicationUser?> FindStudentByStudentIdAsync(string studentId);
        Task<Wallet?> GetWalletByUserIdAsync(string userId);
        Task<ServiceResult> TopUpWalletAsync(string studentUserId, decimal amount, string cashAdminId, string? note);
    }

    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _context;

        public WalletService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> FindStudentByStudentIdAsync(string studentId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.StudentId == studentId);
        }

        public async Task<Wallet?> GetWalletByUserIdAsync(string userId)
        {
            return await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<ServiceResult> TopUpWalletAsync(string studentUserId, decimal amount, string cashAdminId, string? note)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Find student by internal user ID
                    var student = await _context.Users.FirstOrDefaultAsync(u => u.Id == studentUserId);
                    if (student == null)
                    {
                        return ServiceResult.ErrorResult("Student not found.");
                    }

                    // Find student's wallet
                    var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == student.Id);
                    if (wallet == null)
                    {
                        return ServiceResult.ErrorResult("Student wallet not found.");
                    }

                    // Validate amount
                    if (amount <= 0)
                    {
                        return ServiceResult.ErrorResult("Top-up amount must be greater than 0.");
                    }

                    // Create wallet transaction
                    var walletTransaction = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Amount = amount,
                        TransactionType = "Credit",
                        Description = string.IsNullOrEmpty(note) ? "Cash Admin Top-up" : $"Cash Admin Top-up: {note}",
                        CreatedAt = DateTime.Now
                    };

                    _context.WalletTransactions.Add(walletTransaction);
                    await _context.SaveChangesAsync();

                    // Update wallet balance
                    wallet.Balance += amount;
                    wallet.UpdatedAt = DateTime.Now;
                    _context.Wallets.Update(wallet);
                    await _context.SaveChangesAsync();

                    // Create cash collection record
                    // CashCollection.StudentId stores the internal student user ID (foreign key)
                    var cashCollection = new CashCollection
                    {
                        CashAdminId = cashAdminId,
                        StudentId = studentUserId,
                        Amount = amount,
                        WalletTransactionId = walletTransaction.Id,
                        CreatedAt = DateTime.Now
                    };

                    _context.CashCollections.Add(cashCollection);
                    await _context.SaveChangesAsync();

                    // Commit transaction
                    await transaction.CommitAsync();

                    return ServiceResult.SuccessResult(
                        $"Wallet top-up successful. Added ₹{amount:F2} to {student.FullName}'s wallet.",
                        new { Student = student, Wallet = wallet }
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult.ErrorResult($"Error during top-up: {ex.Message}" + (ex.InnerException != null ? $" - {ex.InnerException.Message}" : ""));
                }
            }
        }
    }
}
