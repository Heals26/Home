using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
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

        var _Recipe = _PersistenceContext.GetEntities<Recipe>()
            .Where(r => r.RecipeID == inputPort.RecipeID)
            .Select(r => new { Recipe = r, r.Ingredients })
            .SingleOrDefault()
            ?.Recipe;

        if (_Recipe == null)
        {
            await outputPort.PresentRecipeNotFoundAsync(inputPort.RecipeID, cancellationToken);
            return;
        }

        var _Ingredient = new Ingredient()
        {
            Name = inputPort.Name,
            Quantity = inputPort.Quantity,
            Volume = inputPort.Volume,
            Weight = inputPort.Weight
        };

        _PersistenceContext.Add(_Ingredient);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        _Recipe.Ingredients.Add(new RecipeIngredient()
        {
            IngredientID = _Ingredient.IngredientID,
            RecipeID = _Recipe.RecipeID
        });

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeIngredientAddedAsync(_Ingredient.IngredientID, cancellationToken);
    }

    #endregion Methods

}
