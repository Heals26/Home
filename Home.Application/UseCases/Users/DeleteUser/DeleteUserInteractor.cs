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
        {
            // Calendar membership cannot cascade from the user (the household already cascades to
            // it through the event), so a departing member is taken off their events here.
            _PersistenceContext.RemoveRange(_PersistenceContext.GetEntities<CalendarEventMember>()
                .Where(m => m.UserID == _User.UserID)
                .ToList());

            _PersistenceContext.Remove(_User);
        }

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await output.PresentUserDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
