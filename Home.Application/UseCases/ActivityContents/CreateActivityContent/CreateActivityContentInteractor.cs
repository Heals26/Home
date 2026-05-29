using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ActivityContents.CreateActivityContent;

internal class CreateActivityContentInteractor : IInteractor<CreateActivityContentInputPort, ICreateActivityContentOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateActivityContentInputPort inputPort,
        ICreateActivityContentOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<Activity>>();
        var _ActivityLogic = serviceFactory.GetService<IActivityLogic>();

        var _Region = _PersistenceContext.GetEntities<ActivityRegion>()
            .Where(r => r.ActivityRegionID == inputPort.ActivityRegionID)
            .Select(r => new
            {
                Region = r,
                r.Activity,
                r.Fields
            })
            .SingleOrDefault()
            ?.Region;

        if (_Region == null)
        {
            await outputPort.PresentActivityRegionNotFoundAsync(inputPort.ActivityRegionID, cancellationToken);
            return;
        }

        var _ActivityContent = _ActivityLogic.AddContent(inputPort);
        _Region.Fields.Add(_ActivityContent);

        _AuditLogic.UpdateAudit(_Region.Activity);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityContentCreatedAsync(_ActivityContent.ActivityContentID, cancellationToken);
    }

    #endregion Methods

}
