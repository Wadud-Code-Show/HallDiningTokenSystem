# Hall Dining Token System - Visual Data Architecture

## Complete System Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          STUDENT MEAL TOKEN SYSTEM                          │
│                         Data Architecture Overview                          │
└─────────────────────────────────────────────────────────────────────────────┘


						 ╔═══════════════════════════════╗
						 ║                               ║
						 ║   ADMIN DASHBOARD             ║
						 ║                               ║
						 ║ • Meal Management             ║
						 ║ • View Reports                ║
						 ║ • Settlement Approvals        ║
						 ║                               ║
						 ╚═════════════╦══════════════════╝
									   │
					┌──────────────────┼──────────────────┐
					│                  │                  │
					▼                  ▼                  ▼
		   ┌──────────────┐   ┌──────────────┐   ┌──────────────┐
		   │ MealSession  │   │Settlement    │   │  Reports     │
		   │              │   │  Requests    │   │              │
		   │ • Create     │   │              │   │ • Meal Sales │
		   │ • Edit       │   │ • Pending    │   │ • Cash Flow  │
		   │ • Archive    │   │ • Review     │   │ • Revenue    │
		   └────────┬─────┘   │ • Approve    │   └──────────────┘
					│         └──────┬──────┘
					│                │
					▼                ▼
			┌─────────────────────────────────┐
			│    ApplicationUser (SuperAdmin) │
			│                                 │
			│ ✓ Full system access            │
			│ ✓ All CRUD operations           │
			│ ✓ Approves settlements          │
			└─────────────────────────────────┘


	┌───────────────────────────────────────────────────────────────┐
	│              STUDENT PORTAL                                   │
	│                                                               │
	│   ┌─────────────────┐          ┌──────────────────┐          │
	│   │ View Available  │          │  Available Meals │          │
	│   │     Meals       │  ─────→  │  (Sale Open)     │          │
	│   └─────────────────┘          └────────┬─────────┘          │
	│                                         │                    │
	│   ┌─────────────────┐                   │                    │
	│   │  Check Wallet   │                   │                    │
	│   │    Balance      │  ◄────────────────┘                    │
	│   └────────┬────────┘                                        │
	│            │                                                 │
	│            └──→ [Balance Sufficient?]                        │
	│                 │                                            │
	│              Yes│  No                                        │
	│                 ├──────► [Show Error]                        │
	│                 │                                            │
	│                 │                                            │
	│                 ▼                                            │
	│          ┌──────────────┐                                   │
	│          │  Buy Token   │                                   │
	│          └────────┬─────┘                                   │
	│                   │                                         │
	│                   ▼                                         │
	│         [CREATE Order Record]                              │
	│         [CREATE MealToken]                                 │
	│         [UPDATE Wallet -$]                                 │
	│         [CREATE WalletTransaction]                         │
	│                   │                                         │
	│                   ▼                                         │
	│         ┌──────────────────────┐                           │
	│         │ Receive OTP Code     │                           │
	│         │ (For Redemption)     │                           │
	│         └──────────────────────┘                           │
	│                   │                                         │
	│                   ▼                                         │
	│         ┌──────────────────────┐                           │
	│         │ Save/Share OTP       │                           │
	│         │ (QR/SMS/App)         │                           │
	│         └──────────────────────┘                           │
	│                                                             │
	└───────────────────────────────────────────────────────────┘
						│
		 ┌──────────────┼──────────────┐
		 │              │              │
		 ▼              ▼              ▼
	┌────────────────────────────────────────┐
	│   ApplicationUser (Student Role)       │
	│                                        │
	│ ✓ Purchase meal tokens                 │
	│ ✓ View wallet balance                  │
	│ ✓ View orders history                  │
	│ ✗ Cannot redeem (DeliveryMan does)     │
	│ ✗ Cannot manage cash                   │
	└────────────────────────────────────────┘


	┌──────────────────────────────────────────────────────────┐
	│         MEAL SERVICE TIME (MealServingStart→End)         │
	│                                                          │
	│  ┌──────────────────────────────────────────────────┐   │
	│  │  DeliveryMan at Meal Distribution Point         │   │
	│  │                                                  │   │
	│  │  1. Student presents with OTP code              │   │
	│  │  2. DeliveryMan scans code (QR reader/phone)    │   │
	│  │  3. System looks up MealToken by OtpCode        │   │
	│  │                                                  │   │
	│  │         [LOOKUP: OtpCode → MealToken]           │   │
	│  │         [CHECK: Status = Active]                │   │
	│  │         [CHECK: In serving time window]         │   │
	│  │                                                  │   │
	│  │     If Valid:                                    │   │
	│  │    ├─ Update MealToken.Status → Redeemed        │   │
	│  │    ├─ Set MealToken.RedeemedAt = Now            │   │
	│  │    ├─ Update Order.Status → Completed           │   │
	│  │    ├─ Set Order.CompletedAt = Now               │   │
	│  │    └─ ✓ Serve meal                              │   │
	│  │                                                  │   │
	│  │     If Invalid:                                  │   │
	│  │    ├─ Show error (expired/invalid)              │   │
	│  │    └─ Request student pay separately            │   │
	│  │                                                  │   │
	│  └──────────────────────────────────────────────────┘   │
	│                                                          │
	└──────────────────────────────────────────────────────────┘
						│
		 ┌──────────────┼──────────────┐
		 │              │              │
		 ▼              ▼              ▼
	┌────────────────────────────────────────┐
	│   ApplicationUser (DeliveryMan Role)   │
	│                                        │
	│ ✓ Redeem (scan) meal tokens            │
	│ ✓ Mark orders as completed             │
	│ ✗ Cannot create orders                 │
	│ ✗ Cannot handle cash/wallets           │
	└────────────────────────────────────────┘


	┌──────────────────────────────────────────────────────────┐
	│         AFTER MEAL SERVICE (Post MealServingEnd)        │
	│                                                          │
	│  System Auto-Processing:                               │
	│                                                          │
	│  Loop all MealTokens with Status="Active":             │
	│    IF MealToken.ExpiresAt < Now:                       │
	│      ├─ MealToken.Status → Expired                     │
	│      ├─ Log expiration                                 │
	│      └─ Cannot be redeemed                             │
	│                                                          │
	│  Loop all Orders with Status="Pending":                │
	│    IF Order.MealSession.MealServingEnd < Now:          │
	│      ├─ No corresponding Completed token?              │
	│      ├─ Order remains Pending (manual review)          │
	│      ├─ Student may request refund                     │
	│      └─ CashAdmin notes the discrepancy               │
	│                                                          │
	└──────────────────────────────────────────────────────────┘
```

---

## Cash Flow Architecture

```
┌────────────────────────────────────────────────────────────────────┐
│                    CASH COLLECTION WORKFLOW                        │
└────────────────────────────────────────────────────────────────────┘


Phase 1: Physical Cash Collection
─────────────────────────────────

	Student                    CashAdmin
	   │                          │
	   │   [Has Cash]             │
	   ├────────────────────────→ │ Collects ৳500
	   │                          │
	   │                      ┌───┴────────────┐
	   │                      │                │
	   │                      ▼                ▼
	   │              ┌────────────────┐   [Records in System]
	   │              │    Physical    │
	   │              │    Cash: ৳500  │   Creates:
	   │              └────────────────┘   ├─ CashCollection
	   │                                   │  {CashAdminId, StudentId,
	   │                                   │   Amount: 500}
	   │                                   │
	   │                                   └─ Auto-links:
	   │                                      WalletTransaction
	   │                                      {Type: "Credit",
	   │                                       Amount: 500}
	   │
	   └──────────────────────────────────────────────────────→
						Over multiple days/weeks
						CashAdmin accumulates
						multiple collections


Phase 2: CashAdmin Requests Settlement
─────────────────────────────────────

	CashAdmin                SuperAdmin
	   │                        │
	   │  [Collected ৳5000]     │
	   │  [Over week]           │
	   │  [Requests payment]    │
	   ├───────────────────────→│ Receives notification
	   │  CashSettlement        │
	   │  Request submitted     │ ┌──────────────────┐
	   │                        │ │ SuperAdmin Views:│
	   │                        │ │ • Pending        │
	   │                        │ │  Requests        │
	   │                        │ │ • Amount: ৳5000  │
	   │                        │ │ • From: CashAdmin│
	   │                        │ │ • Note: Week 1   │
	   │                        │ └──────────────────┘
	   │                        │
	   │                        │ [Approves]
	   │                        │
	   │                        │ Creates:
	   │                        │ ├─ CashSettlement
	   │                        │ │  {CashAdminId,
	   │                        │ │   Amount: 5000,
	   │                        │ │   ReceivedByAdminId}
	   │                        │
	   │                        │ Updates:
	   │                        │ └─ CashSettlementRequest
	   │                        │    {Status: "Approved",
	   │                        │     ReviewedByAdminId,
	   │                        │     AdminNote}
	   │
	   │◄─────────────────────────
	   │  [Notified of Approval]


Phase 3: Payment & Settlement
────────────────────────────

	SuperAdmin              Bank/Finance
	   │                        │
	   │  [Process Payment]     │
	   │  [Amount: ৳5000]       │
	   ├───────────────────────→│ Transfer initiated
	   │  (Outside System)      │
	   │                        │
	   │                    ┌───┴────┐
	   │                    │         │
	   │                    ▼         ▼
	   │              [Bank]     [CashAdmin Account]
	   │              Debit      Credit ৳5000
	   │
	   │ Log in System:
	   │ CashSettlement Complete
	   │


Data State Transitions:
───────────────────────

1. BEFORE Collection:
   ├─ Wallet Balance: ৳1000
   ├─ CashCollection: [empty]
   └─ CashSettlement: [empty]

2. AFTER Collection (CashAdmin records):
   ├─ Wallet Balance: ৳1500 ← Increased!
   ├─ WalletTransaction: [Credit: ৳500]
   ├─ CashCollection: {StudentId, CashAdminId, 500}
   └─ CashSettlement: [empty]

3. AFTER Settlement Request:
   ├─ CashSettlementRequest: {Status: Pending, Amount: 5000}
   └─ Awaiting SuperAdmin approval

4. AFTER Approval:
   ├─ CashSettlementRequest: {Status: Approved}
   ├─ CashSettlement: {Amount: 5000, SettlementDate: Now}
   └─ Payment authorized, ready for bank processing

```

---

## Wallet Transaction Flow

```
┌─────────────────────────────────────────────────────┐
│          WALLET TRANSACTION AUDIT TRAIL             │
└─────────────────────────────────────────────────────┘


Student's Wallet Journey:
──────────────────────

	Initial State:
	├─ Balance: ৳0
	└─ Transactions: []

	[1] Wallet Top-up by CashAdmin:
	├─ Creates WalletTransaction:
	│  {WalletId, Type:"Credit", Amount:1000, Desc:"Cash deposit"}
	├─ Wallet.Balance: ৳0 → ৳1000
	└─ Transactions: [T1]

	[2] Buys first meal token:
	├─ Creates WalletTransaction:
	│  {WalletId, Type:"Debit", Amount:100, Desc:"Lunch token"}
	├─ Creates Order:
	│  {UserId, MealSessionId, Amount:100, Status:"Pending"}
	├─ Creates MealToken:
	│  {OrderId, OtpCode, Status:"Active"}
	├─ Wallet.Balance: ৳1000 → ৳900
	└─ Transactions: [T1, T2]

	[3] Redeems token at meal:
	├─ MealToken: Status → "Redeemed"
	├─ Order: Status → "Completed"
	├─ NO wallet change (already paid)
	└─ Transactions: [T1, T2] (no new transaction)

	[4] Second cash collection:
	├─ Creates WalletTransaction:
	│  {WalletId, Type:"Credit", Amount:500, Desc:"Cash deposit"}
	├─ Wallet.Balance: ৳900 → ৳1400
	└─ Transactions: [T1, T2, T3]

	[5] Buys second meal token:
	├─ Creates WalletTransaction:
	│  {WalletId, Type:"Debit", Amount:100, Desc:"Dinner token"}
	├─ Wallet.Balance: ৳1400 → ৳1300
	└─ Transactions: [T1, T2, T3, T4]

	[6] Does NOT redeem token (expires):
	├─ MealToken: Status → "Expired"
	├─ Order: Status → "Pending" (still)
	├─ Wallet: NO REFUND (no transaction)
	├─ Note: ৳100 is "lost" but balance not adjusted
	└─ Transactions: [T1, T2, T3, T4] (no change)
	   └─ Problem: Balance doesn't match
					orders + tokens!


Final Balance Reconciliation:
──────────────────────────

	Wallet.Balance: ৳1300

	Sum of Credits:    ৳1000 + ৳500 = ৳1500
	Sum of Debits:     ৳100  + ৳100 = ৳200
	Net:               ৳1500 - ৳200  = ৳1300 ✓

	BUT:
	├─ Redeemed tokens: 1 (✓ served)
	├─ Expired tokens:  1 (✗ not served)
	├─ Pending orders:  1 (? unclear)
	└─ Discrepancy:     1 token paid, not redeemed
						Admin must investigate

```

---

## System State Diagram

```
┌──────────────────────────────────────────────────────┐
│        MEALTOKEN LIFECYCLE STATE MACHINE             │
└──────────────────────────────────────────────────────┘


					┌─────────────┐
					│ Order       │
					│ Placed      │
					└──────┬──────┘
						   │
						   │ MealToken created
						   ▼
					┌─────────────────┐
					│ Token: ACTIVE   │
					│                 │
					│ Status Options: │
					│ • Waiting       │
					│ • Purchasable   │
					│ • In Transit    │
					└────────┬────────┘
							 │
		 ┌───────────────────┼───────────────────┐
		 │                   │                   │
	[Redeemed?]          [Redeemed?]         [Expired?]
	  YES│                NO│                  │YES
		 │                  │                  │
		 ▼                  ▼                  ▼
	┌─────────────┐   ┌──────────────┐  ┌─────────────┐
	│ Token:      │   │ Token:       │  │ Token:      │
	│ REDEEMED    │   │ EXPIRED      │  │ EXPIRED     │
	│             │   │              │  │             │
	│ ✓ Served    │   │ ✗ Not used   │  │ ✗ Auto      │
	│ ✓ Order →   │   │ ✓ Discrepancy│  │   expired   │
	│   Complete  │   │   found      │  │ ✓ Cannot use│
	└─────────────┘   │              │  └─────────────┘
					  │ Manual       │
					  │ investigation│
					  │ may refund   │
					  └──────────────┘


Conditions:
───────────

REDEEMED (Success Path):
  ├─ Condition: MealServingStart < Now < MealServingEnd
  │             AND DeliveryMan scans OTP
  ├─ RedeemedAt = Now
  ├─ Order.Status → Completed
  └─ Meal served ✓

EXPIRED (Auto-Expiry):
  ├─ Condition: Now > MealServingEnd
  │             AND Status still "Active"
  ├─ Auto system job runs (daily/hourly)
  ├─ Status → Expired
  ├─ Order.Status remains "Pending"
  ├─ Student may request refund
  └─ Admin investigates discrepancy

NEVER REDEEMED (Discard):
  ├─ Condition: Token.Status = "Expired"
  ├─ Cannot redeem after expiry
  ├─ Wallet not refunded
  ├─ Food not served
  └─ Loss recorded in reports

```

---

## Role-Based Data Access Matrix

```
┌──────────────────────────────────────────────────────────┐
│          DATA ACCESS BY ROLE (Authorization)            │
└──────────────────────────────────────────────────────────┘

				  │ Super │ Cash  │       │ Delivery│
Table             │ Admin │ Admin │Student│   Man   │
──────────────────┼───────┼───────┼───────┼─────────┤
MealSession       │ CRUD  │ R     │ R     │ R       │
Order             │ RD    │ R     │ R*    │ R       │
				  │ (all) │       │(own)  │         │
──────────────────┼───────┼───────┼───────┼─────────┤
MealToken         │ R     │ -     │ -     │ RU*     │
				  │       │       │       │(redeem) │
──────────────────┼───────┼───────┼───────┼─────────┤
Wallet            │ R     │ -     │ R*    │ -       │
				  │ (all) │       │(own)  │         │
──────────────────┼───────┼───────┼───────┼─────────┤
WalletTransaction │ R     │ -     │ R*    │ -       │
				  │ (all) │       │(own)  │         │
──────────────────┼───────┼───────┼───────┼─────────┤
CashCollection    │ CRUD  │ CRD*  │ -     │ -       │
				  │       │(own)  │       │         │
──────────────────┼───────┼───────┼───────┼─────────┤
CashSettlement    │ R     │ R*    │ -     │ -       │
Request           │       │(own)  │       │         │
──────────────────┼───────┼───────┼───────┼─────────┤
CashSettlement    │ CRD*  │ R*    │ -     │ -       │
				  │(cre8) │(own)  │       │         │
──────────────────┼───────┼───────┼───────┼─────────┤
Reports           │ R*    │ R*    │ R*    │ -       │
				  │       │       │(own)  │         │
──────────────────┼───────┼───────┼───────┼─────────┤

Legend:
  C = Create    R = Read    U = Update    D = Delete
  * = Limited access (own records only, or filtered)
  - = No access
```

---

## Data Consistency Rules

```
┌────────────────────────────────────────────┐
│     REFERENTIAL INTEGRITY & CONSTRAINTS    │
└────────────────────────────────────────────┘

Foreign Key Rules:
─────────────────

Order:
  ├─ UserId → ApplicationUser (must exist)
  │  └─ User must have "Student" role
  ├─ MealSessionId → MealSession (must exist)
  │  └─ Cannot point to deleted/archived sessions
  └─ Cascading:
	 ├─ If User deleted → Orders orphaned/deleted
	 └─ If MealSession deleted → Orders invalid

MealToken:
  ├─ OrderId → Order (must exist)
  └─ Cannot exist without Order

Wallet:
  ├─ UserId → ApplicationUser (UNIQUE, must exist)
  └─ One per user, cannot be duplicated

WalletTransaction:
  ├─ WalletId → Wallet (must exist)
  └─ If Wallet deleted → Transactions orphaned

CashCollection:
  ├─ CashAdminId → ApplicationUser (CashAdmin role)
  ├─ StudentId → ApplicationUser (Student role)
  ├─ WalletTransactionId → WalletTransaction (opt)
  └─ Different admins can collect from same student

CashSettlement(Request):
  ├─ CashAdminId → ApplicationUser (must exist)
  ├─ ReviewedByAdminId → ApplicationUser opt (SuperAdmin)
  └─ CashSettlementId → CashSettlement (1:1 optional)


Unique Constraints:
──────────────────

Wallet.UserId UNIQUE
  └─ Only one wallet per student

MealToken.OtpCode UNIQUE
  └─ Each code scanned exactly once
  └─ Prevents duplicate redemptions


Check Constraints:
─────────────────

MealSession:
  └─ TokenSellStart < TokenSellEnd
  └─ TokenSellEnd < MealServingStart
  └─ MealServingStart < MealServingEnd
  └─ Price > 0
  └─ Capacity ≥ 0 (0 = unlimited)

Wallet:
  └─ Balance ≥ 0 (cannot go negative)

WalletTransaction:
  └─ Amount > 0 (positive only)

CashCollection, CashSettlement:
  └─ Amount > 0


Data Validation Rules:
─────────────────────

MealSession.Price:
  ├─ Immutable after TokenSellStart reached
  ├─ Prevents: Student sees price ৳100, later charged ৳200

MealSession.TokenSellStart:
  ├─ Immutable after SoldCount > 0
  ├─ Prevents: Sudden change of purchase window

Order.Amount:
  ├─ Must equal MealSession.Price at purchase time
  ├─ Prevents: Price discrepancy

MealToken.OtpCode:
  ├─ Must be cryptographically unique
  ├─ Prevents: Code collision/duplicate tokens

Order.Status transitions:
  ├─ Pending → Completed (via redeemed token)
  ├─ Pending → Cancelled (admin action)
  ├─ NEVER goes backward
  ├─ Prevents: "Un-completing" an order

```

---

## Performance Considerations

```
┌─────────────────────────────────────────────┐
│    RECOMMENDED DATABASE INDEXES             │
└─────────────────────────────────────────────┘

Frequently Queried:
───────────────────

Orders table:
  ├─ PK: Id
  └─ Recommended:
	 ├─ IX_Order_UserId (student's orders)
	 ├─ IX_Order_MealSessionId (meal's orders)
	 ├─ IX_Order_Status (filter by pending/completed)
	 └─ IX_Order_OrderDate DESC (recent first)

MealToken table:
  ├─ PK: Id
  └─ Recommended:
	 ├─ UNIQUE IX_MealToken_OtpCode (redemption lookup)
	 ├─ IX_MealToken_Status (find actives/expired)
	 ├─ IX_MealToken_OrderId (join to order)
	 └─ IX_MealToken_RedeemedAt (reports)

Wallet table:
  ├─ PK: Id
  └─ Recommended:
	 ├─ UNIQUE IX_Wallet_UserId (one per student)
	 └─ IX_Wallet_Balance? (rarely filtered)

WalletTransaction table:
  ├─ PK: Id
  └─ Recommended:
	 ├─ IX_WalletTransaction_WalletId (all txns)
	 ├─ IX_WalletTransaction_Type (credit/debit)
	 ├─ IX_WalletTransaction_CreatedAt (date range)
	 └─ COMPOUND: (WalletId, CreatedAt DESC)

CashCollection table:
  ├─ PK: Id
  └─ Recommended:
	 ├─ IX_CashCollection_CashAdminId (admin's totals)
	 ├─ IX_CashCollection_StudentId (student's pays)
	 └─ IX_CashCollection_CreatedAt (date range)

CashSettlementRequest table:
  ├─ PK: Id
  └─ Recommended:
	 ├─ IX_CashSettlementRequest_Status (pending)
	 ├─ IX_CashSettlementRequest_CashAdminId
	 └─ IX_CashSettlementRequest_RequestedAt

CashSettlement table:
  ├─ PK: Id
  └─ Recommended:
	 ├─ IX_CashSettlement_CashAdminId
	 └─ IX_CashSettlement_SettlementDate


Query Patterns to Optimize:
──────────────────────────

1. "Get latest meals in Sale Open status"
   └─ Filter: Status computed
   └─ Index: MealSession.CreatedAt DESC, IsArchived

2. "Find token by OTP code for redemption"
   └─ Filter: OtpCode = X
   └─ Index: UNIQUE MealToken.OtpCode
   └─ Join: to Order, MealSession

3. "Get student's pending orders"
   └─ Filter: UserId = X, Status = "Pending"
   └─ Index: IX_Order_UserId+Status

4. "List CashAdmin's uncollected funds"
   └─ Filter: CashAdminId = X
   └─ Aggregate: SUM(Amount)
   └─ Index: IX_CashCollection_CashAdminId

5. "Find pending settlement requests for approval"
   └─ Filter: Status = "Pending"
   └─ Sort: RequestedAt ASC (FIFO)
   └─ Index: IX_CashSettlementRequest_Status

6. "Student statement of wallet activity"
   └─ Filter: WalletId = X, CreatedAt BETWEEN dates
   └─ Sort: CreatedAt DESC (newest first)
   └─ Index: COMPOUND (WalletId, CreatedAt DESC)

```

