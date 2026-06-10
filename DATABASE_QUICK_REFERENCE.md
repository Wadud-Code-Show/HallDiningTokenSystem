# Hall Dining Token System - Data Model Quick Reference

## 📊 Table Summary

### 1. **ApplicationUser** (ASP.NET Core Identity)
**Role:** Core user entity for authentication & authorization
```
Columns: Id, Email, UserName, FullName, StudentId, PasswordHash, ... Identity fields
Primary Key: Id
Roles: SuperAdmin, CashAdmin, Student, DeliveryMan
```

---

### 2. **MealSession**
**Role:** Defines meal offerings with purchase & serving windows
```
Columns:
  - Id (PK)
  - Name (e.g., "Breakfast")
  - Description
  - Price (token cost)
  - Capacity (max tokens, 0=unlimited)
  - TokenSellStartDateTime (purchase window start)
  - TokenSellEndDateTime (purchase window end)
  - MealServingStartDateTime (service window start)
  - MealServingEndDateTime (service window end)
  - IsArchived (soft delete)
  - CreatedAt
  - SoldCount (# tokens sold)

Status (Computed, not stored):
  - Archived (IsArchived=true)
  - Upcoming (now < TokenSellStart)
  - Sale Open (TokenSellStart < now < TokenSellEnd)
  - Sale Closed (TokenSellEnd < now < MealServingStart)
  - Serving (MealServingStart < now < MealServingEnd)
  - Completed (now > MealServingEnd)
```

---

### 3. **Order**
**Role:** Tracks student's meal token purchase
```
Columns:
  - Id (PK)
  - UserId (FK → ApplicationUser)
  - MealSessionId (FK → MealSession)
  - Amount (price paid from wallet)
  - Status (Pending, Completed, Cancelled)
  - OrderDate (when purchased)
  - CompletedAt (when token redeemed)

One Order = One MealToken
Purchase allowed only during TokenSellStart → TokenSellEnd
```

---

### 4. **MealToken**
**Role:** Individual token issued to student for meal redemption
```
Columns:
  - Id (PK)
  - OrderId (FK → Order, unique)
  - OtpCode (unique QR/OTP code for scanning)
  - Status (Active, Redeemed, Expired)
  - CreatedAt (when issued)
  - RedeemedAt (when scanned at meal service)
  - ExpiresAt (auto-expire timestamp)

Generated automatically when Order created
Redeemed during MealServingStart → MealServingEnd window
Auto-expires after MealServingEnd if not redeemed
```

---

### 5. **Wallet**
**Role:** Student's account for meal token purchase balance
```
Columns:
  - Id (PK)
  - UserId (FK → ApplicationUser, UNIQUE)
  - Balance (current account balance)
  - CreatedAt (wallet creation)
  - UpdatedAt (last balance change)

One per student, created at registration
Balance decreases on token purchase (Debit)
Balance increases on cash collection (Credit)
Cannot go negative (validation enforced)
All changes tracked in WalletTransaction
```

---

### 6. **WalletTransaction**
**Role:** Immutable audit log of wallet balance changes
```
Columns:
  - Id (PK)
  - WalletId (FK → Wallet)
  - Amount (transaction amount)
  - TransactionType (Credit or Debit)
  - Description (reason for transaction)
  - CreatedAt (transaction timestamp)

Every balance change creates one transaction
Immutable (never updated)
Transaction Type:
  - Debit: Token purchase, fees, penalties
  - Credit: Cash collection top-up, refunds
```

---

### 7. **CashCollection**
**Role:** Records physical cash collected from student
```
Columns:
  - Id (PK)
  - CashAdminId (FK → ApplicationUser)
  - StudentId (FK → ApplicationUser)
  - Amount (cash collected)
  - WalletTransactionId (FK → WalletTransaction, optional)
  - CreatedAt (collection timestamp)

Created when CashAdmin collects cash from Student
Auto-links to WalletTransaction (Credit) for audit trail
Enables tracking: "Which student paid cash, which admin collected"
CashAdmin accumulates collections for settlement
```

---

### 8. **CashSettlementRequest**
**Role:** CashAdmin's request to be paid for accumulated collections
```
Columns:
  - Id (PK)
  - CashAdminId (FK → ApplicationUser)
  - Amount (total collected amount requesting)
  - Status (Pending, Approved, Rejected)
  - RequestNote (CashAdmin's message)
  - RequestedAt (when created)
  - ReviewedByAdminId (FK → ApplicationUser, nullable)
  - ReviewedAt (when reviewed)
  - AdminNote (SuperAdmin's approval/rejection reason)
  - CashSettlementId (FK → CashSettlement, optional)

Creates workflow:
  1. CashAdmin creates request (Status=Pending)
  2. SuperAdmin reviews (Status=Approved/Rejected)
  3. If approved → CashSettlement created
  4. CashAdmin receives payment

Enables approval workflow & financial control
```

---

### 9. **CashSettlement**
**Role:** Records actual payment to CashAdmin
```
Columns:
  - Id (PK)
  - CashAdminId (FK → ApplicationUser)
  - Amount (payment amount)
  - ReceivedByAdminId (FK → ApplicationUser SuperAdmin)
  - Note (payment reference)
  - SettlementDate (when paid)

Only created after CashSettlementRequest approved
Immutable payment record
Audit trail of who authorized (ReceivedByAdminId)
Used for cash reconciliation & financial reporting
```

---

## 🔗 Relationship Summary

```
ApplicationUser ─┬─→ Orders (1:N)
				 ├─→ Wallets (1:1 unique)
				 ├─→ CashCollections (1:N) as CashAdmin
				 ├─→ CashCollections (1:N) as Student
				 ├─→ CashSettlements (1:N) as CashAdmin
				 ├─→ CashSettlements (1:N) as ReceivedByAdmin
				 ├─→ CashSettlementRequests (1:N) as CashAdmin
				 └─→ CashSettlementRequests (1:N) as ReviewedByAdmin

MealSession ─→ Orders (1:N)

Order ─┬─→ ApplicationUser (N:1)
	   ├─→ MealSession (N:1)
	   └─→ MealTokens (1:N)

MealToken ─→ Order (N:1)

Wallet ─┬─→ ApplicationUser (1:1)
		└─→ WalletTransactions (1:N)

WalletTransaction ─┬─→ Wallet (N:1)
				   └─← CashCollection (optional)

CashCollection ─┬─→ ApplicationUser CashAdmin (N:1)
				├─→ ApplicationUser Student (N:1)
				└─→ WalletTransaction (N:1 optional)

CashSettlementRequest ─┬─→ ApplicationUser CashAdmin (N:1)
					   ├─→ ApplicationUser ReviewedByAdmin (N:1 opt)
					   └─→ CashSettlement (1:1 optional)

CashSettlement ─┬─→ ApplicationUser CashAdmin (N:1)
				└─→ ApplicationUser ReceivedByAdmin (N:1)
```

---

## 💼 Business Process Flows

### Flow 1: Student Buys Meal Token
```
1. Student views available meals in "Sale Open" status
2. Student selects meal and clicks "Buy Token"
3. System validates: Wallet balance ≥ meal price
4. Creates Order → Debit WalletTransaction → Creates MealToken
5. Student receives OTP code (QR/SMS)
```

**Data Changes:**
- Order: inserted
- MealToken: inserted
- WalletTransaction: inserted
- Wallet: balance -amount
- MealSession: SoldCount +1

---

### Flow 2: Student Redeems Token at Meal
```
1. DeliveryMan scans student's OTP code at meal service
2. System validates: Token status = "Active", in serving window
3. System marks MealToken as "Redeemed"
4. DeliveryMan serves meal to student
```

**Data Changes:**
- MealToken: status → Redeemed, RedeemedAt = now
- Order: status → Completed, CompletedAt = now

---

### Flow 3: CashAdmin Collects Cash
```
1. CashAdmin meets student and collects physical cash
2. CashAdmin records collection in system
3. System auto-credits student's wallet
4. Creates CashCollection + WalletTransaction (Credit)
```

**Data Changes:**
- CashCollection: inserted
- WalletTransaction: inserted (Credit)
- Wallet: balance +amount

---

### Flow 4: CashAdmin Requests Settlement
```
1. CashAdmin submits CashSettlementRequest with total collected
2. SuperAdmin receives pending request
3. SuperAdmin reviews and approves/rejects
4. If approved: Creates CashSettlement (payment authorization)
5. CashAdmin receives actual payment (outside system)
```

**Data Changes:**
- CashSettlementRequest: inserted → updated (approval)
- CashSettlement: inserted (if approved)

---

## 📈 Data Volume Estimates

For a mid-sized hall (500 students):
```
ApplicationUser: ~500+ (students + admins + delivery staff)
MealSession: ~100-200 per semester (3 meals/day × semester length)
Order: ~3000-5000 per semester (6-10 orders per student avg)
MealToken: ~3000-5000 (1:1 with Order)
Wallet: ~500 (1 per student)
WalletTransaction: ~5000-10000 (2+ per student on avg)
CashCollection: ~500-1000 (sporadic, 1-2 per student)
CashSettlementRequest: ~20-50 (per semester)
CashSettlement: ~15-30 (approved requests)
```

---

## 🔐 Security & Role-Based Access

```
SuperAdmin:
  ✓ Create/Edit/Archive MealSessions
  ✓ View all Orders & Tokens
  ✓ Review CashSettlementRequests
  ✓ Generate Reports
  ✗ Cannot place Orders
  ✗ Cannot request Settlements

CashAdmin:
  ✓ Record CashCollections
  ✓ Submit CashSettlementRequests
  ✓ View own collected amounts
  ✗ Cannot create MealSessions
  ✗ Cannot redeem Tokens
  ✗ Cannot approve Settlements

Student:
  ✓ View available MealSessions
  ✓ Purchase meal Tokens
  ✓ View own Orders & Wallet balance
  ✓ View OTP code for Token
  ✗ Cannot create MealSessions
  ✗ Cannot redeem Tokens (DeliveryMan does)
  ✗ Cannot approve anything

DeliveryMan:
  ✓ Redeem (scan) MealTokens
  ✓ Mark Orders as Completed
  ✗ Cannot create Orders
  ✗ Cannot modify Wallets
  ✗ Cannot handle cash
```

---

## 🚨 Critical Validations

```
MealSession:
  - TokenSellStart < TokenSellEnd < MealServingStart < MealServingEnd
  - All times must be in future (at creation)
  - Price must be positive
  - Capacity must be 0 or positive
  - Once TokenSellStart reached → Price immutable
  - Once SoldCount > 0 → TokenSellStart immutable

Order:
  - Wallet balance ≥ amount
  - Ordered during TokenSellStart ≤ Now ≤ TokenSellEnd
  - One order per student per meal (prevent duplicates)
  - UserId must have "Student" role

MealToken:
  - OtpCode unique
  - Status transitions: Active → Redeemed OR Active → Expired
  - RedeemedAt can only be set once
  - Redemption only in MealServingStart ≤ Now ≤ MealServingEnd

Wallet:
  - Balance cannot go negative
  - Balance precision: 2 decimal places (currency)
  - UserId unique (one per student)

CashCollection:
  - Amount positive
  - CashAdminId has "CashAdmin" role
  - StudentId has "Student" role
  - Creates WalletTransaction automatically

CashSettlementRequest:
  - Amount must match total collected
  - Status workflow: Pending → (Approved→Rejected OR Approved)
  - ReviewedByAdminId must be SuperAdmin

CashSettlement:
  - Can only be created if request Approved
  - Amount must match request amount
  - ReceivedByAdminId must be SuperAdmin
```

---

## 📝 Field Data Types & Sizes

```
String fields (recommend VARCHAR(255) unless noted):
  Name, Description, StudentId, Email, UserName, FullName - 255
  Status, TransactionType - 50
  OtpCode - 50 (unique)
  RequestNote, AdminNote, Note - 500 (text areas)

Decimal fields (DECIMAL(18,2) for currency):
  Price, Amount, Balance

DateTime fields (DATETIME2):
  TokenSellStartDateTime, TokenSellEndDateTime
  MealServingStartDateTime, MealServingEndDateTime
  CreatedAt, UpdatedAt, OrderDate, CompletedAt
  RedeemedAt, ExpiresAt, SettlementDate
  RequestedAt, ReviewedAt

Integer fields (INT):
  Id, Capacity, SoldCount (all FK fields)

Boolean fields (BIT):
  IsArchived
```

---

## 📊 Report Templates

**Meal Summary Report:**
```
MealSession: Id, Name, Price, Capacity, SoldCount
  Status, TokenSellStart, MealServingEnd, IsArchived
```

**Student Order Report:**
```
Order: Id, StudentName, MealName, Amount, Status, OrderDate
  TokenStatus, Order.Status, CompletedAt
```

**CashAdmin Performance Report:**
```
CashAdmin: Name, TotalCollected, SettlementRequests
  ApprovedAmount, PendingAmount, RejectedAmount
```

**Financial Reconciliation:**
```
Period: Date range
  TotalOrders (count), TotalRevenue, SettlementsApproved
  SettlementsPending, CashOnHand
```

