using HallDiningTokenSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace HallDiningTokenSystem.Data
{
    public class DbSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbSeeder(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedUsersAsync();
        }

        private async Task SeedRolesAsync()
        {
            var roles = new[] { "SuperAdmin", "CashAdmin", "Student", "DeliveryMan" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async Task SeedUsersAsync()
        {
            // SuperAdmin
            await SeedUserAsync(
                email: "superadmin@test.com",
                password: "Test@123",
                fullName: "Super Admin",
                roles: new[] { "SuperAdmin" }
            );

            // CashAdmin 1
            await SeedUserAsync(
                email: "cashadmin1@test.com",
                password: "Test@123",
                fullName: "Authorized Cash Admin 1",
                roles: new[] { "CashAdmin" }
            );

            // CashAdmin 2
            await SeedUserAsync(
                email: "cashadmin2@test.com",
                password: "Test@123",
                fullName: "Authorized Cash Admin 2",
                roles: new[] { "CashAdmin" }
            );

            // Student 1
            await SeedUserAsync(
                email: "student1@test.com",
                password: "Test@123",
                fullName: "Rahim Student",
                studentId: "220130",
                roles: new[] { "Student" }
            );

            // Student 2
            await SeedUserAsync(
                email: "student2@test.com",
                password: "Test@123",
                fullName: "Karim Student",
                studentId: "220131",
                roles: new[] { "Student" }
            );

            // DeliveryMan
            await SeedUserAsync(
                email: "delivery@test.com",
                password: "Test@123",
                fullName: "Delivery Man",
                roles: new[] { "DeliveryMan" }
            );
        }

        private async Task SeedUserAsync(
            string email,
            string password,
            string fullName,
            string? studentId = null,
            string[]? roles = null)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                // Update user properties
                bool needsUpdate = false;

                if (existingUser.FullName != fullName)
                {
                    existingUser.FullName = fullName;
                    needsUpdate = true;
                }

                if (existingUser.StudentId != studentId)
                {
                    existingUser.StudentId = studentId;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    await _userManager.UpdateAsync(existingUser);
                }

                return;
            }

            // Create the user
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                StudentId = studentId
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Assign roles
            if (roles != null && roles.Length > 0)
            {
                await _userManager.AddToRolesAsync(user, roles);
            }

            // Create wallet for Student users
            if (roles != null && roles.Contains("Student"))
            {
                var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == user.Id);
                if (wallet == null)
                {
                    wallet = new Wallet
                    {
                        UserId = user.Id,
                        Balance = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Wallets.Add(wallet);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
