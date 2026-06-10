# Student Module - Developer Quick Reference

## 🎯 Overview

The Student Foundation Module provides students with access to:
1. Dashboard - Wallet balance, meal counts, quick actions
2. Available Meals - Browsable list of upcoming meals
3. Meal Details - Full information about a specific meal

**Authorization:** `[Authorize(Roles = "Student")]`  
**Status:** ✅ Complete and ready for Phase 2

---

## 📁 File Locations

### Controller
```
HallDiningTokenSystem/Controllers/StudentController.cs
```

### ViewModels
```
HallDiningTokenSystem/ViewModels/StudentDashboardViewModel.cs
HallDiningTokenSystem/ViewModels/StudentMealViewModel.cs
HallDiningTokenSystem/ViewModels/StudentMealDetailsViewModel.cs
```

### Views
```
HallDiningTokenSystem/Views/Student/Dashboard.cshtml
HallDiningTokenSystem/Views/Student/Meals.cshtml
HallDiningTokenSystem/Views/Student/MealDetails.cshtml
```

### Navigation
```
HallDiningTokenSystem/Views/Shared/_Layout.cshtml (Student role section)
```

---

## 🔄 Data Flow

### Dashboard
```
StudentController.Dashboard()
  └─ Get current user from UserManager
  └─ Fetch wallet balance
  └─ Count Upcoming meals: Now < TokenSellStartDateTime
  └─ Count Sale Open meals: Now >= TokenSellStart && Now <= TokenSellEnd
  └─ Map to StudentDashboardViewModel
  └─ Return Dashboard.cshtml
```

### Meals List
```
StudentController.Meals()
  └─ Query MealSessions where:
	 ├─ !IsArchived
	 ├─ Now <= MealServingEndDateTime
	 └─ OrderBy TokenSellStartDateTime
  └─ Map each to StudentMealViewModel
	 ├─ Compute RemainingCapacity
	 ├─ Compute CurrentStatus
	 ├─ Compute StatusBadgeClass
	 └─ Compute CanPurchase flag
  └─ Return List<StudentMealViewModel>
  └─ Return Meals.cshtml
```

### Meal Details
```
StudentController.MealDetails(int id)
  └─ Find MealSession by id
  └─ Security checks:
	 ├─ Return NotFound if meal is null
	 ├─ Return NotFound if meal.IsArchived
	 └─ Return NotFound if status == "Completed"
  └─ Map to StudentMealDetailsViewModel
  └─ Return MealDetails.cshtml
```

---

## 💡 Key Concepts

### Status Calculation
```csharp
// Computed at ViewModel level, not stored in database
private string GetStatus()
{
	var now = DateTime.Now;

	if (now < TokenSellStartDateTime)
		return "Upcoming";

	if (now >= TokenSellStartDateTime && now <= TokenSellEndDateTime)
		return "Sale Open";

	// Other statuses never shown to students
	return "Unknown";
}
```

### Status Badge Classes
```csharp
private string GetStatusBadgeClass()
{
	return CurrentStatus switch
	{
		"Upcoming" => "bg-secondary",
		"Sale Open" => "bg-success",
		_ => "bg-secondary"
	};
}
```

### Purchase Readiness
```csharp
public bool CanPurchase => CurrentStatus == "Sale Open" && RemainingCapacity > 0;
public bool IsPurchaseReady => CanPurchase;
```

### Remaining Capacity
```csharp
public int RemainingCapacity => Capacity > 0 ? Capacity - SoldCount : 0;
```

---

## 🔧 Extending for Phase 2 (Token Purchase)

### Step 1: Enable Buy Token Button
**File:** `Views/Student/MealDetails.cshtml`

Change from:
```html
<button class="btn btn-primary btn-lg" disabled title="Token purchase will be available soon">
	<i class="bi bi-cart-plus"></i> Buy Token (Coming Soon)
</button>
```

To:
```html
<form method="post" asp-action="PurchaseToken" asp-controller="Student" class="d-inline">
	<input type="hidden" name="mealId" value="@Model?.Id" />
	<button type="submit" class="btn btn-primary btn-lg" 
			id="buyTokenBtn" onclick="return validatePurchase()">
		<i class="bi bi-cart-plus"></i> Buy Token
	</button>
</form>
```

### Step 2: Add Purchase Action
**File:** `Controllers/StudentController.cs`

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> PurchaseToken(int mealId)
{
	// Get current user
	var currentUser = await _userManager.GetUserAsync(User);
	if (currentUser == null)
		return Unauthorized();

	// Validate meal exists and is operational
	var meal = await _context.MealSessions.FindAsync(mealId);
	if (meal == null || meal.IsArchived)
		return NotFound();

	var status = StudentMealDetailsViewModel.GetMealStatus(meal...);
	if (status != "Sale Open")
	{
		TempData["Error"] = "This meal is no longer available for purchase.";
		return RedirectToAction(nameof(MealDetails), new { id = mealId });
	}

	// Validate wallet balance
	var wallet = await _context.Wallets
		.FirstOrDefaultAsync(w => w.UserId == currentUser.Id);

	if (wallet == null || wallet.Balance < meal.Price)
	{
		TempData["Error"] = "Insufficient wallet balance.";
		return RedirectToAction(nameof(MealDetails), new { id = mealId });
	}

	using (var transaction = await _context.Database.BeginTransactionAsync())
	{
		try
		{
			// Create Order
			var order = new Order
			{
				StudentId = currentUser.Id,
				MealSessionId = mealId,
				Amount = meal.Price,
				Status = "Active",
				CreatedAt = DateTime.Now
			};
			_context.Orders.Add(order);
			await _context.SaveChangesAsync();

			// Generate OTP
			var otp = GenerateOTP();

			// Create MealToken
			var mealToken = new MealToken
			{
				OrderId = order.Id,
				OtpCode = otp,
				Status = "Active",
				CreatedAt = DateTime.Now,
				ExpiresAt = meal.TokenSellEndDateTime
			};
			_context.MealTokens.Add(mealToken);

			// Deduct from wallet
			wallet.Balance -= meal.Price;
			_context.Wallets.Update(wallet);

			// Create wallet transaction
			var transaction_record = new WalletTransaction
			{
				WalletId = wallet.Id,
				Type = "Debit",
				Amount = meal.Price,
				Description = $"Token purchase for {meal.Name}",
				TransactionDate = DateTime.Now
			};
			_context.WalletTransactions.Add(transaction_record);

			// Update sold count
			meal.SoldCount += 1;
			_context.MealSessions.Update(meal);

			await _context.SaveChangesAsync();
			await transaction.CommitAsync();

			TempData["Success"] = $"Token purchased successfully! Your OTP: {otp}";
			return RedirectToAction(nameof(Dashboard));
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			TempData["Error"] = "Purchase failed. Please try again.";
			return RedirectToAction(nameof(MealDetails), new { id = mealId });
		}
	}
}
```

### Step 3: Create BuyTokenViewModel
**File:** `ViewModels/BuyTokenViewModel.cs`

```csharp
public class BuyTokenViewModel
{
	public int MealId { get; set; }
	public string MealName { get; set; } = null!;
	public decimal Price { get; set; }
	public decimal CurrentBalance { get; set; }
	public bool HasSufficientBalance => CurrentBalance >= Price;
	public int RemainingCapacity { get; set; }
}
```

### Step 4: Show Confirmation Page
**File:** `Views/Student/PurchaseConfirmation.cshtml`

```html
@model BuyTokenViewModel

<div class="container py-5">
	<div class="row justify-content-center">
		<div class="col-md-6">
			<div class="card border-0 shadow-sm rounded-3">
				<div class="card-body text-center p-4">
					<i class="bi bi-check-circle text-success" style="font-size: 3rem;"></i>
					<h3 class="fw-bold mt-3">Purchase Successful!</h3>
					<p class="text-muted">@Model?.MealName</p>

					<div class="bg-light p-3 rounded-2 my-4">
						<p class="text-muted small mb-2">Your OTP Code</p>
						<p class="fw-bold display-5 mb-0">@Model?.OtpCode</p>
					</div>

					<p class="text-muted small">Show this code at meal time to collect your meal.</p>

					<a href="@Url.Action("Dashboard", "Student")" class="btn btn-primary btn-lg">
						Back to Dashboard
					</a>
				</div>
			</div>
		</div>
	</div>
</div>
```

---

## 🧪 Testing Student Views

### Test Dashboard
```csharp
[Fact]
public async Task Dashboard_ReturnsViewWithStudentViewBag()
{
	var user = new ApplicationUser { Id = "test-student", FullName = "Test Student", StudentId = "STU001" };
	var wallet = new Wallet { UserId = "test-student", Balance = 1000m };

	// Setup test data
	var context = MockDbContext.Create();
	context.Users.Add(user);
	context.Wallets.Add(wallet);

	var controller = new StudentController(context, _userManager);
	controller.ControllerContext.HttpContext.User = CreateClaimsPrincipal(user.Id);

	var result = await controller.Dashboard();

	var viewResult = Assert.IsType<ViewResult>(result);
	var model = Assert.IsType<StudentDashboardViewModel>(viewResult.Model);

	Assert.Equal(1000m, model.CurrentBalance);
	Assert.Equal("Test Student", model.StudentName);
}
```

### Test Meals Filtering
```csharp
[Fact]
public async Task Meals_OnlyShowsOperationalMeals()
{
	var context = MockDbContext.Create();

	// Add meals with different statuses
	context.MealSessions.AddRange(
		new MealSession { Id = 1, Name = "Upcoming", IsArchived = false, MealServingEndDateTime = DateTime.Now.AddDays(1), TokenSellStartDateTime = DateTime.Now.AddDays(1) },
		new MealSession { Id = 2, Name = "Archived", IsArchived = true, MealServingEndDateTime = DateTime.Now.AddDays(1) },
		new MealSession { Id = 3, Name = "Completed", IsArchived = false, MealServingEndDateTime = DateTime.Now.AddDays(-1) }
	);

	var controller = new StudentController(context, _userManager);
	var result = await controller.Meals();

	var viewResult = Assert.IsType<ViewResult>(result);
	var model = Assert.IsType<List<StudentMealViewModel>>(viewResult.Model);

	Assert.Single(model); // Only Upcoming meal
	Assert.Equal("Upcoming", model[0].MealName);
}
```

---

## 📊 Database Schema (Used, Not Modified)

```
MealSession
├─ Id (PK)
├─ Name
├─ Price
├─ Capacity
├─ SoldCount
├─ TokenSellStartDateTime
├─ TokenSellEndDateTime
├─ MealServingStartDateTime
├─ MealServingEndDateTime
├─ IsArchived
└─ Orders (FK Collection)

Wallet
├─ Id (PK)
├─ UserId (FK)
├─ Balance
└─ WalletTransactions (FK Collection)

ApplicationUser
├─ Id (PK)
├─ FullName
├─ StudentId
├─ Wallets (FK Collection)
└─ Orders (FK Collection)
```

---

## 🚨 Common Issues & Fixes

### Issue: Student sees archived meals
**Solution:** Check controller Meals() filter includes `&& !m.IsArchived`

### Issue: Status shows incorrectly
**Solution:** Verify `DateTime.Now` is being used (not a fixed date)

### Issue: Capacity shows negative
**Solution:** Check RemainingCapacity property: `Capacity > 0 ? Capacity - SoldCount : 0`

### Issue: Wallet balance shows wrong currency
**Solution:** Ensure view uses `৳` symbol, not `ToString("C")`

### Issue: Buy Token button not disabled
**Solution:** Add `disabled` attribute to button tag

---

## 🎓 Learning Path

1. **Understand the Module**
   - Read this quick reference
   - Review StudentController.cs
   - View StudentDashboardViewModel.cs

2. **Trace Data Flow**
   - Follow Dashboard view to controller
   - Follow Meals list to controller
   - Trace database queries

3. **Review Security**
   - Understand role authorization
   - Check NotFound() for invalid meals
   - Review user identity checks

4. **Plan Phase 2**
   - Design token purchase flow
   - Plan database updates
   - Test with sample data

---

## 📞 Integration Points

### For Admin Module
- Check `AdminController` for admin meal views
- Compare StudentMealViewModel vs admin ViewModels
- Note differences in computed properties

### For Wallet Module
- Review `Wallet` entity
- Check `WalletTransaction` records
- Plan wallet deduction in purchase flow

### For Future DeliveryMan Module
- Plan `MealToken` validation
- Design OTP verification
- Plan redemption flow

---

## ✅ Checklist Before Phase 2

- [ ] Review all student views
- [ ] Test navigation between pages
- [ ] Verify wallet display is correct
- [ ] Check meal filtering works
- [ ] Confirm status badges show correctly
- [ ] Validate capacity calculations
- [ ] Test on mobile/tablet/desktop
- [ ] Plan token purchase logic
- [ ] Design OTP generation
- [ ] Plan wallet deduction
- [ ] Review transaction logging

