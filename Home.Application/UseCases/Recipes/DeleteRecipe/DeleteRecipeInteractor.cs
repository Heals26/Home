using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.DeleteRecipe;

internal class DeleteRecipeInteractor : IInteractor<DeleteRecipeInputPort, IDeleteRecipeOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteRecipeInputPort input,
        IDeleteRecipeOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        _PersistenceContext.GetEntities<Recipe>()
            .Where(r => r.RecipeID == input.RecipeID)
            .ToList()
            .ForEach(r => _PersistenceContext.Remove(r));

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await output.PresentRecipeDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
