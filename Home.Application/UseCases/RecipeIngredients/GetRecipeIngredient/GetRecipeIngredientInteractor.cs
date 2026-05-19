using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeIngredients.GetRecipeIngredient;

internal class GetRecipeIngredientInteractor : IInteractor<GetRecipeIngredientInputPort, IGetRecipeIngredientOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetRecipeIngredientInputPort inputPort,
        IGetRecipeIngredientOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _Ingredient = _PersistenceContext.GetEntities<Ingredient>()
            .Where(i => i.IngredientID == inputPort.IngredientID)
            .Select(i => new
            {
                Ingredient = i,
                Notes = i.Notes.Select(n => new
                {
                    IngredientNote = n,
                    n.Note
                })
            })
            .SingleOrDefault()
            ?.Ingredient;

        if (_Ingredient == null)
            await outputPort.PresentRecipeIngredientNotFoundAsync(inputPort.IngredientID, cancellationToken);
        else
            await outputPort.PresentRecipeIngredientAsync(_Ingredient, cancellationToken);
    }

    #endregion Methods

}
