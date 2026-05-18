using AutoMapper;
using Home.Application.UseCases.Recipes.DeleteRecipe;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Recipes.DeleteRecipe;

public class DeleteRecipePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteRecipeOutputPort
{

    #region Methods

    Task IDeleteRecipeOutputPort.PresentRecipeDeletedNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
