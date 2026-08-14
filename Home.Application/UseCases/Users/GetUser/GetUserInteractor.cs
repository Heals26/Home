using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.GetUser;

internal class GetUserInteractor : IInteractor<GetUserInputPort, IGetUserOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetUserInputPort inputPort,
        IGetUserOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _User = _PersistenceContext.GetEntities<User>()
            .SingleOrDefault(u => u.UserID == inputPort.UserID && u.Household.HouseholdID == _Household.HouseholdID);

        await outputPort.PresentUserAsync(_User, cancellationToken);
    }

    #endregion Methods

}
