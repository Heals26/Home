using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeSteps.GetRecipeStep;

public record GetRecipeStepInputPort(long RecipeStepID)
    : IInputPort<IGetRecipeStepOutputPort>;
