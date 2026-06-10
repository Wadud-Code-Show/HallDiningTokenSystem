# Student Module - Component Interaction Map

## Overview Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│                   STUDENT MODULE COMPONENTS                        │
│                                                                     │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │  StudentController [Authorization: Student Role]          │   │
│  │  ├─ Dashboard()              → GET /Student/Dashboard     │   │
│  │  ├─ Meals()                  → GET /Student/Meals         │   │
│  │  └─ MealDetails(id)          → GET /Student/MealDetails/{id}
│  │                                                            │   │
│  └───────┬────────────────────────┬──────────────────────┬──┘   │
│          │                        │                      │        │
│          ▼                        ▼                      ▼        │
│   ┌──────────────────┐  ┌────────────────┐  ┌────────────────┐  │
│   │ StudentDashboard │  │ StudentMeal    │  │ StudentMeal    │  │
│   │ ViewModel        │  │ ViewModel      │  │ DetailsModel   │  │
│   │                  │  │                │  │                │  │
│   │ • Balance        │  │ • Id           │  │ • Id           │  │
│   │ • Tokens         │  │ • Name         │  │ • Name         │  │
│   │ • Meals          │  │ • Price        │  │ • Price        │  │
│   │ • Student Info   │  │ • Status       │  │ • Status       │  │
│   │                  │  │ • Capacity     │  │ • Capacity     │  │
│   │ Computed Props:  │  │ • CanPurchase  │  │ • CanPurchase  │  │
│   │ • ActiveTokens   │  │ • SoldCount    │  │ • SoldCount    │  │
│   │ • TotalMeals     │  │ • Dates        │  │ • Dates        │  │
│   │                  │  │ • Price        │  │ • Budget Info  │  │
│   └──────┬───────────┘  └────────┬───────┘  └────────┬───────┘  │
│          │                       │                   │           │
│          └───────────┬───────────┴─────────┬────────┘            │
│                      │                     │                     │
└──────────────────────┼─────────────────────┼─────────────────────┘
					   │                     │
					   ▼                     ▼
		  ┌──────────────────────┐ ┌──────────────────────┐
		  │  Views (Razor)      │ │  Views (Razor)       │
		  │                      │ │                      │
		  │  Dashboard.cshtml    │ │  Meals.cshtml        │
		  │  MealDetails.cshtml  │ │  (Table & Grid)      │
		  │                      │ │                      │
		  │  Components:         │ │  Components:         │
		  │  - Cards             │ │  - Table View        │
		  │  - Wallet Display    │ │  - Card Grid View    │
		  │  - Summaries         │ │  - Badges            │
		  │  - Quick Actions     │ │  - Progress Bars     │
		  └──────────────────────┘ └──────────────────────┘
```

---

## Data Flow Diagram

### Dashboard Flow
```
Student Clicks "Dashboard"
	   │
	   ▼
StudentController.Dashboard()
	   │
	   ├─► await _userManager.GetUserAsync(User)
	   │   └─ Get current user identity
	   │
	   ├─► await _context.Wallets
	   │                  .FirstOrDefaultAsync(...)
	   │   └─ Get user's wallet
	   │
	   ├─► await _context.MealSessions
	   │                  .Where(m => !m.IsArchived && Now < TokenSellStart)
	   │                  .CountAsync()
	   │   └─ Count upcoming meals
	   │
	   ├─► await _context.MealSessions
	   │                  .Where(m => !m.IsArchived && Now >= Start && Now <= End)
	   │                  .CountAsync()
	   │   └─ Count active meals
	   │
	   ├─► Map to StudentDashboardViewModel
	   │   ├─ CurrentBalance = wallet?.Balance
	   │   ├─ UpcomingMealsCount = count result
	   │   ├─ ActiveMealsCount = count result
	   │   ├─ TotalPurchasedTokens = 0 (future)
	   │   ├─ TotalRedeemedTokens = 0 (future)
	   │   ├─ StudentId = user.StudentId
	   │   └─ StudentName = user.FullName
	   │
	   └─► return View(viewModel)
			└─ Render Dashboard.cshtml
```

### Meals Flow
```
Student Clicks "Available Meals"
	   │
	   ▼
StudentController.Meals()
	   │
	   ├─► await _context.MealSessions
	   │             .Include(m => m.Orders)
	   │             .Where(m => !m.IsArchived && 
	   │                         Now <= m.MealServingEndDateTime)
	   │             .OrderBy(m => m.TokenSellStartDateTime)
	   │             .ToListAsync()
	   │   └─ Get operational meals, sorted
	   │
	   ├─► For each meal in results:
	   │   │
	   │   └─► Map to StudentMealViewModel
	   │       ├─ Id = meal.Id
	   │       ├─ MealName = meal.Name
	   │       ├─ Price = meal.Price
	   │       ├─ All datetime fields
	   │       ├─ Capacity = meal.Capacity
	   │       ├─ SoldCount = meal.SoldCount
	   │       │
	   │       ├─► Computed: RemainingCapacity
	   │       │   = Capacity > 0 ? Capacity - SoldCount : 0
	   │       │
	   │       ├─► Computed: CurrentStatus
	   │       │   ├─ if Now < TokenSellStart → "Upcoming"
	   │       │   └─ if Now >= Start && Now <= End → "Sale Open"
	   │       │
	   │       ├─► Computed: StatusBadgeClass
	   │       │   ├─ "Upcoming" → "bg-secondary"
	   │       │   └─ "Sale Open" → "bg-success"
	   │       │
	   │       ├─► Computed: CanPurchase
	   │       │   = Status == "Sale Open" && Remaining > 0
	   │       │
	   │       └─► Computed: IsPurchaseReady
	   │           = CanPurchase
	   │
	   └─► return View(viewModelList)
			└─ Render Meals.cshtml
			   ├─ Table view (all devices)
			   └─ Card view (desktop only)
```

### Meal Details Flow
```
Student Clicks "View Details" on meal card
	   │
	   ▼
StudentController.MealDetails(int id)
	   │
	   ├─► await _context.MealSessions
	   │             .Include(m => m.Orders)
	   │             .FirstOrDefaultAsync(m => m.Id == id)
	   │   └─ Get specific meal
	   │
	   ├─► if meal == null
	   │   └─► return NotFound()  [SECURITY]
	   │
	   ├─► var status = GetStatus(meal...)
	   │   └─ Compute current status
	   │
	   ├─► if meal.IsArchived || status == "Completed"
	   │   └─► return NotFound()  [SECURITY]
	   │       (Never show archived/completed to students)
	   │
	   ├─► Map to StudentMealDetailsViewModel
	   │   ├─ Id = meal.Id
	   │   ├─ MealName = meal.Name
	   │   ├─ Price = meal.Price
	   │   ├─ All datetime fields
	   │   ├─ Capacity = meal.Capacity
	   │   ├─ SoldCount = meal.SoldCount
	   │   │
	   │   ├─► Computed: RemainingCapacity
	   │   ├─► Computed: CurrentStatus
	   │   ├─► Computed: StatusBadgeClass
	   │   ├─► Computed: CanPurchase
	   │   └─► Computed: IsPurchaseReady
	   │
	   └─► return View(viewModel)
			└─ Render MealDetails.cshtml
			   ├─ Basic info card
			   ├─ Timeline card
			   ├─ Purchase section (with "Coming Soon" button)
			   ├─ Availability status
			   └─ Navigation buttons
```

---

## Component Interaction Detail

### StudentController ↔ Database
```
StudentController
	│
	├─ Dependencies:
	│  ├─ ApplicationDbContext _context
	│  └─ UserManager<ApplicationUser> _userManager
	│
	├─ Queries:
	│  ├─ _context.Wallets.FirstOrDefaultAsync(...)
	│  ├─ _context.MealSessions.Where(...).CountAsync()
	│  └─ _context.MealSessions.Include(...).ToListAsync()
	│
	├─ Results passed to:
	│  ├─ StudentDashboardViewModel
	│  ├─ List<StudentMealViewModel>
	│  └─ StudentMealDetailsViewModel
	│
	└─ Views rendered:
	   ├─ Dashboard.cshtml
	   ├─ Meals.cshtml
	   └─ MealDetails.cshtml
```

### ViewModel Computation Flow
```
StudentMealViewModel
	├─ Input Properties (from database):
	│  ├─ Capacity
	│  ├─ SoldCount
	│  ├─ TokenSellStartDateTime
	│  ├─ TokenSellEndDateTime
	│  └─ ...
	│
	├─ Computed Properties:
	│  │
	│  ├─ RemainingCapacity (automatic property)
	│  │  └─ Capacity > 0 ? Capacity - SoldCount : 0
	│  │
	│  ├─ CurrentStatus (calls GetStatus())
	│  │  └─ Evaluates DateTime.Now vs timestamps
	│  │     ├─ Now < Start → "Upcoming"
	│  │     └─ Now >= Start && Now <= End → "Sale Open"
	│  │
	│  ├─ StatusBadgeClass (calls GetStatusBadgeClass())
	│  │  └─ Status switch
	│  │     ├─ "Upcoming" → "bg-secondary"
	│  │     └─ "Sale Open" → "bg-success"
	│  │
	│  ├─ CanPurchase (automatic property)
	│  │  └─ CurrentStatus == "Sale Open" && RemainingCapacity > 0
	│  │
	│  └─ IsPurchaseReady (automatic property)
	│     └─ CanPurchase
	│
	└─ Used by View:
	   ├─ Access RemainingCapacity for display
	   ├─ Access CurrentStatus for badge
	   ├─ Access StatusBadgeClass for styling
	   ├─ Conditional display based on CanPurchase
	   └─ Check IsPurchaseReady for button state
```

### View ↔ ViewModel Binding
```
Meals.cshtml (Table View)
	│
	├─ Model Type: List<StudentMealViewModel>
	│
	├─ @foreach (var meal in Model)
	│  │
	│  ├─ Display: @meal.MealName
	│  ├─ Display: @meal.Price.ToString("N2")
	│  ├─ Display: @meal.TokenSellStartDateTime.ToString(...)
	│  │
	│  ├─ Badge: @meal.StatusBadgeClass
	│  │  └─ Applied as class in span
	│  │  └─ Displays @meal.CurrentStatus
	│  │
	│  ├─ Display: @meal.RemainingCapacity / @meal.Capacity
	│  │
	│  └─ Link: asp-route-id="@meal.Id"
	│     └─ Points to MealDetails(id)
	│
	└─ Result: Professional meal table with status colors
```

---

## Security Validation Layers

```
Layer 1: Role-Based Authorization
┌────────────────────────────────────┐
│ [Authorize(Roles = "Student")]     │
│ All StudentController actions      │
│ Only students can access           │
└────────────────────────────────────┘
			  ▼
Layer 2: User Identity Verification
┌────────────────────────────────────┐
│ UserManager.GetUserAsync(User)     │
│ Verify user exists and is logged in│
│ Return 401 if not authenticated    │
└────────────────────────────────────┘
			  ▼
Layer 3: Meal Visibility Rules
┌────────────────────────────────────┐
│ if (meal == null)                  │
│   return NotFound()                │
│ if (meal.IsArchived)               │
│   return NotFound()                │
│ if (status == "Completed")         │
│   return NotFound()                │
└────────────────────────────────────┘
			  ▼
Layer 4: Status Validation
┌────────────────────────────────────┐
│ Only show meals students can see   │
│ Based on current time and status   │
│ Not based on request data          │
└────────────────────────────────────┘
```

---

## Extension Points for Phase 2

```
Current State (Phase 1)
┌─────────────────────────────────────┐
│ StudentMealDetailsViewModel         │
│ ├─ CanPurchase (computed flag)     │◄─── READY
│ ├─ IsPurchaseReady (computed flag) │◄─── READY
│ └─ [More fields...]                │
└─────────────────────────────────────┘
			  │
			  │ Future: Use these flags
			  │
			  ▼
Phase 2 Extension Points
┌─────────────────────────────────────┐
│ PurchaseToken(int mealId)           │ NEW
│ ├─ Validate meal still "Sale Open"  │
│ ├─ Validate wallet balance ≥ price  │
│ ├─ Create Order                     │
│ ├─ Generate OTP & MealToken         │
│ ├─ Deduct wallet                    │
│ ├─ Create WalletTransaction         │
│ └─ Update SoldCount                 │
└─────────────────────────────────────┘
			  │
			  │ Result
			  │
			  ▼
┌─────────────────────────────────────┐
│ PurchaseConfirmation.cshtml         │ NEW
│ ├─ Show success message             │
│ ├─ Display OTP code                 │
│ ├─ Show meal details                │
│ └─ Link back to dashboard           │
└─────────────────────────────────────┘
```

---

## Performance Characteristics

### Database Queries
```
Dashboard:
  ├─ 1 query: Get wallet
  ├─ 1 query: Count upcoming meals
  └─ 1 query: Count active meals
  └─ Total: 3 queries ✅ Efficient

Meals List:
  ├─ 1 query: Get all operational meals with orders
  │          (Single query with Include)
  └─ Total: 1 query ✅ Very efficient

MealDetails:
  ├─ 1 query: Get specific meal
  └─ Total: 1 query ✅ Very efficient
```

### Computation Overhead
```
Per Meal in List:
  ├─ RemainingCapacity: Arithmetic (fast)
  ├─ CurrentStatus: DateTime comparison (fast)
  ├─ StatusBadgeClass: Switch statement (fast)
  ├─ CanPurchase: Boolean logic (fast)
  └─ IsPurchaseReady: Boolean (fast)
  └─ Total: < 1ms per item ✅ Negligible

No N+1 queries ✅
No circular references ✅
No additional DB calls in views ✅
```

---

## File Dependencies

```
StudentController.cs
├─ Uses: ApplicationDbContext
├─ Uses: UserManager<ApplicationUser>
├─ Uses: StudentDashboardViewModel
├─ Uses: StudentMealViewModel
├─ Uses: StudentMealDetailsViewModel
├─ Uses: MealDetailsViewModel (for GetMealStatus)
└─ References: MealSession, Wallet, ApplicationUser

StudentDashboardViewModel.cs
└─ No external dependencies

StudentMealViewModel.cs
└─ Self-contained logic

StudentMealDetailsViewModel.cs
└─ Self-contained logic

Dashboard.cshtml
├─ Uses: StudentDashboardViewModel
└─ Html.RouteLink for navigation

Meals.cshtml
├─ Uses: List<StudentMealViewModel>
└─ Html.RouteLink for navigation

MealDetails.cshtml
├─ Uses: StudentMealDetailsViewModel
└─ Html.RouteLink for navigation

_Layout.cshtml
├─ Checks: User.IsInRole("Student")
└─ Renders appropriate navbar items
```

---

## Testing Strategy

```
Unit Tests for ViewModels:
├─ RemainingCapacity calculation
├─ Status computation
├─ Badge class selection
├─ CanPurchase flag logic
└─ IsPurchaseReady flag logic

Integration Tests for Controller:
├─ Dashboard returns correct counts
├─ Meals filters correctly
├─ MealDetails security (NotFound tests)
├─ User identity verified
└─ Database queries efficient

End-to-End Tests:
├─ Dashboard to Meals navigation
├─ Meals to Details navigation
├─ Details back to Meals
├─ Responsive layout
└─ All links work
```

---

## Maintenance Checklist

- [ ] Monitor Dashboard query performance
- [ ] Check Meals filter accuracy
- [ ] Verify MealDetails NotFound behavior
- [ ] Test wallet display updates
- [ ] Confirm status computation accuracy
- [ ] Check responsive design regularly
- [ ] Monitor user feedback
- [ ] Plan Phase 2 enhancements

---

**Component Interaction Map** - Complete  
**Last Updated:** May 29, 2026  
**Status:** ✅ Ready for Development & Maintenance

