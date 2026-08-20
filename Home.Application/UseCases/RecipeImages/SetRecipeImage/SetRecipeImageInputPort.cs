using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeImages.SetRecipeImage;

public record SetRecipeImageInputPort(
    byte[] Content,
    long RecipeID)
    : IInputPort<ISetRecipeImageOutputPort>;
