using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Users.DeleteUser;

internal class DeleteUserInteractor : IInteractor<DeleteUserInputPort, IDeleteUserOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteUserInputPort input,
        IDeleteUserOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _User = _PersistenceContext.GetEntities<User>()
            .SingleOrDefault(u => u.UserID == input.UserID && u.Household.HouseholdID == _Household.HouseholdID);

        if (_User != null)
            _PersistenceContext.Remove(_User);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await output.PresentUserDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
