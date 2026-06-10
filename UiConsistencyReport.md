# UI Consistency Report - Meal Lifecycle Terminology

This report documents the changes made to align the user interface with the updated meal lifecycle model.

## Files Changed

### Student Views
- `HallDiningTokenSystem\Views\Student\Dashboard.cshtml`
- `HallDiningTokenSystem\Views\Student\Meals.cshtml`
- `HallDiningTokenSystem\Views\Student\MealDetails.cshtml`

### Admin Views
- `HallDiningTokenSystem\Views\Admin\Meals.cshtml`
- `HallDiningTokenSystem\Views\Admin\MealDetails.cshtml`
- `HallDiningTokenSystem\Views\Admin\CreateMeal.cshtml`
- `HallDiningTokenSystem\Views\Admin\EditMeal.cshtml`
- `HallDiningTokenSystem\Views\Admin\MealHistory.cshtml`

### Delivery Staff Views
- `HallDiningTokenSystem\Views\DeliveryMan\Dashboard.cshtml`
- `HallDiningTokenSystem\Views\DeliveryMan\VerifyToken.cshtml`

### Reports Views
- `HallDiningTokenSystem\Views\Reports\MealDetails.cshtml`

## Labels Updated

| Category | Old Terminology | New Terminology |
| :--- | :--- | :--- |
| **Token Purchase Phase** | Start Time / Token Sale Starts | Sale Start |
| **Token Purchase Phase** | End Time / Token Sale Ends | Sale End |
| **Meal Serving Phase** | Meal Serving Starts | Serving Start |
| **Meal Serving Phase** | Meal Serving Ends | Serving End |
| **Meal Serving Phase** | Window / Serving Window | Serving Window |
| **Phase Titles** | Token Sale Window | Token Purchase Phase |
| **Phase Titles** | Meal Serving Window | Meal Serving Phase |
| **Status Reference** | Active | Sale-Open (where applicable) |

## Key Improvements
1. **Ambiguity Removal:** Replaced generic "Start Time" and "End Time" with phase-specific labels ("Sale Start", "Serving End", etc.) across all user roles.
2. **Phase Consistency:** Ensured that "Token Purchase Phase" and "Meal Serving Phase" are used as standard headers.
3. **Delivery Flow:** Updated the DeliveryMan dashboard to use "Serving Window" and clarified the verification process terminology.
4. **Admin Protection Rules:** Updated the descriptive text in the Edit Meal view to use the new terminology when explaining field locks.

## Remaining Inconsistencies
- Some model properties still use names like `TokenSellStartDateTime`. These were NOT renamed to maintain database and logic stability as per instructions.
- Some view models might still use `ActiveMealsCount` or similar names internally, but the UI labels now reflect the correct terminology.
- `Views/DeliveryMan/ServingHistory.cshtml` still uses "Redemptions" which is technically correct in the lifecycle model (Redemption is the action within the Serving Phase).

## Conclusion
The UI is now consistent with the meal lifecycle model, providing clear distinctions between the purchase and serving phases for Students, Admins, and Delivery Staff.
