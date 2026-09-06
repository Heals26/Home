namespace Home.WebApi.UseCases.MealPlanEntries.CreateMealPlanEntry;

/// <summary>
/// Send a <c>RecipeID</c> to plan something the household cooks, or a <c>Title</c> to plan an
/// occasion instead. One or the other.
/// </summary>
public record CreateMealPlanEntryApiRequest(DateTime Date, long? MealSlotID, long? RecipeID, string Title);
