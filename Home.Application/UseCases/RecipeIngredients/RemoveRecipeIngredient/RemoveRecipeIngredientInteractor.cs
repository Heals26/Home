using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeIngredients.RemoveRecipeIngredient;

internal class RemoveRecipeIngredientInteractor : IInteractor<RemoveRecipeIngredientInputPort, IRemoveRecipeIngredientOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        RemoveRecipeIngredientInputPort inputPort,
        IRemoveRecipeIngredientOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _Link = _PersistenceContext.GetEntities<RecipeIngredient>()
            .Where(ri => ri.RecipeID == inputPort.RecipeID && ri.IngredientID == inputPort.IngredientID)
            .SingleOrDefault();

        if (_Link == null)
        {
            await outputPort.PresentRecipeIngredientNotFoundAsync(inputPort.IngredientID, cancellationToken);
            return;
        }

        var _Ingredient = _PersistenceContext.Find<Ingredient>(inputPort.IngredientID);

        _PersistenceContext.Remove(_Link);
        if (_Ingredient != null)
            _PersistenceContext.Remove(_Ingredient);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeIngredientRemovedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
