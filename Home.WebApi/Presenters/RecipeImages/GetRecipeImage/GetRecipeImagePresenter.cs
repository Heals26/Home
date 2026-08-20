using AutoMapper;
using Home.Application.UseCases.RecipeImages.GetRecipeImage;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.RecipeImages.GetRecipeImage;

public class GetRecipeImagePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetRecipeImageOutputPort
{

    #region Methods

    Task IGetRecipeImageOutputPort.PresentRecipeImageAsync(byte[] content, string contentType, CancellationToken cancellationToken)
        => this.OkAsync(new MemoryStream(content), contentType, cancellationToken);

    Task IGetRecipeImageOutputPort.PresentRecipeImageNotFoundAsync(long recipeID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe {recipeID} has no photo.", cancellationToken);

    #endregion Methods

}
