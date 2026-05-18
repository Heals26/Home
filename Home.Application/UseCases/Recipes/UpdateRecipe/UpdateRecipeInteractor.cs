using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.UpdateRecipe;

internal class UpdateRecipeInteractor : IInteractor<UpdateRecipeInputPort, IUpdateRecipeOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateRecipeInputPort inputPort,
        IUpdateRecipeOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _Recipe = _PersistenceContext.Find<Recipe>(inputPort.RecipeID);

        if (_Recipe == null)
        {
            await outputPort.PresentRecipeNotFoundAsync(inputPort.RecipeID, cancellationToken);
            return;
        }

        if (inputPort.Name.HasBeenSet)
            _Recipe.Name = inputPort.Name.Value;

        if (inputPort.Url.HasBeenSet)
            _Recipe.Url = inputPort.Url.Value;

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
