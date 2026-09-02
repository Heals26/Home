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

        // A member of another household is a member we cannot see, which is a 404 like every other
        // slice says. Presenting the null instead answered 200 with an empty body and left
        // PresentUserNotFoundAsync unreachable.
        if (_User == null)
            await outputPort.PresentUserNotFoundAsync(inputPort.UserID, cancellationToken);
        else
            await outputPort.PresentUserAsync(_User, cancellationToken);
    }

    #endregion Methods

}
