using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeIngredients.AddRecipeIngredient;

internal class AddRecipeIngredientInteractor : IInteractor<AddRecipeIngredientInputPort, IAddRecipeIngredientOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        AddRecipeIngredientInputPort inputPort,
        IAddRecipeIngredientOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Recipe = _PersistenceContext.GetEntities<Recipe>()
            .Where(r => r.RecipeID == inputPort.RecipeID
                && r.Household.HouseholdID == _Household.HouseholdID)
            .Select(r => new
            {
                Recipe = r,
                r.Ingredients
            })
            .SingleOrDefault()
            ?.Recipe;

        if (_Recipe == null)
        {
            await outputPort.PresentRecipeNotFoundAsync(inputPort.RecipeID, cancellationToken);
            return;
        }

        var _Ingredient = new Ingredient()
        {
            Amount = inputPort.Amount,
            Name = inputPort.Name,
            Unit = inputPort.Unit
        };

        _PersistenceContext.Add(_Ingredient);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        // A new ingredient goes on the end, because a recipe is written in the order it is cooked.
        // Computed here rather than sent by the caller so two people adding at once cannot land on
        // the same position.
        var _NextSequence = _Recipe.Ingredients.Count == 0
            ? 1
            : _Recipe.Ingredients.Max(ri => ri.Sequence) + 1;

        _Recipe.Ingredients.Add(new RecipeIngredient()
        {
            IngredientID = _Ingredient.IngredientID,
            RecipeID = _Recipe.RecipeID,
            Sequence = _NextSequence
        });

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeIngredientAddedAsync(_Ingredient.IngredientID, cancellationToken);
    }

    #endregion Methods

}
