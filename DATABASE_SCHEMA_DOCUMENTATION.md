# Hall Dining Token System - Complete Database Schema Documentation

## Overview
The Hall Dining Token System is built on a comprehensive database architecture that manages meal sessions, student dining tokens, wallet transactions, and cash settlements. The system follows a multi-role architecture with SuperAdmin, CashAdmin, Student, and DeliveryMan roles.

---

## 📊 Database Tables & Relationships

### 1. **ApplicationUser** (Identity Framework)
**Purpose:** Core user entity extending ASP.NET Core Identity

**Columns:**
- `Id` (Primary Key) - Unique user identifier
- `Email` - User email address
- `UserName` - Login username
- `FullName` - User's full name
- `StudentId` - Student registration number (nullable)
- Plus inherited Identity fields: `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`, `PhoneNumber`, `TwoFactorEnabled`, etc.

**Relationships:**
- ✅ One-to-Many: ApplicationUser → Orders (User placed orders)
- ✅ One-to-Many: ApplicationUser → Wallets (User has one wallet, but stored as collection for flexibility)
- ✅ One-to-Many: ApplicationUser → CashCollections (both as CashAdmin and Student)
- ✅ One-to-Many: ApplicationUser → CashSettlements (both as CashAdmin and ReceivedByAdmin)
- ✅ One-to-Many: ApplicationUser → CashSettlementRequests (both as CashAdmin and ReviewedByAdmin)

**Roles:**
- `SuperAdmin` - System administrator, manages meals, reviews settlement requests
- `CashAdmin` - Collects cash from students, requests settlements
- `Student` - Purchases meal tokens, maintains wallet
- `DeliveryMan` - Delivers meals and redeems tokens

---

### 2. **MealSession**
**Purpose:** Represents a meal offering with defined purchase and serving windows

**Columns:**
- `Id` (Primary Key)
- `Name` (string) - Meal name (e.g., "Breakfast", "Lunch", "Dinner")
- `Description` (string, nullable) - Meal details/ingredients
- `Price` (decimal) - Token price
- `Capacity` (int) - Maximum tokens available (0 = unlimited)
- `TokenSellStartDateTime` (DateTime) - When token purchase begins
- `TokenSellEndDateTime` (DateTime) - When token purchase ends
- `MealServingStartDateTime` (DateTime) - When meal begins serving
- `MealServingEndDateTime` (DateTime) - When meal stops serving
- `IsArchived` (bool) - Soft delete flag for historical tracking
- `CreatedAt` (DateTime) - Record creation timestamp
- `SoldCount` (int) - Number of tokens sold

**Key Characteristics:**
- Two-phase meal lifecycle:
  1. **Token Purchase Phase** (TokenSellStart → TokenSellEnd)
  2. **Meal Serving Phase** (MealServingStart → MealServingEnd)
- Dynamic status computed from timestamps and IsArchived flag
  - `Archived` - Soft deleted
  - `Upcoming` - Current time before TokenSellStart
  - `Sale Open` - Current time between TokenSellStart and TokenSellEnd
  - `Sale Closed` - Current time between TokenSellEnd and MealServingStart
  - `Serving` - Current time between MealServingStart and MealServingEnd
  - `Completed` - Current time after MealServingEnd

**Relationships:**
- ✅ One-to-Many: MealSession → Orders (Students can place multiple orders for same meal)

---

### 3. **Order**
**Purpose:** Records token purchase by a student for a specific meal

**Columns:**
- `Id` (Primary Key)
- `UserId` (Foreign Key) - Reference to ApplicationUser (student)
- `MealSessionId` (Foreign Key) - Reference to MealSession
- `Amount` (decimal) - Amount deducted from wallet
- `Status` (string) - Order status
  - `Pending` - Order created, awaiting redemption
  - `Completed` - Token redeemed at meal service
  - `Cancelled` - Order cancelled
- `OrderDate` (DateTime) - When order was placed
- `CompletedAt` (DateTime, nullable) - When token was redeemed

**Relationships:**
- ✅ Many-to-One: Order → ApplicationUser (User placed order)
- ✅ Many-to-One: Order → MealSession (Meal being ordered)
- ✅ One-to-Many: Order → MealTokens (Order can have multiple tokens for redemption)

**Business Logic:**
- Only students can place orders
- Order can only be placed during TokenSellStart → TokenSellEnd window
- Amount must be available in student's wallet
- One order = one meal token (not multiple)

---

### 4. **MealToken**
**Purpose:** Individual token generated from an order, redeemed at meal service

**Columns:**
- `Id` (Primary Key)
- `OrderId` (Foreign Key) - Reference to Order
- `OtpCode` (string) - QR/OTP code for redemption (e.g., unique code for each token)
- `Status` (string) - Token lifecycle status
  - `Active` - Token available for redemption
  - `Redeemed` - Token used at meal service
  - `Expired` - Token not redeemed before MealServingEndDateTime
- `CreatedAt` (DateTime) - Token creation time
- `RedeemedAt` (DateTime, nullable) - When token was redeemed
- `ExpiresAt` (DateTime, nullable) - Auto-expiry time

**Relationships:**
- ✅ Many-to-One: MealToken → Order (Token belongs to one order)

**Business Logic:**
- One order generates exactly one MealToken
- Token is redeemed during MealServingStart → MealServingEnd window
- OTP code is used by DeliveryMan to redeem at physical meal distribution
- Tokens automatically expire after MealServingEndDateTime

**Future Enhancement:**
- Could be expanded for multi-item orders (e.g., 5 lunch tokens)

---

### 5. **Wallet**
**Purpose:** Stores student's account balance for meal token purchases

**Columns:**
- `Id` (Primary Key)
- `UserId` (Foreign Key) - Reference to ApplicationUser (student)
- `Balance` (decimal) - Current account balance
- `CreatedAt` (DateTime) - Wallet creation timestamp
- `UpdatedAt` (DateTime, nullable) - Last balance update

**Relationships:**
- ✅ Many-to-One: Wallet → ApplicationUser (Student owns wallet)
- ✅ One-to-Many: Wallet → WalletTransactions (All transactions for wallet)

**Business Logic:**
- Created automatically when student registers
- Balance decreases when student purchases meal token
- Balance increases when CashAdmin collects payment (Cash → Digital)
- Should maintain audit trail via WalletTransactions

---

### 6. **WalletTransaction**
**Purpose:** Audit log for all wallet balance changes (Credit/Debit)

**Columns:**
- `Id` (Primary Key)
- `WalletId` (Foreign Key) - Reference to Wallet
- `Amount` (decimal) - Transaction amount
- `TransactionType` (string) - `Credit` or `Debit`
  - `Credit` - Money added to wallet (e.g., CashAdmin top-up)
  - `Debit` - Money deducted from wallet (e.g., meal token purchase)
- `Description` (string) - Transaction reason
  - Examples: "Meal token purchase", "Cash collection from CashAdmin", "Refund"
- `CreatedAt` (DateTime) - Transaction timestamp

**Relationships:**
- ✅ Many-to-One: WalletTransaction → Wallet (Transaction belongs to wallet)
- ✅ Optional: WalletTransaction ← CashCollection (CashCollection can reference transaction)

**Business Logic:**
- Every balance change must create a transaction record
- Immutable audit trail - transactions cannot be modified
- Enables complete financial history and reconciliation
- Used for generating statements and reports

---

### 7. **CashCollection**
**Purpose:** Records physical cash collected from students by CashAdmin

**Columns:**
- `Id` (Primary Key)
- `CashAdminId` (Foreign Key) - Reference to ApplicationUser (CashAdmin role)
- `StudentId` (Foreign Key) - Reference to ApplicationUser (Student role)
- `Amount` (decimal) - Cash amount collected
- `WalletTransactionId` (Foreign Key, nullable) - Link to wallet credit transaction
- `CreatedAt` (DateTime) - Collection timestamp

**Relationships:**
- ✅ Many-to-One: CashCollection → ApplicationUser (CashAdmin)
- ✅ Many-to-One: CashCollection → ApplicationUser (Student)
- ✅ Optional Many-to-One: CashCollection → WalletTransaction (For audit trail)

**Business Flow:**
1. CashAdmin collects physical cash from Student
2. CashAdmin records collection in system
3. System creates WalletTransaction (Credit) to student's wallet
4. CashCollection links to that transaction for accountability
5. CashAdmin accumulates total collections
6. CashAdmin later requests settlement of accumulated cash

**Key Insight:**
- Separates physical cash flow from digital wallet flow
- Enables tracking which students paid cash vs digital
- Accountability: ties cash to specific CashAdmin and Student

---

### 8. **CashSettlementRequest**
**Purpose:** CashAdmin requests settlement (payment) of collected cash from SuperAdmin

**Columns:**
- `Id` (Primary Key)
- `CashAdminId` (Foreign Key) - CashAdmin requesting settlement
- `Amount` (decimal) - Total amount requesting
- `Status` (string) - Request workflow status
  - `Pending` - Awaiting SuperAdmin review
  - `Approved` - SuperAdmin approved, settlement created
  - `Rejected` - SuperAdmin rejected request
- `RequestNote` (string, nullable) - CashAdmin's note (e.g., "Collections from week 1")
- `RequestedAt` (DateTime) - When request was made
- `ReviewedByAdminId` (Foreign Key, nullable) - SuperAdmin who reviewed
- `ReviewedAt` (DateTime, nullable) - When reviewed
- `AdminNote` (string, nullable) - SuperAdmin's approval/rejection reason
- `CashSettlementId` (Foreign Key, nullable) - Link to actual settlement record

**Relationships:**
- ✅ Many-to-One: CashSettlementRequest → ApplicationUser (CashAdmin)
- ✅ Optional Many-to-One: CashSettlementRequest → ApplicationUser (ReviewedByAdmin)
- ✅ One-to-One: CashSettlementRequest → CashSettlement (Optional)

**Business Flow:**
1. CashAdmin collects cash from students over time
2. CashAdmin creates CashSettlementRequest with total amount
3. SuperAdmin reviews pending requests
4. SuperAdmin approves (creates CashSettlement) or rejects
5. If approved, amount is marked for payment
6. CashAdmin receives payment and process completes

**Key Features:**
- Approval workflow ensures financial control
- Complete audit trail of all settlement requests
- Notes enable communication between roles
- Prevents CashAdmin from unilateral payment extraction

---

### 9. **CashSettlement**
**Purpose:** Records approved and completed payment to CashAdmin

**Columns:**
- `Id` (Primary Key)
- `CashAdminId` (Foreign Key) - CashAdmin receiving payment
- `Amount` (decimal) - Settlement amount paid
- `ReceivedByAdminId` (Foreign Key) - SuperAdmin who processed payment
- `Note` (string, nullable) - Payment notes/reference
- `SettlementDate` (DateTime) - When payment was made

**Relationships:**
- ✅ Many-to-One: CashSettlement → ApplicationUser (CashAdmin)
- ✅ Many-to-One: CashSettlement → ApplicationUser (ReceivedByAdmin - SuperAdmin)
- ✅ One-to-Many: CashSettlement ← CashSettlementRequests (Approval record)

**Business Logic:**
- Only created after CashSettlementRequest is approved
- Immutable record of actual payment
- Marks completion of cash handling workflow
- Used for financial reconciliation and auditing

---

## 🔄 Complete Data Flow Diagrams

### **Meal Ordering Flow**
```
1. SuperAdmin creates MealSession
   ↓
2. Student checks available meals during Sale Open window
   ↓
3. Student places Order (wallet balance checked)
   ↓
4. System creates MealToken with OTP code
   ↓
5. WalletTransaction (Debit) records balance reduction
   ↓
6. During MealServingStart → MealServingEnd window:
   - DeliveryMan scans OTP code
   - MealToken status → Redeemed
   - MealToken.RedeemedAt = current time
   - Order.Status → Completed
   ↓
7. After MealServingEnd:
   - Unredeemed tokens automatically expire
   - MealToken.Status → Expired
```

### **Cash Collection & Settlement Flow**
```
1. CashAdmin collects cash from Student
   ↓
2. CashAdmin records CashCollection in system
   ↓
3. System automatically creates WalletTransaction (Credit)
   ↓
4. Student's Wallet.Balance increases
   ↓
5. CashAdmin accumulates multiple collections
   ↓
6. CashAdmin creates CashSettlementRequest
   ↓
7. SuperAdmin reviews request
   ├─→ Approved: Creates CashSettlement
   │   ↓
   │   CashAdmin receives payment from SuperAdmin
   │
   └─→ Rejected: Request ends, CashAdmin retains collected cash
```

### **Wallet Transaction Flow**
```
CREDIT (Balance ↑):
- Cash collection by CashAdmin
- Manual top-up by admin
- Refunds

DEBIT (Balance ↓):
- Meal token purchase
- Service charges
- Penalties
```

---

## 📋 Key Business Rules

### **Meal Session Rules**
- ✅ Price locked once TokenSellStart is reached (immutable)
- ✅ TokenSellStart time cannot change once tokens sold (SoldCount > 0)
- ✅ Status is computed, never stored - derived from timestamps and IsArchived
- ✅ Can only archive Upcoming meals (status must be "Upcoming")
- ✅ Capacity can be 0 (unlimited) or positive integer

### **Order/Token Rules**
- ✅ One Order = One MealToken
- ✅ Orders can only be placed during TokenSellStart → TokenSellEnd
- ✅ Tokens can only be redeemed during MealServingStart → MealServingEnd
- ✅ Student must have sufficient wallet balance at purchase time
- ✅ Same student can only purchase one token per meal session

### **Wallet Rules**
- ✅ Balance cannot go negative
- ✅ All transactions immutable (audit trail)
- ✅ One wallet per student
- ✅ Automatic creation on student registration

### **Cash Settlement Rules**
- ✅ CashAdmin can only request settlement of collected cash
- ✅ SuperAdmin must approve before payment
- ✅ Settlement amount must match collected amount (validations)
- ✅ Once settled, CashAdmin has zero balance for that request

---

## 🔑 Primary Key & Foreign Key Summary

| Table | Primary Key | Foreign Keys |
|-------|------------|--------------|
| ApplicationUser | Id | None (Identity) |
| MealSession | Id | None |
| Order | Id | UserId (ApplicationUser), MealSessionId |
| MealToken | Id | OrderId |
| Wallet | Id | UserId (ApplicationUser) |
| WalletTransaction | Id | WalletId |
| CashCollection | Id | CashAdminId, StudentId (both ApplicationUser), WalletTransactionId |
| CashSettlement | Id | CashAdminId, ReceivedByAdminId (both ApplicationUser) |
| CashSettlementRequest | Id | CashAdminId, ReviewedByAdminId, CashSettlementId |

---

## 📊 Unique Constraints & Indexes (Recommended)

```
CREATE UNIQUE INDEX UX_Wallet_UserId ON Wallets(UserId)
-- Ensures one wallet per student

CREATE INDEX IX_Order_UserId ON Orders(UserId)
-- Fast lookup of student orders

CREATE INDEX IX_Order_MealSessionId ON Orders(MealSessionId)
-- Track orders per meal

CREATE INDEX IX_MealToken_Status ON MealTokens(Status)
-- Fast filtering by token status

CREATE INDEX IX_CashCollection_CashAdminId ON CashCollections(CashAdminId)
-- CashAdmin's collections

CREATE INDEX IX_CashCollection_StudentId ON CashCollections(StudentId)
-- Student's payment history

CREATE INDEX IX_CashSettlementRequest_Status ON CashSettlementRequests(Status)
-- Pending approvals
```

---

## 📈 Scalability & Performance Considerations

### **Current Capability:**
- ✅ Supports multiple meal sessions simultaneously
- ✅ Handles unlimited students
- ✅ Multiple CashAdmins can operate independently
- ✅ Historical data preserved via IsArchived flag

### **Future Optimizations:**
- Add user role indices for permission checks
- Archive old completed orders/tokens to separate table
- Add payment method tracking in CashCollection
- Consider separate reporting database for analytics
- Add email notification triggers on settlement approval

---

## 🎯 System Workflows at a Glance

| Workflow | Start | End | Key Players |
|----------|-------|-----|------------|
| **Meal Creation** | SuperAdmin clicks Create | MealSession saved | SuperAdmin |
| **Token Purchase** | Student in Sale Open window | Order + Token created | Student, System |
| **Token Redemption** | MealServingStart time | Token marked Redeemed | DeliveryMan, System |
| **Cash Collection** | CashAdmin meets Student | CashCollection + Wallet Credit | CashAdmin, Student, System |
| **Settlement Request** | CashAdmin has collections | CashSettlementRequest created | CashAdmin |
| **Settlement Approval** | SuperAdmin reviews | CashSettlement created | SuperAdmin |
| **Settlement Payment** | SuperAdmin approves | CashAdmin receives payment | SuperAdmin, CashAdmin |

---

## 📝 Notes

- **No stored procedures or views currently implemented** - Logic in application layer
- **Soft delete via IsArchived** - Preserves historical data
- **Status computation model** - More flexible than stored status column
- **Role-based access control** - Managed by ASP.NET Identity + policy-based authorization
- **Audit trail via Transactions** - Complete financial history maintained

