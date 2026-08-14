using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.GetUsers;

internal class GetUsersInteractor : IInteractor<GetUsersInputPort, IGetUsersOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetUsersInputPort inputPort,
        IGetUsersOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Users = _PersistenceContext.GetEntities<User>()
            .Where(u => u.Household.HouseholdID == _Household.HouseholdID)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToList();

        await outputPort.PresentUsersAsync(_Users, cancellationToken);
    }

    #endregion Methods

}
