using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityStates.UpdateActivityState;

internal class UpdateActivityStateInteractor
    : IInteractor<UpdateActivityStateInputPort, IUpdateActivityStateOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateActivityStateInputPort inputPort,
        IUpdateActivityStateOutputPort outputPort,
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

        if (inputPort.Name.HasBeenSet)
            _ActivityState.Name = inputPort.Name.Value.Trim();

        if (inputPort.Sequence.HasBeenSet)
            _ActivityState.Sequence = inputPort.Sequence.Value;

        // Turning the finished flag on or off has to catch up the cards already sitting in the
        // column, or the dashboard keeps listing chores the family has stopped thinking about.
        if (inputPort.IsComplete.HasBeenSet && inputPort.IsComplete.Value != _ActivityState.IsComplete)
        {
            _ActivityState.IsComplete = inputPort.IsComplete.Value;

            var _Activities = _PersistenceContext.GetEntities<Activity>()
                .Where(a => a.Household.HouseholdID == _Household.HouseholdID
                    && a.State != null
                    && a.State.ActivityStateID == _ActivityState.ActivityStateID)
                .ToList();

            foreach (var _Activity in _Activities)
                _ActivityLogic.ApplyStateChange(_Activity, _ActivityState);
        }

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityStateUpdatedAsync(cancellationToken);
    }

    #endregion Methods

}
