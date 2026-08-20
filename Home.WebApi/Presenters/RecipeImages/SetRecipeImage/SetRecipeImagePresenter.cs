using AutoMapper;
using Home.Application.UseCases.RecipeImages.SetRecipeImage;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.RecipeImages.SetRecipeImage;

public class SetRecipeImagePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISetRecipeImageOutputPort
{

    #region Methods

    Task ISetRecipeImageOutputPort.PresentRecipeImageSetNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task ISetRecipeImageOutputPort.PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe {recipeID} was not found.", cancellationToken);

    #endregion Methods

}
