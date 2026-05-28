using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.Activities.CreateActivity;

internal class CreateActivityInteractor : IInteractor<CreateActivityInputPort, ICreateActivityOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateActivityInputPort inputPort,
        ICreateActivityOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<Activity>>();

        var _Activity = new Activity()
        {
            Title = inputPort.Title,
            DueDateUTC = inputPort.DueDateUTC,
            Audits = [],
            Regions = [],
            State = inputPort.StateID.HasValue
                ? _PersistenceContext.Find<ActivityState>(inputPort.StateID.Value)
                : null,
            Status = inputPort.StatusID.HasValue
                ? _PersistenceContext.Find<ActivityStatus>(inputPort.StatusID.Value)
                : null,
            User = inputPort.UserID.HasValue
                ? _PersistenceContext.Find<User>(inputPort.UserID.Value)
                : null,
        };

        _PersistenceContext.Add(_Activity);
        _AuditLogic.AddAudit(_Activity);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityCreatedAsync(_Activity.ActivityID, cancellationToken);
    }

    #endregion Methods

}
