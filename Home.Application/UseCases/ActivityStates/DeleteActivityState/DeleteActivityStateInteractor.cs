using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityStates.DeleteActivityState;

internal class DeleteActivityStateInteractor
    : IInteractor<DeleteActivityStateInputPort, IDeleteActivityStateOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteActivityStateInputPort inputPort,
        IDeleteActivityStateOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _ActivityLogic = serviceFactory.GetService<IActivityLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ActivityState = _PersistenceContext.GetEntities<ActivityState>()
            .Where(s => s.ActivityStateID == inputPort.ActivityStateID
                && s.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_ActivityState == null)
        {
            await outputPort.PresentActivityStateNotFoundAsync(inputPort.ActivityStateID, cancellationToken);
            return;
        }

        // A board with no columns has nowhere to put a card and no way back through the UI.
        var _ColumnCount = _PersistenceContext.GetEntities<ActivityState>()
            .Count(s => s.Household.HouseholdID == _Household.HouseholdID);

        if (_ColumnCount <= 1)
        {
            _ = await outputPort.PresentLastActivityStateAsync(cancellationToken);
            return;
        }

        var _TargetState = _PersistenceContext.GetEntities<ActivityState>()
            .Where(s => s.ActivityStateID == inputPort.MoveCardsToStateID
                && s.ActivityStateID != inputPort.ActivityStateID
                && s.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_TargetState == null)
        {
            await outputPort.PresentTargetActivityStateNotFoundAsync(inputPort.MoveCardsToStateID, cancellationToken);
            return;
        }

        var _Activities = _PersistenceContext.GetEntities<Activity>()
            .Where(a => a.Household.HouseholdID == _Household.HouseholdID
                && a.State != null
                && a.State.ActivityStateID == _ActivityState.ActivityStateID)
            .ToList();

        foreach (var _Activity in _Activities)
            _ActivityLogic.ApplyStateChange(_Activity, _TargetState);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        // Another device could have dropped a card in here while the move was running, and the
        // foreign key would then reject the delete — report that instead of a 500.
        var _IsStillInUse = _PersistenceContext.GetEntities<Activity>()
            .Any(a => a.State != null && a.State.ActivityStateID == _ActivityState.ActivityStateID);

        if (_IsStillInUse)
        {
            _ = await outputPort.PresentActivityStateInUseAsync(inputPort.ActivityStateID, cancellationToken);
            return;
        }

        _PersistenceContext.Remove(_ActivityState);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityStateDeletedAsync(cancellationToken);
    }

    #endregion Methods

}
