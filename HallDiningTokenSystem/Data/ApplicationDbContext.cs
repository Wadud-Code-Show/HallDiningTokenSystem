using HallDiningTokenSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HallDiningTokenSystem.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<MealSession> MealSessions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<MealToken> MealTokens { get; set; }
        public DbSet<CashCollection> CashCollections { get; set; }
        public DbSet<CashSettlement> CashSettlements { get; set; }
        public DbSet<CashSettlementRequest> CashSettlementRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ApplicationUser properties
            builder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .HasMaxLength(100);

            builder.Entity<ApplicationUser>()
                .Property(u => u.StudentId)
                .HasMaxLength(50);

            // Add unique filtered index for StudentId (only where not null)
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.StudentId)
                .IsUnique()
                .HasFilter("[StudentId] IS NOT NULL");

            // Configure Wallet relationships
            builder.Entity<Wallet>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure WalletTransaction relationships
            builder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(wt => wt.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Order relationships
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.MealSession)
                .WithMany(ms => ms.Orders)
                .HasForeignKey(o => o.MealSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure MealToken relationships
            builder.Entity<MealToken>()
                .HasOne(mt => mt.Order)
                .WithMany(o => o.MealTokens)
                .HasForeignKey(mt => mt.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure CashCollection relationships
            builder.Entity<CashCollection>()
                .HasOne(cc => cc.CashAdmin)
                .WithMany()
                .HasForeignKey(cc => cc.CashAdminId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CashCollection_CashAdmin");

            builder.Entity<CashCollection>()
                .HasOne(cc => cc.Student)
                .WithMany()
                .HasForeignKey(cc => cc.StudentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CashCollection_Student");

            builder.Entity<CashCollection>()
                .HasOne(cc => cc.WalletTransaction)
                .WithMany()
                .HasForeignKey(cc => cc.WalletTransactionId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Configure CashSettlement relationships
            builder.Entity<CashSettlement>()
                .HasOne(cs => cs.CashAdmin)
                .WithMany()
                .HasForeignKey(cs => cs.CashAdminId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CashSettlement_CashAdmin");

            builder.Entity<CashSettlement>()
                .HasOne(cs => cs.ReceivedByAdmin)
                .WithMany()
                .HasForeignKey(cs => cs.ReceivedByAdminId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CashSettlement_ReceivedByAdmin");

            // Configure CashSettlementRequest relationships
            builder.Entity<CashSettlementRequest>()
                .HasOne(csr => csr.CashAdmin)
                .WithMany()
                .HasForeignKey(csr => csr.CashAdminId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CashSettlementRequest_CashAdmin");

            builder.Entity<CashSettlementRequest>()
                .HasOne(csr => csr.ReviewedByAdmin)
                .WithMany()
                .HasForeignKey(csr => csr.ReviewedByAdminId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false)
                .HasConstraintName("FK_CashSettlementRequest_ReviewedByAdmin");

            builder.Entity<CashSettlementRequest>()
                .HasOne(csr => csr.CashSettlement)
                .WithMany()
                .HasForeignKey(csr => csr.CashSettlementId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false)
                .HasConstraintName("FK_CashSettlementRequest_CashSettlement");
        }
    }
}
