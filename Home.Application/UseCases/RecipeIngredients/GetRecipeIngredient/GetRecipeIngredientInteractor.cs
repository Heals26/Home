using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
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
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Ingredient = _PersistenceContext.GetEntities<Ingredient>()
            .Where(i => i.IngredientID == inputPort.IngredientID
                && i.Recipes.Any(ri => ri.Recipe.Household.HouseholdID == _Household.HouseholdID))
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
