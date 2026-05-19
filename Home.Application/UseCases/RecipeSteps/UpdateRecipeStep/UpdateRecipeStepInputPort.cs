using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.RecipeSteps.UpdateRecipeStep;

public record UpdateRecipeStepInputPort(
    long RecipeStepID,
    PropertyChangeTracker<string> Content,
    PropertyChangeTracker<int> Sequence,
    PropertyChangeTracker<string> Title)
    : IInputPort<IUpdateRecipeStepOutputPort>;
