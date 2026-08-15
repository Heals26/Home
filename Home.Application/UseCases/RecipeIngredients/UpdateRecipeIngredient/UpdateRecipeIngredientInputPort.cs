using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.RecipeIngredients.UpdateRecipeIngredient;

public record UpdateRecipeIngredientInputPort(
    PropertyChangeTracker<decimal?> Amount,
    long IngredientID,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<long?> Unit)
    : IInputPort<IUpdateRecipeIngredientOutputPort>;
