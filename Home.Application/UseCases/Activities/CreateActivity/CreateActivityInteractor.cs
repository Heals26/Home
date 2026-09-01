using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Activities;
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
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _ActivityLogic = serviceFactory.GetService<IActivityLogic>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<Activity>>();

        var _Household = _AuthorisationService.GetHousehold();

        // Columns belong to a household, so a guessed ID has to miss rather than land on
        // another family's board.
        var _State = inputPort.StateID.HasValue
            ? _PersistenceContext.GetEntities<ActivityState>()
                .SingleOrDefault(s => s.ActivityStateID == inputPort.StateID.Value && s.Household.HouseholdID == _Household.HouseholdID)
            : null;

        var _Activity = new Activity()
        {
            Audits = [],
            DueDateUTC = inputPort.DueDateUTC,
            DueTime = inputPort.DueTime,
            Household = _Household,
            Regions = [],
            Tags = [],
            Title = inputPort.Title,
            User = inputPort.UserID.HasValue
                ? _PersistenceContext.GetEntities<User>()
                    .SingleOrDefault(u => u.UserID == inputPort.UserID.Value && u.Household.HouseholdID == _Household.HouseholdID)
                : null
        };

        _ActivityLogic.ApplyStateChange(_Activity, _State);

        _PersistenceContext.Add(_Activity);
        _AuditLogic.AddAudit(_Activity);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityCreatedAsync(_Activity.ActivityID, cancellationToken);
    }

    #endregion Methods

}
