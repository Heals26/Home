using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeImages.GetRecipeImage;

public record GetRecipeImageInputPort(long RecipeID) : IInputPort<IGetRecipeImageOutputPort>;
