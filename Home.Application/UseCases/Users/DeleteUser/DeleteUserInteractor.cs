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

            // Both activity links to a member are NoAction, so the cards are unhooked here: the
            // chore survives, it just stops naming someone who is gone.
            foreach (var _Activity in _PersistenceContext.GetEntities<Activity>()
                .Where(a => (a.User != null && a.User.UserID == _User.UserID) || (a.CompletedByUser != null && a.CompletedByUser.UserID == _User.UserID))
                .Select(a => new { Activity = a, a.User, a.CompletedByUser })
                .ToList()
                .Select(a => a.Activity))
            {
                if (_Activity.User?.UserID == _User.UserID)
                    _Activity.User = null;

                if (_Activity.CompletedByUser?.UserID == _User.UserID)
                    _Activity.CompletedByUser = null;
            }

            _PersistenceContext.Remove(_User);
        }

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await output.PresentUserDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
