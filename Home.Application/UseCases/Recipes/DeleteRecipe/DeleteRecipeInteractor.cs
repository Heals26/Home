using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
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
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Recipe = _PersistenceContext.GetEntities<Recipe>()
            .SingleOrDefault(r => r.RecipeID == input.RecipeID && r.Household.HouseholdID == _Household.HouseholdID);

        if (_Recipe != null)
            _PersistenceContext.Remove(_Recipe);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await output.PresentRecipeDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
