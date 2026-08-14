namespace Home.WebApi.UseCases.MealPlanEntries.CreateMealPlanEntry;

public record CreateMealPlanEntryApiRequest(DateTime Date, long RecipeID);
