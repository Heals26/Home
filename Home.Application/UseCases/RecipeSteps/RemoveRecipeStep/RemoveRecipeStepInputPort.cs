using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeSteps.RemoveRecipeStep;

public record RemoveRecipeStepInputPort(long RecipeStepID)
    : IInputPort<IRemoveRecipeStepOutputPort>;
