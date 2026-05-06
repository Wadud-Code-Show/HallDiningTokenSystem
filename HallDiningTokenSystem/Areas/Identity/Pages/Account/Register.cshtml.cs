using HallDiningTokenSystem.Data;
using HallDiningTokenSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HallDiningTokenSystem.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [BindProperty]
        public StudentRegisterViewModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check for duplicate StudentId
            var existingStudent = _context.Users.FirstOrDefault(u => u.StudentId == Input.StudentId);
            if (existingStudent != null)
            {
                ModelState.AddModelError("Input.StudentId", "This Student ID is already registered.");
                return Page();
            }

            // Create ApplicationUser
            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true,
                FullName = Input.FullName,
                StudentId = Input.StudentId
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // Assign Student role
                await _userManager.AddToRoleAsync(user, "Student");

                // Create Wallet with Balance = 0
                var wallet = new Wallet
                {
                    UserId = user.Id,
                    Balance = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);

                return LocalRedirect(returnUrl);
            }

            // Add error messages from Identity
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
