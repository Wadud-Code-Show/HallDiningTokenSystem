# 🚀 QUICK START - Student Module Next Steps

**Last Updated:** May 29, 2026  
**Status:** ✅ Ready to Begin Phase 2  

---

## 👋 Welcome!

The Student Foundation Module is complete and ready for extension. This document gets you started on Phase 2 (Token Purchase).

---

## ⚡ 5-Minute Overview

### What Exists Now
✅ Student can view dashboard with wallet balance  
✅ Student can browse available meals  
✅ Student can view meal details  
✅ Navigation is role-based  

### What's Missing (Phase 2)
❌ Token purchase functionality  
❌ OTP generation  
❌ Wallet deduction  
❌ Order creation  

### Your Job
Enable the **"Buy Token (Coming Soon)"** button and implement purchase flow.

---

## 🎯 Quick Setup

### 1. Build and Run
```powershell
cd D:\Dotnet Core Projects\HallDiningTokenSystem
dotnet run
```

### 2. Test Login
- Create a Student account or use existing test account
- Navigate to `/Student/Dashboard`
- You should see:
  - Wallet balance
  - Meal counts
  - Quick action buttons

### 3. Browse Meals
- Click "View Available Meals"
- See list of operational meals
- Click "View Details" on any meal

### 4. View Meal Details
- See full meal information
- See "Buy Token (Coming Soon)" button
- Button is disabled
- This is what you'll enable

---

## 📁 Key Files to Know

### Student Controller
**File:** `Controllers/StudentController.cs`

Contains three methods you'll extend:
```csharp
public async Task<IActionResult> Dashboard()
public async Task<IActionResult> Meals()
public async Task<IActionResult> MealDetails(int id)
```

**Add here:** `PurchaseToken(int mealId)` method for Phase 2

### Meal Details View
**File:** `Views/Student/MealDetails.cshtml`

Find: "Buy Token (Coming Soon)" button

Currently:
```html
<button class="btn btn-primary btn-lg" disabled>
	<i class="bi bi-cart-plus"></i> Buy Token (Coming Soon)
</button>
```

Change to:
```html
<form method="post" asp-action="PurchaseToken">
	<input type="hidden" name="mealId" value="@Model?.Id" />
	<button type="submit" class="btn btn-primary btn-lg">
		<i class="bi bi-cart-plus"></i> Buy Token
	</button>
</form>
```

### ViewModels
**Files:**
- `ViewModels/StudentMealDetailsViewModel.cs`
- Use: `CanPurchase` and `IsPurchaseReady` flags

These flags are already computed. Use them for validation!

---

## 🛠️ Implementation Checklist

### Step 1: Enable Button (15 minutes)
- [ ] Open `Views/Student/MealDetails.cshtml`
- [ ] Find the "Buy Token" button
- [ ] Replace disabled button with form
- [ ] Test: Button should be clickable now
- [ ] Test: Form should POST to `/Student/PurchaseToken`

### Step 2: Create Purchase Action (1 hour)
- [ ] Add `PurchaseToken(int mealId)` to StudentController
- [ ] Validate meal exists
- [ ] Validate meal is in "Sale Open" status
- [ ] Check wallet balance
- [ ] Return error if insufficient funds
- [ ] Continue to step 3 if valid

### Step 3: Create Order (30 minutes)
- [ ] Create Order object
- [ ] Set: StudentId (from UserManager)
- [ ] Set: MealSessionId
- [ ] Set: Amount (from meal price)
- [ ] Set: Status = "Active"
- [ ] Save to database

### Step 4: Generate OTP (30 minutes)
- [ ] Create GenerateOTP() method
- [ ] Generate 6-8 digit random code
- [ ] Store in MealToken
- [ ] Return to user

### Step 5: Create MealToken (30 minutes)
- [ ] Create MealToken object
- [ ] Link to Order
- [ ] Set OTP code
- [ ] Set Status = "Active"
- [ ] Set ExpiresAt = TokenSellEndDateTime
- [ ] Save to database

### Step 6: Deduct Wallet (15 minutes)
- [ ] Load wallet from database
- [ ] Subtract price
- [ ] Update wallet.Balance
- [ ] Save to database

### Step 7: Create Transaction (15 minutes)
- [ ] Create WalletTransaction object
- [ ] Set Type = "Debit"
- [ ] Set Amount = price
- [ ] Set Description = "Token purchase for {meal name}"
- [ ] Save to database

### Step 8: Update Sold Count (10 minutes)
- [ ] Increment MealSession.SoldCount
- [ ] Save to database

### Step 9: Create Confirmation (1 hour)
- [ ] Create PurchaseConfirmation.cshtml view
- [ ] Display success message
- [ ] Show OTP code prominently
- [ ] Show meal details
- [ ] Add "Back to Dashboard" button

### Step 10: Error Handling (30 minutes)
- [ ] Use transactions for atomicity
- [ ] Handle insufficient balance
- [ ] Handle meal no longer available
- [ ] Show user-friendly error messages
- [ ] Log errors

**Total Estimated Time:** 5-6 hours

---

## 📚 Reference Documentation

**Quick Reference:** `STUDENT_MODULE_QUICK_REFERENCE.md`
- Code snippets ready to use
- Phase 2 implementation section
- Common issues & fixes

**Architecture:** `STUDENT_MODULE_COMPONENT_INTERACTION.md`
- Data flow diagrams
- How components interact
- Extension points

**Implementation Details:** `STUDENT_FOUNDATION_MODULE_REPORT.md`
- Architecture decisions
- Validation rules
- Database relationships

---

## 🔑 Key Code Patterns Already in Place

### Get Current User
```csharp
var currentUser = await _userManager.GetUserAsync(User);
```

### Check Meal Status
```csharp
var status = StudentMealDetailsViewModel.GetMealStatus(
	meal.TokenSellStartDateTime,
	meal.TokenSellEndDateTime,
	meal.MealServingStartDateTime,
	meal.MealServingEndDateTime,
	meal.IsArchived);
```

### Check Wallet Balance
```csharp
var wallet = await _context.Wallets
	.FirstOrDefaultAsync(w => w.UserId == currentUser.Id);

if (wallet == null || wallet.Balance < meal.Price)
{
	// Insufficient balance
}
```

---

## 🧪 Testing Your Code

### Unit Test Example
```csharp
[Fact]
public async Task PurchaseToken_WithSufficientBalance_CreatesOrder()
{
	// Arrange
	var testMeal = new MealSession { /* setup */ };
	var testUser = new ApplicationUser { /* setup */ };
	var testWallet = new Wallet { UserId = testUser.Id, Balance = 1000m };

	// Act
	var result = await controller.PurchaseToken(testMeal.Id);

	// Assert
	var order = await _context.Orders.FirstAsync();
	Assert.NotNull(order);
	Assert.Equal(testMeal.Id, order.MealSessionId);
}
```

### Integration Test Example
```csharp
[Fact]
public async Task PurchaseToken_InsufficientBalance_ReturnsError()
{
	// Arrange
	var testMeal = new MealSession { Price = 1000m };
	var testWallet = new Wallet { Balance = 10m }; // Not enough!

	// Act
	var result = await controller.PurchaseToken(testMeal.Id);

	// Assert
	Assert.Contains("Insufficient", TempData["Error"].ToString());
}
```

---

## ⚠️ Common Issues (Already Documented)

1. **"Meal status shows wrong"**
   - Use `DateTime.Now` (not fixed date)
   - Check sequence: TokenStart < TokenEnd < MealStart < MealEnd

2. **"Wallet balance shows wrong"**
   - Ensure wallet is loaded for current user
   - Check nullable: `wallet?.Balance ?? 0`

3. **"Transaction not atomic"**
   - Use database transaction
   - Rollback on any error

4. **"NullReferenceException"**
   - Check: user, meal, wallet not null
   - Use null coalescing: `??`

---

## 🔗 Database Tables (Already Exist)

### MealSession
```
Id, Name, Price, Capacity, SoldCount,
TokenSellStartDateTime, TokenSellEndDateTime,
MealServingStartDateTime, MealServingEndDateTime,
IsArchived, CreatedAt
```

### Order (Empty - Create records here)
```
Id, StudentId (FK), MealSessionId (FK), Amount, Status, CreatedAt
```

### MealToken (Empty - Create records here)
```
Id, OrderId (FK), OtpCode, Status, CreatedAt, ExpiresAt, RedeemedAt
```

### Wallet (Has data - Use for balance)
```
Id, UserId (FK), Balance
```

### WalletTransaction (Has records - Create here)
```
Id, WalletId (FK), Type, Amount, TransactionDate, Description
```

---

## 🎯 Success Criteria for Phase 2

- [ ] "Buy Token" button is enabled
- [ ] Clicking button shows confirmation
- [ ] OTP code is displayed
- [ ] Wallet balance decreases
- [ ] Order is created in database
- [ ] MealToken is created with OTP
- [ ] WalletTransaction is logged
- [ ] SoldCount is incremented
- [ ] Error handling works
- [ ] Transactions are atomic
- [ ] Tests pass
- [ ] Code reviewed
- [ ] Deployed to staging

---

## 🚀 Getting Started NOW

### Option A: Start with Database (Recommended)
1. Review existing tables
2. Plan how data flows
3. Draw schema diagram
4. Then code purchase flow

### Option B: Start with Controller
1. Write PurchaseToken() shell
2. Add validation logic
3. Add error handling
4. Then implement database operations

### Option C: Start with UI
1. Enable the button
2. Test form posts
3. Create confirmation view
4. Then implement backend

---

## 📞 Need Help?

**Documentation Available:**
- `DOCUMENTATION_INDEX.md` - Find what you need
- `STUDENT_MODULE_QUICK_REFERENCE.md` - Code snippets
- `STUDENT_FOUNDATION_MODULE_REPORT.md` - Architecture
- `STUDENT_MODULE_COMPONENT_INTERACTION.md` - Data flow

**Code Examples Ready:**
- Extension points identified
- Test examples provided
- Error handling templates
- Security patterns shown

---

## ✅ You're Ready!

You have:
✅ Complete documentation  
✅ Code examples  
✅ Architecture diagrams  
✅ Database schema  
✅ Test examples  
✅ Error handling patterns  
✅ Security guidelines  

**Everything you need to implement Phase 2 is ready.**

---

## 🎓 Learning Path

1. Read `STUDENT_MODULE_QUICK_REFERENCE.md` (20 min)
2. Study `STUDENT_MODULE_COMPONENT_INTERACTION.md` (25 min)
3. Review `STUDENT_FOUNDATION_MODULE_REPORT.md` (30 min)
4. Look at code in StudentController.cs (15 min)
5. Start implementing! (5-6 hours)

---

## 🏁 Final Notes

### What Makes This Easy
- Clear code to extend
- No technical debt
- Good documentation
- Architecture is sound
- Security is built in

### What Makes This Safe
- Role-based authorization
- Input validation ready
- Database transactions planned
- Error handling patterns provided
- Test examples included

### What Makes This Fast
- Code snippets provided
- Architecture documented
- Extension points clear
- Database schema known
- Integration points identified

---

**You're ready to build Phase 2!**

Start with documentation index, choose your reading path, then implement.

Good luck! 🚀

---

**Quick Start Guide** - Your launchpad for Phase 2  
**Date:** May 29, 2026  
**Status:** ✅ READY TO IMPLEMENT

