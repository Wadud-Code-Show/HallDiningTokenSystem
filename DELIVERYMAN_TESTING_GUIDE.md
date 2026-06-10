# DeliveryMan Token Verification Testing Guide

## Overview
This guide provides step-by-step instructions to test the DeliveryMan meal serving and token redemption module in a realistic scenario.

## Prerequisites
- Solution builds successfully (✅ confirmed)
- Database migration applied (✅ confirmed)
- User with DeliveryMan role created
- User with Student role created
- Meal session created with active serving window

## Test Scenario Setup

### 1. Create Test Users

#### DeliveryMan User
```
Username: deliveryman@test.com
Password: DeliveryMan@Test123
Role: DeliveryMan
Full Name: John Delivery
```

#### Student User
```
Username: student@test.com
Password: Student@Test123
Role: Student
Student ID: STU001
Full Name: Alice Student
```

### 2. Create Active Meal
1. Login as Admin/SuperAdmin
2. Navigate to "Create Meal"
3. Fill details:
   - Meal Name: "Lunch - Friday"
   - Description: "Friday lunch special"
   - Price: 50 (or your currency)
   - Capacity: 10
   - Meal Date: Today's date
   - Serving Start Time: Current time or recent past
   - Serving End Time: 30-60 minutes from now
   - Click "Create"

### 3. Student Purchases Token
1. Logout of Admin account
2. Login as Student (student@test.com)
3. Navigate to "Available Meals"
4. Find and click "Lunch - Friday"
5. Click "Buy Token (Coming Soon)" or purchase button
6. Verify purchase succeeds - note the OTP code displayed
7. **Store the OTP code for testing**
8. Navigate to "My Tokens" - verify token shows as "Active"

## Test Cases

### Test 1: Valid Token Redemption ✅

**Purpose**: Verify successful token verification and redemption

**Steps**:
1. Logout as Student
2. Login as DeliveryMan (deliveryman@test.com)
3. Navigate to "Serving Dashboard"
   - Verify "Lunch - Friday" appears in list
   - Verify time remaining shows correctly
   - Verify sold count = 1, redeemed = 0
4. Click "Verify Token for this Meal" or go to "Verify Token"
5. Enter the student's OTP code from step 7 above
6. Press Enter or click "Verify Token"
7. **Expected Result**: Green success card appears
   - Message: "Token Redeemed"
   - Shows student name: "Alice Student"
   - Shows meal: "Lunch - Friday"
   - Shows timestamp of redemption
   - Shows student ID: "STU001"

**Database Verification**:
```sql
-- In SQL Server Management Studio
SELECT Id, OtpCode, Status, RedeemedAt, RedeemedById 
FROM MealTokens 
WHERE OtpCode = 'YOUR_OTP_CODE';

-- Should show:
-- Status = 'Redeemed'
-- RedeemedAt = recent timestamp
-- RedeemedById = DeliveryMan user ID
```

**Continue**:
8. Click "Verify Next Token" button
9. Dashboard should show: redeemed = 1, remaining = 0

---

### Test 2: Invalid OTP ❌

**Purpose**: Verify system rejects non-existent OTP codes

**Steps**:
1. From Verify Token page
2. Enter: "INVALID123"
3. Press Enter
4. **Expected Result**: Red error card appears
   - Message: "Invalid Token: OTP code not found."
   - No state change in database

---

### Test 3: Already Redeemed ❌

**Purpose**: Prevent double-redemption of same token

**Prerequisites**: Complete Test 1 first

**Steps**:
1. From Verify Token page (DeliveryMan still logged in)
2. Enter the same OTP from Test 1 again
3. Press Enter
4. **Expected Result**: Red error card appears
   - Message: "Token already redeemed."
   - Status badge shows: "Already Redeemed"
   - Database unchanged - only one redemption record

---

### Test 4: Serving Window Closed (Token Expired) ❌

**Purpose**: Prevent redemption outside serving hours

**Setup**:
1. Create new meal with serving end time = 2 minutes from now
2. Student purchases token (note OTP code)
3. Wait 3+ minutes for serving window to close

**Steps**:
1. DeliveryMan tries to verify the token
2. Enter student's OTP
3. Press Enter
4. **Expected Result**: Red error card appears
   - Message: "Serving window closed: Serving window has closed. Ended at [time]"
   - Token status remains "Active" (not marked as redeemed)
   - Order status remains "Pending" (not completed)

---

### Test 5: Meal Archived ❌

**Purpose**: Prevent redemption of archived meals

**Setup**:
1. Create new meal, student purchases token
2. Note the OTP code
3. Admin archives the meal

**Steps**:
1. DeliveryMan tries to verify the token
2. Enter student's OTP
3. Press Enter
4. **Expected Result**: Red error card appears
   - Message: "This meal has been archived and cannot accept redemptions."
   - Token status remains "Active"
   - Order status remains "Pending"

---

### Test 6: Serving History Filtering 📋

**Purpose**: Verify serving history shows only current user's redemptions

**Prerequisites**: Complete Test 1 (at least one successful redemption)

**Steps**:
1. DeliveryMan is logged in
2. Navigate to "Serving History"
3. **Verify overview**:
   - Total Redemptions: 1 (or number of successful verifications)
   - Date: Today's date
   - Serving Staff: "John Delivery" (logged-in user name)

4. **Verify table/list**:
   - Shows OTP code (should match from Test 1)
   - Shows student name: "Alice Student"
   - Shows student ID: "STU001"
   - Shows meal: "Lunch - Friday"
   - Shows timestamp of verification
   - Shows "Redeemed" status badge in green
   - Shows "time ago" calculation (e.g., "5 minutes ago")

5. **Date filter test**:
   - Click date picker, select yesterday's date
   - Click "Filter"
   - **Expected**: Empty state appears (no redemptions yesterday)
   - Click "Clear" to go back to today

6. **Print test**:
   - Click "Print History" button
   - Print preview should hide buttons/form elements
   - Should show clean table/cards with redemption records

---

### Test 7: Multiple Concurrent Serves 🔄

**Purpose**: Verify dashboard updates with multiple served tokens

**Setup**:
1. Create meal with capacity = 5
2. Have 3 different students purchase tokens (note all 3 OPT codes)

**Steps**:
1. DeliveryMan logs in, goes to Dashboard
   - Shows: total sold = 3, redeemed = 0
2. Verify Token 1
   - Success, Dashboard shows: sold = 3, redeemed = 1
3. Verify Token 2
   - Success, Dashboard shows: sold = 3, redeemed = 2
4. Check Serving History
   - Shows 2 redemptions (most recent first)
   - Both show correct student names, timestamps

---

## Urgent Indicators Test 🚨

**Purpose**: Verify dashboard shows urgency warnings

**Setup**:
1. Create meal with serving end time = 10 minutes from now

**Verification**:
1. DeliveryMan goes to Dashboard
2. Find the meal in serving window
3. **Verify visual indicators**:
   - Card should show "URGENT" badge in red
   - Time remaining displays in red/warning color
   - Text shows something like "9:45 remaining" or "URGENT: 10min"

---

## Role-Based Access Test 🔐

**Purpose**: Verify only DeliveryMan can access serving features

**Steps**:
1. Student logs in
   - Navbar should NOT show "Serving Dashboard", "Verify Token", or "Serving History"
   - Attempting direct URL access should trigger 403 Forbidden (Authorize)

2. Admin logs in
   - Navbar should NOT show DeliveryMan serving links
   - Attempting direct URL access should trigger 403 Forbidden (Authorize)

3. DeliveryMan logs in
   - Navbar SHOULD show all three DeliveryMan links
   - All DeliveryMan views are accessible

---

## Performance Notes

### Expected Performance
- Dashboard load: < 1 second (queries active meals and token counts)
- Token verification: < 500ms (includes transaction and save)
- Serving history: < 1 second (filters by DeliveryMan and date)

### Database Query Monitoring
If experiencing slowness, check:
1. `MealTokens` table indexed on `RedeemedById` (✅ automatic via migration)
2. `MealTokens` table indexed on `OtpCode` for lookups
3. `MealSessions` queries filtering on `IsArchived` and time ranges

---

## Rollback Procedure (If Needed)

If you need to revert to before DeliveryMan module:

```powershell
cd "D:\Dotnet Core Projects\HallDiningTokenSystem\HallDiningTokenSystem"

# Remove the migration
dotnet ef migrations remove

# This will:
# 1. Delete the migration file
# 2. Update the snapshot
# 3. NOT undo database (do manually next)

# To undo database changes:
dotnet ef database update 20260529025606_RefactorMealLifecycleModel
```

---

## Success Criteria ✅

- [x] Dashboard shows active meals with redemption stats
- [x] Verify Token page displays large OTP input
- [x] Valid OTP redemption succeeds with green success screen
- [x] Invalid OTP shows appropriate error
- [x] Already-redeemed token shows "Token already redeemed" error
- [x] Serving window enforcement works (outside hours = error)
- [x] Archived meal prevents token verification
- [x] Serving History shows current user's redemptions only
- [x] Date filtering works in serving history
- [x] Mobile-responsive views work correctly
- [x] Role-based access prevents non-DeliveryMan users
- [x] Solution builds without errors
- [x] Database migration applied successfully

---

## Common Issues & Troubleshooting

### Issue: Datetime mismatch between client/server
**Solution**: Ensure server time is UTC or consistently applied. Controller uses `DateTime.Now` - verify server clock is correct.

### Issue: OTP not matching
**Solution**: OTP codes are case-insensitive (JavaScript auto-uppercase). Verify exact OTP from student's My Tokens page.

### Issue: "No active serving meals" always shows
**Solution**: Check meal time ranges and current server time. Ensure meal serving window includes current time.

### Issue: Serving History showing other DeliveryMan's records
**Solution**: Verify `RedeemedById` filtering in `ServingHistory()` action. Should filter by `currentUser.Id` only.

### Issue: Double-redemption still possible
**Solution**: This should not happen - check both Step 2 AND Step 6 in VerifyToken POST action are executing.

---

## Performance Stress Test (Optional)

### High-Volume Verification
1. Create meal with capacity = 1000
2. Simulate 100 token verifications in sequence
3. Monitor dashboard response times
4. Check database for transaction locks

**Expected**: No performance degradation, all transactions succeed

---

End of Testing Guide
