using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
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

        var _Ingredient = _PersistenceContext.Find<Ingredient>(inputPort.IngredientID);

        if (_Ingredient == null)
        {
            await outputPort.PresentRecipeIngredientNotFoundAsync(inputPort.IngredientID, cancellationToken);
            return;
        }

        if (inputPort.Name.HasBeenSet)
            _Ingredient.Name = inputPort.Name.Value;

        if (inputPort.Quantity.HasBeenSet)
            _Ingredient.Quantity = inputPort.Quantity.Value;

        if (inputPort.Volume.HasBeenSet)
            _Ingredient.Volume = inputPort.Volume.Value;

        if (inputPort.Weight.HasBeenSet)
            _Ingredient.Weight = inputPort.Weight.Value;

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeIngredientNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
