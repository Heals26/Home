using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.GetRecipes;

internal class GetRecipesInteractor : IInteractor<GetRecipesInputPort, IGetRecipesOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetRecipesInputPort input,
        IGetRecipesOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Recipes = _PersistenceContext.GetEntities<Recipe>()
            .Where(r => r.Household.HouseholdID == _Household.HouseholdID)
            .ToList();

        await output.PresentRecipesAsync(_Recipes, cancellationToken);
    }

    #endregion Methods

}
