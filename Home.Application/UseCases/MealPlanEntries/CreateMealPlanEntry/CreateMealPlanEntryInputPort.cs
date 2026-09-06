using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.MealPlanEntries.CreateMealPlanEntry;

/// <summary>
/// Plans one thing for one day. Exactly one of <paramref name="RecipeID"/> and
/// <paramref name="Title"/> carries it: a recipe when the household is cooking something, a title
/// when the meal is an occasion instead.
/// </summary>
public record CreateMealPlanEntryInputPort(DateTime Date, long? MealSlotID, long? RecipeID, string? Title)
    : IInputPort<ICreateMealPlanEntryOutputPort>;
