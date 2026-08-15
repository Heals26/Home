using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeIngredients.UpdateRecipeIngredient;

internal class UpdateRecipeIngredientInteractor : IInteractor<UpdateRecipeIngredientInputPort, IUpdateRecipeIngredientOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateRecipeIngredientInputPort inputPort,
        IUpdateRecipeIngredientOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Ingredient = _PersistenceContext.GetEntities<Ingredient>()
            .Where(i => i.IngredientID == inputPort.IngredientID
                && i.Recipes.Any(ri => ri.Recipe.Household.HouseholdID == _Household.HouseholdID))
            .SingleOrDefault();

        if (_Ingredient == null)
        {
            await outputPort.PresentRecipeIngredientNotFoundAsync(inputPort.IngredientID, cancellationToken);
            return;
        }

        if (inputPort.Amount.HasBeenSet)
            _Ingredient.Amount = inputPort.Amount.Value;

        if (inputPort.Name.HasBeenSet)
            _Ingredient.Name = inputPort.Name.Value;

        if (inputPort.Unit.HasBeenSet)
            _Ingredient.Unit = inputPort.Unit.Value;

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeIngredientNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
