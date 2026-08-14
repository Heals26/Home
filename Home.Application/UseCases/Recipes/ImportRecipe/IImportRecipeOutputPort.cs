using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Recipes.ImportRecipe;

public interface IImportRecipeOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentRecipeImportedAsync(long recipeID, CancellationToken cancellationToken);
    Task PresentRecipeImportFailedAsync(string url, CancellationToken cancellationToken);

    #endregion Methods

}
