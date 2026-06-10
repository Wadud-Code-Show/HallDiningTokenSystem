# Hall Dining Token System - Entity Relationship Diagram (ERD)

## Complete ER Model (Text Format)

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                         CORE USER & IDENTITY SYSTEM                             │
├─────────────────────────────────────────────────────────────────────────────────┤

	┌──────────────────────────────────┐
	│     ApplicationUser (Identity)   │
	├──────────────────────────────────┤
	│ PK: Id (string)                  │
	│ Email                            │
	│ UserName                         │
	│ FullName                         │
	│ StudentId                        │
	│ PasswordHash                     │
	│ ... (Identity fields)            │
	└──────────────┬───────────────────┘
				   │
	 ┌─────────────┼──────────────────────────────┐
	 │             │                              │
	 ▼             ▼                              ▼
  (Orders)    (Wallets)                    (CashCollections)
  (Admin)     (CashAdmin)                   (CashSettlements)
  (Reviews)                                 (SettlementRequests)


┌─────────────────────────────────────────────────────────────────────────────────┐
│                    MEAL SESSION & ORDERING SYSTEM                               │
├─────────────────────────────────────────────────────────────────────────────────┤

	┌──────────────────────────────────┐
	│      MealSession                 │
	├──────────────────────────────────┤
	│ PK: Id                           │
	│ Name                             │
	│ Description                      │
	│ Price                            │
	│ Capacity                         │
	│ TokenSellStartDateTime           │
	│ TokenSellEndDateTime             │
	│ MealServingStartDateTime         │
	│ MealServingEndDateTime           │
	│ IsArchived                       │
	│ CreatedAt                        │
	│ SoldCount                        │
	└──────────────┬───────────────────┘
				   │
				   │ 1:N (One meal has many orders)
				   │
				   ▼
	┌──────────────────────────────────┐
	│        Order                     │
	├──────────────────────────────────┤
	│ PK: Id                           │
	│ FK: UserId ──────────┐           │
	│ FK: MealSessionId    │           │
	│ Amount               │           │
	│ Status               │           │
	│ OrderDate            │           │
	│ CompletedAt          │           │
	└──────────────┬───────────────────┘
				   │        │
				   │        └──────────→ ApplicationUser (Student)
				   │
				   │ 1:1 (One order has one token)
				   │
				   ▼
	┌──────────────────────────────────┐
	│      MealToken                   │
	├──────────────────────────────────┤
	│ PK: Id                           │
	│ FK: OrderId                      │
	│ OtpCode                          │
	│ Status                           │
	│ CreatedAt                        │
	│ RedeemedAt                       │
	│ ExpiresAt                        │
	└──────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────────────────────┐
│                    WALLET & TRANSACTION SYSTEM                                  │
├─────────────────────────────────────────────────────────────────────────────────┤

	┌──────────────────────────────────┐
	│        Wallet                    │
	├──────────────────────────────────┤
	│ PK: Id                           │
	│ FK: UserId ──────────────────┐   │
	│ Balance                      │   │
	│ CreatedAt                    │   │
	│ UpdatedAt                    │   │
	└──────────────┬───────────────────┘
				   │        │
				   │        └──────────→ ApplicationUser (Student)
				   │
				   │ 1:N (One wallet has many transactions)
				   │
				   ▼
	┌──────────────────────────────────┐
	│    WalletTransaction             │
	├──────────────────────────────────┤
	│ PK: Id                           │
	│ FK: WalletId                     │
	│ Amount                           │
	│ TransactionType (Credit/Debit)   │
	│ Description                      │
	│ CreatedAt                        │
	└──────────────┬───────────────────┘
				   │
				   │ ← Optional FK from CashCollection
				   │
				   ▼
	┌──────────────────────────────────┐
	│    CashCollection                │
	├──────────────────────────────────┤
	│ PK: Id                           │
	│ FK: CashAdminId                  │
	│ FK: StudentId                    │
	│ FK: WalletTransactionId (opt)    │
	│ Amount                           │
	│ CreatedAt                        │
	└──────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────────────────────┐
│               CASH SETTLEMENT & APPROVAL WORKFLOW                               │
├─────────────────────────────────────────────────────────────────────────────────┤

	┌──────────────────────────────────┐
	│  CashSettlementRequest           │
	├──────────────────────────────────┤
	│ PK: Id                           │
	│ FK: CashAdminId ──────────┐      │
	│ FK: ReviewedByAdminId     │      │
	│ FK: CashSettlementId      │      │
	│ Amount                    │      │
	│ Status (Pending/etc)      │      │
	│ RequestNote               │      │
	│ RequestedAt               │      │
	│ ReviewedAt                │      │
	│ AdminNote                 │      │
	└──────────────┬────────────────────┘
				   │     │     │     │
	   ┌───────────┘     │     │     │
	   │                 │     │     │
	   └──→ ApplicationUser    │     │
			(CashAdmin)        │     │
							   │     │
			ApplicationUser ←──┘     │
			(ReviewedByAdmin)        │
									 │
									 │ 1:1 (Optional)
									 │
									 ▼
					┌──────────────────────────────────┐
					│    CashSettlement                │
					├──────────────────────────────────┤
					│ PK: Id                           │
					│ FK: CashAdminId  ──────────┐     │
					│ FK: ReceivedByAdminId      │     │
					│ Amount                     │     │
					│ Note                       │     │
					│ SettlementDate             │     │
					└──────────────────────────────────┘
								 │     │
		 ┌───────────────────────┘     │
		 │                             │
		 └──→ ApplicationUser   ApplicationUser
			(CashAdmin)        (ReceivedByAdmin/SuperAdmin)
```

## Detailed Relationship Connections

### Table: ApplicationUser
```
ApplicationUser (1) ─── N ─── Order
├─ Student places order
└─ Tracks which user ordered

ApplicationUser (1) ─── N ─── Wallet
├─ Student has wallet
└─ One wallet per student

ApplicationUser (1) ─── N ─── CashCollection
├─ As CashAdminId: Collections made by this admin
└─ As StudentId: Collections from this student

ApplicationUser (1) ─── N ─── CashSettlement
├─ As CashAdminId: Settlements received by this admin
└─ As ReceivedByAdminId: Settlements processed by this admin

ApplicationUser (1) ─── N ─── CashSettlementRequest
├─ As CashAdminId: Requests made by this admin
└─ As ReviewedByAdminId: Requests reviewed by this admin
```

### Table: MealSession
```
MealSession (1) ─── N ─── Order
├─ Meal has many orders
├─ Constraints:
│  ├─ Order.OrderDate must be in [TokenSellStart, TokenSellEnd]
│  └─ Status computed from timestamps
└─ Triggers meal token generation
```

### Table: Order
```
Order (1) ─── N ─── ApplicationUser
├─ Foreign Key: UserId
├─ Constraint: User must have role "Student"
└─ Relationship: Student places order

Order (N) ─── 1 ─── MealSession
├─ Foreign Key: MealSessionId
├─ Constraint: Order date in token sale window
└─ Tracks which meal was ordered

Order (1) ─── N ─── MealToken
├─ Logic: Creates exactly one token per order
├─ Token.OrderId = Order.Id
└─ Token lifecycle managed separately
```

### Table: MealToken
```
MealToken (N) ─── 1 ─── Order
├─ Multiple tokens logically belong to one order
├─ Constraint: One token per order (current design)
└─ Future: Could support multi-token orders
```

### Table: Wallet
```
Wallet (N) ─── 1 ─── ApplicationUser
├─ Foreign Key: UserId (Unique)
├─ Constraint: One wallet per student
└─ Balance updated via transactions

Wallet (1) ─── N ─── WalletTransaction
├─ Audit trail of all balance changes
├─ Transactions immutable (no updates)
└─ Enables complete financial history
```

### Table: WalletTransaction
```
WalletTransaction (N) ─── 1 ─── Wallet
├─ All transactions belong to one wallet
├─ TransactionType: "Credit" or "Debit"
├─ Never modified (immutable audit log)
└─ Used for reconciliation

WalletTransaction ← CashCollection (Optional Link)
├─ When CashAdmin collects cash:
│  1. Creates CashCollection record
│  2. Creates WalletTransaction (Credit)
│  3. CashCollection.WalletTransactionId links them
└─ Enables tracking: "This credit came from cash collection"
```

### Table: CashCollection
```
CashCollection (N) ─── 1 ─── ApplicationUser (CashAdmin)
├─ Foreign Key: CashAdminId
├─ Relationship: CashAdmin collects cash
└─ Used to aggregate for settlement

CashCollection (N) ─── 1 ─── ApplicationUser (Student)
├─ Foreign Key: StudentId
├─ Relationship: From which student cash was collected
└─ Enables per-student payment tracking

CashCollection ─── WalletTransaction (Optional)
├─ Foreign Key: WalletTransactionId
├─ When present: Wallet was credited
├─ When null: Manual collection not yet deposited
└─ Enables reconciliation between physical and digital
```

### Table: CashSettlementRequest
```
CashSettlementRequest (N) ─── 1 ─── ApplicationUser (CashAdmin)
├─ Foreign Key: CashAdminId
├─ Relationship: Which admin made request
└─ Used to aggregate collections

CashSettlementRequest (N) ─── 1 ─── ApplicationUser (ReviewedByAdmin)
├─ Foreign Key: ReviewedByAdminId (Nullable)
├─ Constraint: Must be SuperAdmin role
├─ Populated only after review
└─ Audit trail of who approved

CashSettlementRequest (1) ─── 1 ─── CashSettlement (Optional)
├─ Foreign Key: CashSettlementId
├─ Relationship: Created if request approved
├─ Links request to actual settlement
└─ Enables tracing: "This settlement was created from that request"
```

### Table: CashSettlement
```
CashSettlement (N) ─── 1 ─── ApplicationUser (CashAdmin)
├─ Foreign Key: CashAdminId
├─ Relationship: Payment made to this admin
└─ Amount paid field

CashSettlement (N) ─── 1 ─── ApplicationUser (ReceivedByAdmin)
├─ Foreign Key: ReceivedByAdminId
├─ Constraint: Must be SuperAdmin
├─ Relationship: Which admin processed payment
└─ Audit trail of who authorized
```

---

## Data Flow Sequences

### 1. Meal Token Purchase & Redemption
```
Student Interface:
1. Student views available meals (Status = "Sale Open")
   └─ MealSession.TokenSellStartDateTime < NOW < TokenSellEndDateTime

2. Student clicks "Buy Token"
   └─ Check: Wallet.Balance >= MealSession.Price

3. System creates Order record
   └─ Order.UserId = Student.Id
   └─ Order.MealSessionId = Meal.Id
   └─ Order.Status = "Pending"
   └─ Order.Amount = Meal.Price

4. System deducts from Wallet
   └─ Wallet.Balance -= Order.Amount
   └─ Creates WalletTransaction (Type="Debit")

5. System generates MealToken
   └─ MealToken.OrderId = Order.Id
   └─ MealToken.OtpCode = Random unique code
   └─ MealToken.Status = "Active"

6. Student receives OTP code (QR/SMS)
   └─ Student stores for later

At Meal Time:
7. DeliveryMan scans OTP during MealServingStart→End window
   └─ System finds MealToken by OtpCode

8. System validates token
   └─ Check: MealServingStartDateTime < NOW < MealServingEndDateTime
   └─ Check: MealToken.Status = "Active"

9. System marks token redeemed
   └─ MealToken.Status = "Redeemed"
   └─ MealToken.RedeemedAt = NOW
   └─ Order.Status = "Completed"
   └─ Order.CompletedAt = NOW

10. DeliveryMan serves meal to student
	└─ Physical transaction complete

After Serving:
11. System auto-expires unredeemed tokens
	└─ If NOW > MealServingEndDateTime and Token.Status="Active"
	└─ Token.Status = "Expired"
```

### 2. Cash Collection & Settlement Workflow
```
Step 1 - Cash Collection (CashAdmin perspective):
┌─────────────────────────────────────────┐
│ CashAdmin meets Student                 │
│ Collects physical cash (e.g., ৳500)     │
│ Records in system: "CashCollection"     │
└─────────────────────────────────────────┘
				   │
				   ▼
	Creates CashCollection record:
	├─ CashAdminId = CashAdmin.Id
	├─ StudentId = Student.Id
	├─ Amount = 500
	└─ CreatedAt = NOW

	System auto-creates WalletTransaction:
	├─ WalletId = Student.Wallet.Id
	├─ Amount = 500
	├─ Type = "Credit"
	├─ Description = "Cash collection from CashAdmin"
	└─ CreatedAt = NOW

	Links created:
	└─ CashCollection.WalletTransactionId = Transaction.Id

	Student's Wallet updated:
	└─ Wallet.Balance += 500


Step 2 - Multiple Collections (Over time):
┌─────────────────────────────────────────┐
│ CashAdmin repeats collection process    │
│ from different students                 │
│ Accumulates cash                        │
└─────────────────────────────────────────┘
	Result: CashCollection table has multiple records
			All with same CashAdminId
			Total amount = SUM(Amount) for this CashAdmin


Step 3 - Settlement Request:
┌─────────────────────────────────────────┐
│ CashAdmin wants to be paid for          │
│ accumulated collections                 │
│ Creates CashSettlementRequest           │
└─────────────────────────────────────────┘
	CashSettlementRequest record created:
	├─ CashAdminId = CashAdmin.Id
	├─ Amount = Total collected amount
	├─ Status = "Pending"
	├─ RequestNote = "Week 1 collections"
	├─ RequestedAt = NOW
	└─ ReviewedByAdminId = NULL (awaiting review)


Step 4 - SuperAdmin Review:
┌─────────────────────────────────────────┐
│ SuperAdmin views pending requests       │
│ Reviews CashAdmin's collections         │
│ Decides: Approve or Reject              │
└─────────────────────────────────────────┘

	IF Approved:
	│
	├─ Updates CashSettlementRequest:
	│   ├─ Status = "Approved"
	│   ├─ ReviewedByAdminId = SuperAdmin.Id
	│   └─ ReviewedAt = NOW
	│
	└─ Creates CashSettlement record:
		├─ CashAdminId = CashAdmin.Id
		├─ Amount = Request.Amount
		├─ ReceivedByAdminId = SuperAdmin.Id
		├─ SettlementDate = NOW
		└─ Note = "Payment for collections"

		Links created:
		└─ CashSettlementRequest.CashSettlementId = Settlement.Id

	IF Rejected:
	│
	└─ Updates CashSettlementRequest:
		├─ Status = "Rejected"
		├─ ReviewedByAdminId = SuperAdmin.Id
		├─ AdminNote = "Reason for rejection"
		└─ ReviewedAt = NOW


Step 5 - Payment Processing:
┌─────────────────────────────────────────┐
│ SuperAdmin processes actual payment     │
│ (Outside system - bank transfer, etc)   │
└─────────────────────────────────────────┘
	CashSettlement record serves as:
	├─ Proof of authorization
	├─ Audit trail of payment
	├─ Amount reference
	└─ Timestamp for accounting
```

---

## Key Cardinality Rules

| From | To | Cardinality | Rule |
|------|--|----|------|
| ApplicationUser | Order | 1:N | One student can place many orders |
| ApplicationUser | Wallet | 1:1 | One wallet per student (unique) |
| ApplicationUser | CashCollection | 1:N | One CashAdmin makes many collections |
| ApplicationUser | CashSettlement | 1:N | One admin receives many settlements |
| MealSession | Order | 1:N | One meal has many orders |
| Order | MealToken | 1:1 | One order has one token |
| Wallet | WalletTransaction | 1:N | One wallet has many transactions |
| CashCollection | WalletTransaction | N:1 | Many collections may link to transactions |
| CashSettlementRequest | CashSettlement | 1:1 | One request creates one settlement |

---

## Constraint & Validation Rules

### NOT NULL Constraints
```
ApplicationUser: Email, UserName
MealSession: Name, Price, Capacity, TokenSellStartDateTime, TokenSellEndDateTime, 
			 MealServingStartDateTime, MealServingEndDateTime, CreatedAt
Order: UserId, MealSessionId, Amount, Status, OrderDate
MealToken: OrderId, OtpCode, Status, CreatedAt
Wallet: UserId, Balance, CreatedAt
WalletTransaction: WalletId, Amount, TransactionType, Description, CreatedAt
CashCollection: CashAdminId, StudentId, Amount, CreatedAt
CashSettlement: CashAdminId, Amount, ReceivedByAdminId, SettlementDate
CashSettlementRequest: CashAdminId, Amount, Status, RequestedAt
```

### UNIQUE Constraints
```
Wallet.UserId (One wallet per student)
MealToken.OtpCode (Each token code unique for scanning)
```

### CHECK Constraints
```
MealSession.TokenSellStartDateTime < TokenSellEndDateTime
MealSession.TokenSellEndDateTime < MealServingStartDateTime
MealSession.MealServingStartDateTime < MealServingEndDateTime
Wallet.Balance >= 0 (No negative balance)
WalletTransaction.Amount > 0 (Positive amounts)
CashCollection.Amount > 0 (Positive amounts)
CashSettlement.Amount > 0 (Positive amounts)
```

### Role-Based Constraints
```
Order.UserId → Must have "Student" role
CashCollection.CashAdminId → Must have "CashAdmin" role
CashCollection.StudentId → Must have "Student" role
CashSettlement.ReceivedByAdminId → Must have "SuperAdmin" role
CashSettlementRequest.ReviewedByAdminId → Must have "SuperAdmin" role
```

---

## Query Patterns (Common Operations)

### Get Student's Wallet & Balance
```sql
SELECT w.* FROM Wallets w
JOIN AspNetUsers u ON w.UserId = u.Id
WHERE u.StudentId = @studentId
```

### Get Student's Order History
```sql
SELECT o.*, m.Name as MealName, m.Price
FROM Orders o
JOIN MealSessions m ON o.MealSessionId = m.Id
WHERE o.UserId = @userId
ORDER BY o.OrderDate DESC
```

### Get Token by OTPCode (For Redemption)
```sql
SELECT t.*, o.*, m.*
FROM MealTokens t
JOIN Orders o ON t.OrderId = o.Id
JOIN MealSessions m ON o.MealSessionId = m.Id
WHERE t.OtpCode = @otpCode
```

### Get CashAdmin's Pending Settlement
```sql
SELECT SUM(Amount) as TotalCollected
FROM CashCollections
WHERE CashAdminId = @cashAdminId
AND CreatedAt >= @startDate
```

### Get Pending Settlement Requests (For SuperAdmin)
```sql
SELECT r.*, u.FullName, u.Email
FROM CashSettlementRequests r
JOIN AspNetUsers u ON r.CashAdminId = u.Id
WHERE r.Status = 'Pending'
ORDER BY r.RequestedAt ASC
```

