using AutoMapper;
using Home.Application.UseCases.RecipeImages.DeleteRecipeImage;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.RecipeImages.DeleteRecipeImage;

public class DeleteRecipeImagePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteRecipeImageOutputPort
{

    #region Methods

    Task IDeleteRecipeImageOutputPort.PresentRecipeImageDeletedNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
