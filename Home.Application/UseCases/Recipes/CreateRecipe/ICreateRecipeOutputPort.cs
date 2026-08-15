using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Recipes.CreateRecipe;

public interface ICreateRecipeOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentRecipeCreatedAsync(long recipeID, CancellationToken cancellationToken);

    #endregion Methods

}
