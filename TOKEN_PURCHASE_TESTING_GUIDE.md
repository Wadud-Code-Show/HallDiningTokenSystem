# Token Purchase System - Testing Guide

## Quick Test Scenarios

### Scenario 1: Successful Purchase (Happy Path)
**Setup:**
- Student logged in as "Student" role
- Wallet balance: ৳500+
- Meal: "Lunch Special" - Price: ৳150, Capacity: 10, SoldCount: 2
- Current time: Within TokenSellStartDateTime to TokenSellEndDateTime

**Steps:**
1. Navigate to Student Dashboard
2. Click "Available Meals"
3. Click meal details on "Lunch Special"
4. Verify button shows "Buy Token" (not disabled)
5. Click "Buy Token"

**Expected Results:**
- ✅ Order record created in database
- ✅ MealToken record created with unique OTP
- ✅ Wallet.Balance reduced by ৳150
- ✅ WalletTransaction entry created (Debit)
- ✅ MealSession.SoldCount incremented to 3
- ✅ Redirects to MyTokens page
- ✅ Success message: "Meal token purchased successfully."
- ✅ Token visible in MyTokens with meal name, OTP, and status "Active"

**Verify in DB:**
```sql
SELECT * FROM Orders WHERE UserId = @studentId ORDER BY OrderDate DESC LIMIT 1;
SELECT * FROM MealTokens WHERE OrderId = (SELECT TOP 1 Id FROM Orders WHERE UserId = @studentId ORDER BY OrderDate DESC);
SELECT * FROM Wallets WHERE UserId = @studentId;
SELECT * FROM WalletTransactions WHERE WalletId = (SELECT Id FROM Wallets WHERE UserId = @studentId) ORDER BY CreatedAt DESC LIMIT 1;
SELECT SoldCount FROM MealSessions WHERE Id = @mealId;
```

---

### Scenario 2: Insufficient Balance
**Setup:**
- Student logged in
- Wallet balance: ৳50
- Meal: ৳150

**Steps:**
1. Navigate to Student Dashboard
2. Click "Available Meals"
3. Click meal details
4. Click "Buy Token"

**Expected Results:**
- ✅ Error message: "Insufficient wallet balance. Required: ৳150.00, Available: ৳50.00"
- ✅ Redirects back to MealDetails
- ✅ No Order, MealToken, WalletTransaction, or SoldCount changes
- ✅ Wallet balance remains ৳50

**Verify in DB:**
```sql
-- Orders count unchanged
SELECT COUNT(*) FROM Orders WHERE UserId = @studentId;
-- WalletTransaction count unchanged
SELECT COUNT(*) FROM WalletTransactions WHERE WalletId = (SELECT Id FROM Wallets WHERE UserId = @studentId);
```

---

### Scenario 3: Duplicate Purchase Prevention
**Setup:**
- Student already purchased token for meal (Order exists)
- Student has sufficient balance
- Meal still within sale window

**Steps:**
1. Navigate to MealDetails for same meal
2. Click "Buy Token"

**Expected Results:**
- ✅ Error message: "You already purchased a token for this meal."
- ✅ Redirects to MyTokens
- ✅ No new Order or MealToken created
- ✅ Wallet balance unchanged

**Verify in DB:**
```sql
-- Should have exactly 1 Order for this student + meal combo
SELECT COUNT(*) FROM Orders WHERE UserId = @studentId AND MealSessionId = @mealId;
-- Result: 1
```

---

### Scenario 4: Meal Sold Out
**Setup:**
- Meal: Capacity = 5, SoldCount = 5
- Student has sufficient balance
- Within sale window

**Steps:**
1. Navigate to MealDetails
2. Try to click "Buy Token"

**Expected Results:**
- ✅ Button shows "Buy Token" but may be logically disabled or show capacity warning
- ✅ If clicked, error message: "This meal is sold out."
- ✅ Redirects to MealDetails
- ✅ No changes to database

---

### Scenario 5: Purchase Outside Sale Window
**Setup:**
- TokenSellStartDateTime = Tomorrow 10:00 AM
- Current time = Today 2:00 PM
- Student has sufficient balance

**Steps:**
1. Navigate to MealDetails
2. Click "Buy Token"

**Expected Results:**
- ✅ Error message: "Token purchase is not available at this time."
- ✅ Redirects to MealDetails
- ✅ No database changes

---

### Scenario 6: Archived Meal
**Setup:**
- Meal.IsArchived = true
- Student navigates to MealDetails via direct URL

**Steps:**
1. Try to access MealDetails for archived meal
2. Click "Buy Token" if visible

**Expected Results:**
- ✅ MealDetails returns NotFound() (should not be accessible)
- ✅ If somehow purchase attempted, error: "This meal is no longer available for purchase (archived)."
- ✅ Redirects to Meals list

---

### Scenario 7: OTP Uniqueness
**Setup:**
- Generate multiple tokens in rapid succession

**Steps:**
1. Student 1 purchases meal → OTP: AB12CD
2. Student 2 purchases same meal → OTP: XY98ZW
3. Student 3 purchases same meal → OTP: LM56NP

**Expected Results:**
- ✅ Each token has unique OTP code
- ✅ No two tokens in database have same OTP

**Verify in DB:**
```sql
SELECT OtpCode, COUNT(*) FROM MealTokens GROUP BY OtpCode HAVING COUNT(*) > 1;
-- Result: Empty (no duplicates)
```

---

### Scenario 8: Wallet History Display
**Setup:**
- Student has purchased multiple tokens
- Posted multiple top-ups (if applicable)

**Steps:**
1. Navigate to Dashboard
2. Click "Wallet History"

**Expected Results:**
- ✅ Current balance displayed at top
- ✅ All transactions listed in newest-first order
- ✅ Each transaction shows: Date, Type (Credit/Debit), Amount, Description
- ✅ Running balance calculated correctly
- ✅ Debit transactions show meal token purchase description
- ✅ Credits show wallet top-up (if implemented)

**Verify Math:**
```
Running balance = Current balance + (subsequent debits - subsequent credits)
For each transaction row
```

---

### Scenario 9: MyTokens Display
**Setup:**
- Student has 3 active tokens, 1 redeemed, 1 expired

**Steps:**
1. Navigate to MyTokens

**Expected Results:**
- ✅ All 5 tokens displayed
- ✅ OTP codes prominently displayed in monospace font
- ✅ Status badges correctly show "Active", "Redeemed", "Expired"
- ✅ Colors: Active=green, Redeemed=blue, Expired=red
- ✅ Meal serving times visible
- ✅ Token summary shows: 3 Active, 1 Redeemed, 1 Expired
- ✅ Responsive layout on mobile

---

### Scenario 10: Concurrent Purchases (High-Level)
**Setup:**
- Multiple students try to purchase same meal simultaneously
- Meal capacity: 2

**Steps:**
1. Student A submits BuyToken request
2. Student B submits BuyToken request (same time)
3. Student C submits BuyToken request (same time)

**Expected Results:**
- ✅ First two succeed (SoldCount becomes 2)
- ✅ Third fails with "sold out" message
- ✅ No data corruption or duplicate tokens
- ✅ Wallet balances updated correctly for successful purchases

**Verify in DB:**
```sql
SELECT COUNT(*) FROM Orders WHERE MealSessionId = @mealId;
-- Result: 2
SELECT SoldCount FROM MealSessions WHERE Id = @mealId;
-- Result: Previous + 2

-- No duplicate OTPs
SELECT COUNT(DISTINCT OtpCode) FROM MealTokens 
WHERE OrderId IN (SELECT Id FROM Orders WHERE MealSessionId = @mealId);
-- Should equal count of tokens
```

---

## Manual Testing Checklist

### UI/UX Testing
- [ ] Buy Token button enabled only when `IsPurchaseReady = true`
- [ ] Purchase form submits to correct action (BuyToken)
- [ ] Success/error messages display clearly
- [ ] MyTokens page responsive on mobile
- [ ] WalletHistory responsive on mobile
- [ ] OTP displays in large, readable monospace font
- [ ] Status badges color-coded correctly
- [ ] Navigation links all working

### Data Integrity Testing
- [ ] No orphaned Orders (without MealToken)
- [ ] No orphaned MealTokens (without Order)
- [ ] Wallet balance never negative
- [ ] SoldCount matches number of active Orders
- [ ] All Debit transactions match purchases
- [ ] Running balance calculates correctly

### Transaction Testing
- [ ] Purchase succeeds only if ALL steps succeed
- [ ] Rollback triggered on validation failure
- [ ] Rollback triggered on database error
- [ ] No partial writes visible to users
- [ ] Concurrent purchases don't corrupt data

### Security Testing
- [ ] Non-Student role cannot purchase
- [ ] Student can only see own tokens
- [ ] Student cannot manually create Order
- [ ] Token deletion/modification not possible via UI
- [ ] SQL injection prevention (ASP.NET Identity)

### Error Handling Testing
- [ ] All 7 validation failures handled gracefully
- [ ] User redirected appropriately after error
- [ ] Database remains consistent after failed purchase
- [ ] Error messages helpful and accurate

---

## Performance Testing

### Query Performance
- [ ] MyTokens query completes < 500ms
- [ ] WalletHistory query completes < 500ms
- [ ] Dashboard calculations < 200ms

### Transaction Timing
- [ ] Full purchase transaction completes < 2 seconds
- [ ] OTP generation < 100ms

---

## Browser & Device Testing

- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Safari (latest)
- [ ] Edge (latest)
- [ ] Mobile (iOS Safari)
- [ ] Mobile (Android Chrome)

---

## Database Consistency Checks

```sql
-- 1. Check for orphaned Orders
SELECT o.Id FROM Orders o 
LEFT JOIN MealTokens mt ON o.Id = mt.OrderId 
WHERE mt.Id IS NULL;
-- Expected: No rows

-- 2. Check for orphaned MealTokens
SELECT mt.Id FROM MealTokens mt 
LEFT JOIN Orders o ON mt.OrderId = o.Id 
WHERE o.Id IS NULL;
-- Expected: No rows

-- 3. Check negative wallet balances
SELECT Id, UserId, Balance FROM Wallets WHERE Balance < 0;
-- Expected: No rows

-- 4. Verify SoldCount accuracy
SELECT ms.Id, ms.Name, ms.SoldCount, 
	   (SELECT COUNT(*) FROM Orders WHERE MealSessionId = ms.Id) as OrderCount
FROM MealSessions 
WHERE SoldCount != (SELECT COUNT(*) FROM Orders WHERE MealSessionId = ms.Id);
-- Expected: No mismatches

-- 5. Check wallet transaction counts
SELECT w.Id, w.UserId, 
	   (SELECT COUNT(*) FROM WalletTransactions WHERE WalletId = w.Id) as TxCount
FROM Wallets w;
-- Expected: Reasonable distribution

-- 6. Verify running balance logic
SELECT WT.CreatedAt, WT.Amount, WT.TransactionType, 
	   W.Balance as CurrentBalance
FROM WalletTransactions WT
JOIN Wallets W ON WT.WalletId = W.Id
ORDER BY WT.CreatedAt DESC LIMIT 10;
-- Expected: Balance calculations correct
```

---

## Reporting

After testing, document:
- [ ] Issues found
- [ ] Severity levels
- [ ] Reproduction steps
- [ ] Expected vs. actual results
- [ ] Screenshots/logs
- [ ] Device and browser info

---

## Go-Live Checklist

Before deploying to production:
- [ ] All scenarios tested
- [ ] Database consistency verified
- [ ] Performance acceptable
- [ ] No security vulnerabilities
- [ ] Error messages user-friendly
- [ ] Staff trained on system
- [ ] Backup procedure in place
- [ ] Rollback procedure documented
- [ ] Monitoring configured
