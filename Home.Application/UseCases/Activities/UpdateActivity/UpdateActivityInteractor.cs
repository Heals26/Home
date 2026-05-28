using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.Activities.UpdateActivity;

internal class UpdateActivityInteractor : IInteractor<UpdateActivityInputPort, IUpdateActivityOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateActivityInputPort inputPort,
        IUpdateActivityOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<Activity>>();

        var _Activity = _PersistenceContext.Find<Activity>(inputPort.ActivityID);

        if (_Activity == null)
        {
            await outputPort.PresentActivityNotFoundAsync(inputPort.ActivityID, cancellationToken);
            return;
        }

        if (inputPort.Title.HasBeenSet)
            _Activity.Title = inputPort.Title.Value;

        if (inputPort.DueDateUTC.HasBeenSet)
            _Activity.DueDateUTC = inputPort.DueDateUTC.Value;

        if (inputPort.CompletedDateUTC.HasBeenSet)
            _Activity.CompletedDateUTC = inputPort.CompletedDateUTC.Value;

        if (inputPort.StateID.HasBeenSet)
            _Activity.State = inputPort.StateID.Value.HasValue
                ? _PersistenceContext.Find<ActivityState>(inputPort.StateID.Value.Value)
                : null;

        if (inputPort.StatusID.HasBeenSet)
            _Activity.Status = inputPort.StatusID.Value.HasValue
                ? _PersistenceContext.Find<ActivityStatus>(inputPort.StatusID.Value.Value)
                : null;

        if (inputPort.UserID.HasBeenSet)
            _Activity.User = inputPort.UserID.Value.HasValue
                ? _PersistenceContext.Find<User>(inputPort.UserID.Value.Value)
                : null;

        _AuditLogic.UpdateAudit(_Activity);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
