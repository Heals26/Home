using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Recipes.SetRecipeMealSlots;

public record SetRecipeMealSlotsInputPort(IReadOnlyList<long> MealSlotIDs, long RecipeID)
    : IInputPort<ISetRecipeMealSlotsOutputPort>;
