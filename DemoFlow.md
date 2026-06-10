# Demo Flow Guide

Follow these steps to demonstrate the complete lifecycle of the HallDiningTokenSystem.

---

## 1. Setup Phase (SuperAdmin)
**Goal:** Create a meal session available for purchase.

1. Login as `superadmin@test.com`.
2. Navigate to **Create Meal**.
3. Enter meal details (e.g., "Lunch Special").
4. **Important:** Set **Token Sale Start** to a past time and **Token Sale End** to a future time so it's currently open for sale.
5. Set **Meal Serving** times accordingly.
6. Click **Create**.
7. View the **Meal List** to confirm the status is "Sale Open".

---

## 2. Wallet Funding (CashAdmin)
**Goal:** Add balance to a student's wallet.

1. Login as `cashadmin1@test.com`.
2. Search for a student (e.g., "Rahim" or ID "220130") on the **Dashboard**.
3. Click **Top-up Wallet**.
4. Enter an amount (e.g., 500) and click **Top-up**.
5. View the **Cash Statement** to see the transaction recorded.

---

## 3. Token Purchase (Student)
**Goal:** Buy a meal token.

1. Login as `student1@test.com`.
2. Navigate to **Available Meals**.
3. Find the meal created in Step 1 and click **View Details**.
4. Click **Buy Token**.
5. Confirm the purchase.
6. Navigate to **My Tokens** to see the active token and its **OTP Code**.
7. Check **Wallet History** to see the deduction.

---

## 4. Meal Redemption (DeliveryMan)
**Goal:** Verify student's OTP and serve the meal.

1. Login as `delivery@test.com`.
2. Navigate to **Verify Token**.
3. Enter the student's **OTP Code** (obtained in Step 3).
4. Click **Verify**.
5. Confirm the student and meal details on the result page.
6. Click **Redeem Token**.
7. Check **Serving History** to see the redemption recorded.

---

## 5. Financial Audit (SuperAdmin)
**Goal:** Settle cash collections and audit the system.

1. **CashAdmin:** Navigate to **Cash Settlement** and click **Request Settlement**. Enter the amount collected (e.g., 500).
2. **SuperAdmin:** Login as `superadmin@test.com`.
3. Navigate to **Settlements -> Pending Requests**.
4. Review the request from Step 5.1 and click **Approve**.
5. Go to **Settlements -> Consistency Dashboard** to see the system-wide financial balance.
6. Go to **Reports -> Dashboard** to see the overall revenue and activity.
