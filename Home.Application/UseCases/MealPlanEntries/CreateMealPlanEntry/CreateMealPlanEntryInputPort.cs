using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.MealPlanEntries.CreateMealPlanEntry;

public record CreateMealPlanEntryInputPort(DateTime Date, long? MealSlotID, long RecipeID)
    : IInputPort<ICreateMealPlanEntryOutputPort>;
