# Hall Dining Token System - Project Overview

## Project Summary
A comprehensive ASP.NET Core 10.0 web application that manages meal token purchasing and redemption for a hall/dormitory dining system. Built with Razor Views (not Razor Pages) and uses SQL Server with Entity Framework Core.

---

## Technical Stack

### Framework & Infrastructure
- **Target Framework**: .NET 10.0
- **Web Framework**: ASP.NET Core 10.0
- **View Engine**: Razor Views (MVC) with ASP.NET Core MVC
- **Database**: SQL Server (LocalDB - SQLEXPRESS)
- **ORM**: Entity Framework Core 10.0.5
- **Authentication**: ASP.NET Core Identity with Role-Based Authorization

### Key NuGet Packages
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (10.0.5)
- `Microsoft.AspNetCore.Identity.UI` (10.0.5)
- `Microsoft.EntityFrameworkCore.SqlServer` (10.0.5)
- `Microsoft.EntityFrameworkCore.Tools` (10.0.5)
- jQuery, jQuery Validation, Bootstrap 5

---

## Project Structure

```
HallDiningTokenSystem/
├── Areas/
│   └── Identity/Pages/         # Identity UI pages (Register, Account management)
├── Controllers/                 # MVC Controllers
│   ├── AdminController.cs       # SuperAdmin meal management
│   ├── AdminSettlementController.cs  # Settlement approvals
│   ├── CashAdminController.cs   # Cash collection & wallet top-ups
│   ├── DeliveryManController.cs # Token verification & serving
│   ├── HomeController.cs        # Public pages
│   ├── ReportsController.cs     # Reporting & analytics
│   └── StudentController.cs     # Student dashboard & meal purchasing
├── Models/                      # Domain & ViewModel classes
├── Services/                    # Business logic layer
├── Data/
│   ├── ApplicationDbContext.cs  # EF Core DbContext
│   ├── DbSeeder.cs              # Database initialization & seed data
│   └── Migrations/              # EF Core migrations
├── Views/                       # Razor view templates
└── wwwroot/                     # Static assets (CSS, JavaScript, libraries)
```

---

## Core Features

### 1. **Role-Based Access Control**
Four user roles with specific permissions:

| Role | Responsibilities | Key Features |
|------|------------------|--------------|
| **SuperAdmin** | System administration, meal creation | Create/Edit meals, view all reports, approve settlements |
| **CashAdmin** | Cash collection & wallet management | Collect cash, top-up wallets, request settlements |
| **Student** | Purchase and manage meal tokens | Browse meals, buy tokens, track wallet, view tokens |
| **DeliveryMan** | Verify and serve tokens | Verify tokens, mark meals as redeemed |

### 2. **Meal Management System**
- **Meal Lifecycle**: 4 distinct phases
  1. **Upcoming Phase**: Before token sales start
  2. **Token Sale Open**: Students can purchase tokens
  3. **Meal Serving**: Tokens can be redeemed
  4. **Completed**: After meal service ends

- **Meal Properties**:
  ```csharp
  - Id
  - Name (e.g., "Breakfast", "Lunch", "Dinner")
  - Description
  - Price (decimal)
  - Capacity (max tokens)
  - SoldCount (purchased tokens)
  - TokenSellStartDateTime / TokenSellEndDateTime
  - MealServingStartDateTime / MealServingEndDateTime
  - IsArchived (soft delete flag)
  - CreatedAt (audit)
  ```

### 3. **Wallet & Transaction System**
- Students have a **Wallet** with current balance
- **WalletTransactions** track all movements (top-ups, purchases)
- CashAdmins can top-up student wallets
- Students can use wallet balance to purchase meal tokens

### 4. **Token & Order System**
- **Order**: Student purchases tokens for a meal session
- **MealToken**: Individual token with unique OTP
  - Status: "Available", "Redeemed", "Expired"
  - RedeemedBy (DeliveryMan who redeemed it)
  - RedeemedAt (timestamp)
- Each order can have multiple tokens (capacity-based)

### 5. **Cash Settlement System**
- **CashCollection**: Records cash collected from wallet top-ups
- **CashSettlementRequest**: CashAdmin requests settlement
- **CashSettlement**: SuperAdmin approves the settlement
- Full audit trail with timestamps and responsible admins

### 6. **Reporting & Analytics**
- Comprehensive reporting views for:
  - Meals (sales, attendance, inventory)
  - Students (activity, spending, token usage)
  - Wallets (balances, transaction history)
  - Settlements (pending, approved, audit trail)
  - Redemptions (token verification history)

---

## Database Schema

### Core Tables (13 total)

#### Authentication & Users
- **AspNetUsers** (extends IdentityUser)
  - FullName
  - StudentId (unique, filtered index)

- **AspNetRoles**, **AspNetUserRoles** (Identity framework)

#### Financial
- **Wallets**: User → Balance mapping
- **WalletTransactions**: Wallet movements (debit/credit)
- **CashCollections**: Cash collected by CashAdmin
- **CashSettlements**: Approved cash handovers
- **CashSettlementRequests**: Settlement approval workflow

#### Meal & Tokens
- **MealSessions**: Meal definitions with timeline
- **Orders**: Student meal purchases
- **MealTokens**: Individual tokens with OTP and redemption tracking

### Key Relationships
- User → Wallet (1:1)
- Wallet → WalletTransactions (1:N)
- MealSession → Orders (1:N)
- Order → MealTokens (1:N)
- MealToken → RedeemedBy (User) (N:1, optional)

### Constraints & Indexes
- StudentId has unique filtered index (only non-null values)
- Cascading deletes for transaction data
- Restrict deletes for foreign key references
- Named constraints for settlement audit trail

---

## Seed Data (on First Run)

### Default Users

| Email | Role | Password | Notes |
|-------|------|----------|-------|
| superadmin@test.com | SuperAdmin | Test@123 | System administration |
| cashadmin1@test.com | CashAdmin | Test@123 | Cash collection #1 |
| cashadmin2@test.com | CashAdmin | Test@123 | Cash collection #2 |
| student1@test.com | Student | Test@123 | Rahim (ID: 220130) |
| student2@test.com | Student | Test@123 | Karim (ID: 220131) |
| delivery@test.com | DeliveryMan | Test@123 | Meal service verification |

### Wallet Generation
- Wallets are automatically created for all Student users with 0 balance

---

## ViewModels

### Student Views
- **StudentDashboardViewModel**: Balance, token counts, meal stats
- **StudentMealViewModel**: Browse available meals
- **StudentMealDetailsViewModel**: Meal details with purchase option
- **MyTokensViewModel**: Student's purchased tokens with QR codes
- **WalletHistoryViewModel**: Transaction log with filters

### Admin Views
- **CreateMealViewModel**: Form for creating new meals
- **MealDetailsViewModel**: Operational metrics
- **AdminReportViewModel**: Comprehensive meal analytics
- **ServingDashboardViewModel**: Real-time serving dashboard

### Settlement Views
- **CashSettlementRequestViewModel**: Request submission
- **CashStatementViewModel**: Cash flow reports
- **SettlementViewModels**: Status and audit tracking

---

## Key Services

### MealService
```csharp
- CreateMealAsync(CreateMealViewModel)
- GetAllMealsAsync()
- GetMealByIdAsync(int id)
```
- Validates meal timeline sequences
- Prevents backdated meal creation

### WalletService
- Top-up wallet balance
- Deduct for purchases
- Generate transaction records

### SettlementService
- Request cash settlement
- Approve/reject settlements
- Audit logging

---

## Views Structure

### Public/Home
- `Home/Index.cshtml` - Landing page
- `Home/Privacy.cshtml` - Privacy policy
- `Shared/_Layout.cshtml` - Main master layout with role-based navigation

### Student Views
- `Student/Dashboard.cshtml` - Stats and quick access
- `Student/Meals.cshtml` - Meal catalog
- `Student/MealDetails.cshtml` - Purchase interface
- `Student/MyTokens.cshtml` - Token management with QR

### Admin Views
- `Admin/Dashboard.cshtml` - Operations overview
- `Admin/Meals.cshtml` - Meal inventory
- `Admin/CreateMeal.cshtml` - Meal creation form
- `Admin/EditMeal.cshtml` - Modify existing meal
- `Admin/SettlementRequests.cshtml` - Settlement approvals

### CashAdmin Views
- `CashAdmin/Dashboard.cshtml` - Cash operations
- `CashAdmin/TopUpWallet.cshtml` - Wallet management
- `CashAdmin/RequestSettlement.cshtml` - Settlement requests
- `CashAdmin/CashStatement.cshtml` - Financial reports

### Reports Views
- `Reports/Dashboard.cshtml` - Analytics hub
- `Reports/Meals.cshtml` - Meal-level metrics
- `Reports/Students.cshtml` - Student activity
- `Reports/Redemptions.cshtml` - Token verification logs
- `Reports/Settlements.cshtml` - Settlement audit trail

### DeliveryMan Views
- `DeliveryMan/Dashboard.cshtml` - Serving interface
- `DeliveryMan/VerifyToken.cshtml` - Token verification
- `DeliveryMan/VerifyTokenResult.cshtml` - Verification feedback
- `DeliveryMan/ServingHistory.cshtml` - Historical log

---

## Configuration

### Connection String
```
Server=DESKTOP-QGGP0VR\SQLEXPRESS;
Database=HallDiningTokenSystemDb;
Trusted_Connection=True;
MultipleActiveResultSets=true;
TrustServerCertificate=True
```

### Route Configuration
- MVC Default Route: `{controller=Home}/{action=Index}/{id?}`
- Razor Pages: Enabled for Identity framework

### Logging
- Default Level: Information
- ASP.NET Core: Warning

---

## Recent Migrations (EF Core)

1. **InitialIdentityWithApplicationUser** - Base schema setup
2. **AddUniqueStudentIdIndex** - StudentId uniqueness constraint
3. **UpdateApplicationUserConstraints** - Identity configuration
4. **AddPhase2Models** - Financial models (Wallet, CashAdmin features)
5. **RemoveDepartmentBatchFromApplicationUser** - Schema cleanup
6. **AddCashSettlementRequests** - Settlement workflow
7. **AddMealArchiveSupport** - Soft delete for meals
8. **RefactorMealLifecycleModel** - Improved timeline management
9. **AddRedemptionTrackingToMealToken** - Token verification audit

---

## Key Workflow Examples

### Student Purchasing a Meal
1. Student logs in → Views Dashboard
2. Navigates to "Browse Meals"
3. Selects meal in "Token Sale Open" phase
4. Wallet balance checked
5. Order created (from wallet deduction)
6. MealTokens generated with unique OTPs
7. Tokens displayed with QR codes

### Token Verification (DeliveryMan)
1. DeliveryMan scans/enters token OTP
2. System verifies:
   - Token exists
   - Status is "Available"
   - Current time is within meal serving window
3. Token marked as "Redeemed"
4. RedeemedBy/RedeemedAt set
5. Feedback displayed

### Cash Settlement Workflow
1. CashAdmin collects cash, records in system
2. CashAdmin requests settlement
3. SuperAdmin reviews pending requests
4. SuperAdmin approves → CashSettlement created
5. CashAdmin receives cash (off-system)
6. Full audit trail maintained

---

## Development Environment

- **IDE**: Visual Studio Community 2026 (18.7.0-insiders)
- **Framework**: .NET 10.0
- **Database**: SQL Server Express LocalDB
- **Source Control**: Git (GitHub - main branch)

---

## Security Features

- ✅ Role-based authorization (via [Authorize(Roles="...")])
- ✅ Identity framework with password hashing
- ✅ Null reference safety enabled (#nullable enable)
- ✅ Unique StudentId constraint with filtered index
- ✅ Foreign key constraints with appropriate delete behaviors
- ✅ Soft delete pattern for meals (IsArchived)
- ✅ Audit trail for all transactions and settlements

---

## Next Steps / Potential Enhancements

- [ ] Email notifications for settlement approvals
- [ ] SMS alerts for low wallet balance
- [ ] Advanced analytics dashboards
- [ ] Bulk meal import functionality
- [ ] Mobile app integration (API endpoints)
- [ ] Refund processing system
- [ ] Meal inventory management
- [ ] Peak hour demand forecasting

