using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeSteps.AddRecipeStep;

public record AddRecipeStepInputPort(string Content, long RecipeID, int Sequence, string Title)
    : IInputPort<IAddRecipeStepOutputPort>;
