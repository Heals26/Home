using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingCategories.GetShoppingCategories;

internal class GetShoppingCategoriesInteractor : IInteractor<GetShoppingCategoriesInputPort, IGetShoppingCategoriesOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetShoppingCategoriesInputPort inputPort,
        IGetShoppingCategoriesOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ShoppingCategories = _PersistenceContext.GetEntities<ShoppingCategory>()
            .Where(c => c.Household.HouseholdID == _Household.HouseholdID)
            .OrderBy(c => c.Sequence)
            .ThenBy(c => c.Name)
            .ToList();

        await outputPort.PresentShoppingCategoriesAsync(_ShoppingCategories, cancellationToken);
    }

    #endregion Methods

}
