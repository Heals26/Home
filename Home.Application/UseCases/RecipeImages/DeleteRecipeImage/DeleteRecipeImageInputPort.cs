using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeImages.DeleteRecipeImage;

public record DeleteRecipeImageInputPort(long RecipeID) : IInputPort<IDeleteRecipeImageOutputPort>;
