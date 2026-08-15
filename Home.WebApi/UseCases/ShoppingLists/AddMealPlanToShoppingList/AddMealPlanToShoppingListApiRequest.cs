namespace Home.WebApi.UseCases.ShoppingLists.AddMealPlanToShoppingList;

public record AddMealPlanToShoppingListApiRequest(DateTime FromDate, long? MealSlotID, DateTime ToDate);
