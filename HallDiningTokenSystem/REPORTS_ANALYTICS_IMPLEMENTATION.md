# Reports and Analytics Module - Implementation Summary

## Overview
A complete read-only reports and analytics layer has been implemented for the HallDiningTokenSystem, providing SuperAdmin users with comprehensive operational visibility across the entire system.

---

## Files Created

### 1. **HallDiningTokenSystem/Controllers/ReportsController.cs**
- **Purpose**: Core reports controller with SuperAdmin authorization
- **Authorization**: `[Authorize(Roles = "SuperAdmin")]`
- **Actions Implemented**:
  - `Dashboard()` - Summary KPIs and system metrics
  - `Meals()` - Meal revenue and participation report
  - `MealDetails(int id)` - Deep-dive meal analysis with student participation
  - `Students()` - Student analytics with wallet and spending data
  - `StudentDetails(string id)` - Individual student profile and transaction history
  - `Redemptions(filters)` - Redemption log with date/meal/student/delivery-man filters
  - `Wallets()` - Wallet summary across all students
  - `Settlements()` - CashAdmin settlement history
- **Key Features**:
  - Uses `AsNoTracking()` for read-only queries
  - Projection-based queries to minimize data transfer
  - No N+1 queries
  - Derived entirely from transactional data (no summary tables)

### 2. **HallDiningTokenSystem/ViewModels/ReportViewModels.cs**
- **Purpose**: Consolidated view models for all report pages
- **View Models**:
  - `ReportDashboardViewModel` - Dashboard KPIs
  - `MealReportViewModel` - List of meals with metrics
  - `MealDetailsReportViewModel` - Single meal deep-dive
  - `StudentReportViewModel` - Student analytics list
  - `StudentDetailsReportViewModel` - Individual student profile
  - `RedemptionReportViewModel` - Redemption log with filters
  - `WalletReportViewModel` - Wallet summary list
  - `SettlementReportViewModel` - Settlement history
- **Key Features**:
  - Computed badge classes for status visualization
  - Redemption rate calculations
  - Revenue per meal calculations
  - Future-proof structure for export integration (prepared for PDF/Excel/CSV)

### 3. Report Views (Under `HallDiningTokenSystem/Views/Reports/`)
All views use Bootstrap for responsive design and status badges.

#### **Dashboard.cshtml**
- Summary cards showing:
  - Total Students
  - Total Meals
  - Total Tokens Sold
  - Total Tokens Redeemed
  - Total Revenue (Sum of all Order.Amount)
  - System Wallet Balance
- Key metrics: Redemption Rate, Avg Revenue per Meal, Active Students
- Quick-access buttons to other report pages

#### **Meals.cshtml**
- Meal report table with:
  - Meal Name
  - Status Badge (Pending, Serving, Archived, Sale Open)
  - Price
  - Capacity
  - Sold Count
  - Remaining Capacity
  - Redeemed Count
  - Revenue (SoldCount × Price)
  - Utilization Progress Bar
  - Link to Meal Details

#### **MealDetails.cshtml**
- Meal profile section
- Token statistics (sold, redeemed, revenue)
- Sale and serving window information
- Student participation table with:
  - Student Name & ID
  - Purchase Date
  - Redeemed Status
  - Redeemed Time

#### **Students.cshtml**
- Student analytics table with:
  - Student Name & ID
  - Current Wallet Balance
  - Purchased Tokens Count
  - Redeemed Tokens Count
  - Total Spending
  - Redemption Rate
  - Link to Student Details

#### **StudentDetails.cshtml**
- Student profile section
- Wallet balances and summary cards
- Purchase history table
- Redemption history table
- Wallet transaction history

#### **Redemptions.cshtml**
- Filter form (Date Range, Meal, Student, DeliveryMan)
- Redemption count summary
- Redemption log table with:
  - Meal Name
  - Student Name & ID
  - OTP Code
  - Redeemed By (DeliveryMan)
  - Redeemed At timestamp
  - Time-ago display

#### **Wallets.cshtml**
- Wallet summary cards (Total Wallets, Total Balance, Avg Balance, Total Top-ups, Total Spending)
- Student wallet table with:
  - Student Info
  - Wallet Balance
  - Balance Status Badge (Positive/Negative/Empty)
  - Total Top-ups
  - Total Spending

#### **Settlements.cshtml**
- Settlement summary cards (Total Settlements, Total Amount)
- Settlement history table with:
  - CashAdmin Name
  - Collection Date
  - Amount Collected
  - Settlement Status
  - Settlement Date
  - Received By
  - Notes

---

## Files Modified

### **HallDiningTokenSystem/Views/Shared/_Layout.cshtml**
- Added Reports dropdown menu for SuperAdmin users
- Menu items:
  - Reports Dashboard
  - Meals Report
  - Students Report
  - Redemptions Report
  - Wallets Report
  - Settlements Report

---

## Report Hierarchy and Data Flow

### Dashboard (Entry Point)
```
Dashboard
├── Summary KPIs
├── Key Metrics
└── Quick Access Links to:
	├── Meals Report
	├── Students Report
	├── Redemptions Report
	├── Wallets Report
	└── Settlements Report
```

### Detailed Reports
```
Meals Report
├── Meal Details → Student Participation

Students Report
├── Student Details
	├── Purchase History
	├── Redemption History
	└── Wallet Transactions

Redemptions Report
├── Filterable Redemption Log
└── Filters: Date Range, Meal, Student, DeliveryMan

Wallets Report
└── Wallet Summary List

Settlements Report
└── Settlement History Timeline
```

---

## Revenue Calculation Logic

### Total System Revenue
```csharp
Revenue = Sum(Order.Amount) WHERE Status = "Completed"
```

### Revenue Per Meal
```csharp
Revenue = SoldCount × Price
		 = Count(Completed Orders for Meal) × Meal.Price
```

### Student Total Spending
```csharp
TotalSpending = Sum(Order.Amount) WHERE UserId = StudentId AND Status = "Completed"
```

---

## Student Spending Calculation

```csharp
// Queries Order table directly
CurrentWalletBalance = Wallet.Balance

PurchasedTokensCount = Count(Orders) WHERE UserId = StudentId AND Status = "Completed"

RedeemedTokensCount = Count(MealTokens) WHERE MealToken.Order.UserId = StudentId 
										   AND MealToken.Status = "Redeemed"

TotalSpending = Sum(Order.Amount) WHERE UserId = StudentId AND Status = "Completed"

RedemptionRate = RedeemedTokensCount / PurchasedTokensCount
```

---

## Redemption Reporting Logic

### Query Sources
- **MealTokens**: Primary source for redemption events
- **Orders**: Student and meal context
- **ApplicationUser**: DeliveryMan attribution
- **MealSession**: Meal name and information

### Available Filters
1. **Date Range**: `startDate` to `endDate` (filters by `MealToken.RedeemedAt`)
2. **Meal**: `mealId` (filters by `Order.MealSessionId`)
3. **Student**: `studentId` (filters by `Order.UserId`)
4. **DeliveryMan**: `deliveryManId` (filters by `MealToken.RedeemedById`)

### Displayed Information
- Meal Name (from Order.MealSession.Name)
- Student Name & ID (from Order.User)
- OTP Code (from MealToken.OtpCode)
- Redeemed By (from MealToken.RedeemedBy.FullName)
- Redeemed At (from MealToken.RedeemedAt)

---

## Performance Optimizations

1. **AsNoTracking()** - All queries use read-only mode, reducing EF Core overhead
2. **Projection** - Select only needed properties to minimize memory consumption
3. **Include/ThenInclude** - Explicit navigation loading prevents N+1 queries
4. **No Summary Tables** - Direct aggregation from transactional entities ensures real-time accuracy
5. **Efficient Filtering** - Database-level filtering before materialization

---

## Security and Access Control

### Role-Based Access
- **SuperAdmin**: Full access to all reports and all system data
- **CashAdmin**: Limited access (not yet implemented - future work)
- **Students**: No access to reports
- **DeliveryMan**: No access to reports

### Authorization
- `[Authorize(Roles = "SuperAdmin")]` on ReportsController
- All report actions inherit the authorization requirement
- No bypass mechanisms

---

## Status Badges Implementation

Bootstrap badge classes used for visual status indicators:

| Status | Badge Class | Use Case |
|--------|-------------|----------|
| Approved | `bg-success` | Settlement, Transaction |
| Rejected | `bg-danger` | Failed Action |
| Pending | `bg-warning` | Awaiting Action |
| Completed | `bg-info` | Finished Action |
| Serving | `bg-primary` | Active Meal Service |
| Sale Open | `bg-success` | Token Sale Window |
| Archived | `bg-secondary` | Inactive Meal |

---

## Future Export Integration Points

The code is structured to support export without modification to existing logic:

### Recommended Structure for Future Implementation
```csharp
// Example: Create IReportExporter interface
public interface IReportExporter
{
	Task<byte[]> ExportToPdfAsync<T>(List<T> data, string title);
	Task<byte[]> ExportToExcelAsync<T>(List<T> data, string title);
	Task<byte[]> ExportToCsvAsync<T>(List<T> data, string title);
}

// Export action in controller
public async Task<IActionResult> ExportMealsReport(string format)
{
	var meals = await GetMealsData(); // Existing query
	var exporter = _serviceProvider.GetRequiredService<IReportExporter>();

	var bytes = format switch
	{
		"pdf" => await exporter.ExportToPdfAsync(meals, "Meals Report"),
		"excel" => await exporter.ExportToExcelAsync(meals, "Meals Report"),
		"csv" => await exporter.ExportToCsvAsync(meals, "Meals Report"),
		_ => throw new ArgumentException("Invalid format")
	};

	return File(bytes, "application/octet-stream", $"meals-report.{format}");
}
```

---

## Data Source Mapping

All reports derive data from existing transactional tables:

| Report | Primary Tables | Navigation Properties |
|--------|---------------|-----------------------|
| Dashboard | Orders, MealSessions, Wallets, MealTokens | None directly |
| Meals | MealSessions | Orders, Orders.MealTokens |
| Meal Details | MealSessions, Orders, MealTokens | User, MealSession, RedeemedBy |
| Students | Users, Wallets, Orders | Wallet, Wallet.Transactions |
| Student Details | Users, Wallets, Orders | Wallet.Transactions, Orders.MealSession, MealTokens |
| Redemptions | MealTokens | Order, Order.User, Order.MealSession, RedeemedBy |
| Wallets | Wallets | User, Transactions |
| Settlements | CashSettlements | CashAdmin, ReceivedByAdmin |

---

## Query Performance Considerations

### No N+1 Queries
- All related data loaded via `Include` before materialization
- Filtering applied at database level before `.ToListAsync()`

### Aggregation Efficiency
- Sum/Count/Where operations executed in database
- Client-side calculations only for computed properties (rates, badges)

### Scalability
- Can handle thousands of records efficiently
- Projection ensures minimal memory footprint
- No lazy loading enabled; all loads explicit

---

## Testing Recommendations

### Unit Tests
1. Test revenue calculations independently
2. Test student spending aggregation
3. Test redemption filter combinations
4. Test meal status determination logic

### Integration Tests
1. Dashboard KPI accuracy with sample data
2. Meal details participation count accuracy
3. Redemption report filter effectiveness
4. Wallet balance consistency

### Performance Tests
1. Dashboard load time with 1000+ students
2. Redemptions report with large date range
3. Settlement report pagination handling

---

## Not Yet Implemented (By Design)

As per requirements:
- ✗ Excel Export
- ✗ PDF Export
- ✗ CSV Export
- ✗ Email Reports
- ✗ Scheduled Reports
- ✗ Background Jobs
- ✗ CashAdmin Limited Access View

These are intentionally deferred and can be added later using the exported interface pattern described above.

---

## Deployment Notes

1. Ensure SuperAdmin role exists and users are assigned
2. No database migrations needed - reports query existing tables
3. No configuration changes required
4. Reports views inherit site styling automatically from Bootstrap

---

## Summary Statistics

- **Total Files Created**: 10 (1 Controller + 1 ViewModels + 8 Views)
- **Total Files Modified**: 1 (_Layout.cshtml)
- **Report Actions**: 8
- **View Models**: 8
- **Report Pages**: 8
- **Database Tables Queried**: 9 (Users, Wallets, Orders, MealTokens, MealSessions, WalletTransactions, CashCollections, CashSettlements, CashSettlementRequests)
- **Performance Optimization**: AsNoTracking() on all queries
- **Security Level**: SuperAdmin authorization enforced
