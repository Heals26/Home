using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.Activities.SetActivityCompletion;

/// <summary>
/// Ticking a chore off wherever it appears. Which column means "done" is the household's own
/// decision, so the caller says only whether it is finished and this works out where the card
/// belongs — the board, the week, the day and the dashboard all get the same behaviour without
/// any of them needing to know the board's shape.
/// </summary>
internal class SetActivityCompletionInteractor
    : IInteractor<SetActivityCompletionInputPort, ISetActivityCompletionOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SetActivityCompletionInputPort inputPort,
        ISetActivityCompletionOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _ActivityLogic = serviceFactory.GetService<IActivityLogic>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<Activity>>();
        var _TimeProvider = serviceFactory.GetService<TimeProvider>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Activity = _PersistenceContext.GetEntities<Activity>()
            .Where(a => a.ActivityID == inputPort.ActivityID && a.Household.HouseholdID == _Household.HouseholdID)
            .Select(a => new { Activity = a, a.State })
            .SingleOrDefault()
            ?.Activity;

        if (_Activity == null)
        {
            await outputPort.PresentActivityNotFoundAsync(inputPort.ActivityID, cancellationToken);
            return;
        }

        var _Columns = _PersistenceContext.GetEntities<ActivityState>()
            .Where(s => s.Household.HouseholdID == _Household.HouseholdID)
            .OrderBy(s => s.Sequence)
            .ToList();

        var _Target = inputPort.IsComplete
            ? _Columns.FirstOrDefault(s => s.IsComplete)
            : _Columns.FirstOrDefault(s => !s.IsComplete);

        if (_Target != null)
        {
            _ActivityLogic.ApplyStateChange(_Activity, _Target);
        }
        else
        {
            // A board with no column of that kind still has to be able to tick something off.
            _Activity.CompletedDateUTC = inputPort.IsComplete
                ? _TimeProvider.GetUtcNow().UtcDateTime
                : null;
        }

        _AuditLogic.UpdateAudit(_Activity);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityCompletionSetAsync(cancellationToken);
    }

    #endregion Methods

}
