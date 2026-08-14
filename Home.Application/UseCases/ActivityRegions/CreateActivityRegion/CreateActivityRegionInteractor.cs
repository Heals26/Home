using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ActivityRegions.CreateActivityRegion;

internal class CreateActivityRegionInteractor : IInteractor<CreateActivityRegionInputPort, ICreateActivityRegionOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateActivityRegionInputPort inputPort,
        ICreateActivityRegionOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<Activity>>();
        var _ActivityLogic = serviceFactory.GetService<IActivityLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Activity = _PersistenceContext.GetEntities<Activity>()
            .Where(a => a.ActivityID == inputPort.ActivityID
                && a.Household.HouseholdID == _Household.HouseholdID)
            .Select(a => new
            {
                Activity = a,
                a.Regions
            })
            .SingleOrDefault()
            ?.Activity;

        if (_Activity == null)
        {
            await outputPort.PresentActivityNotFoundAsync(inputPort.ActivityID, cancellationToken);
            return;
        }

        var _ActivityRegion = _ActivityLogic.AddRegion(inputPort);
        _Activity.Regions.Add(_ActivityRegion);

        _AuditLogic.UpdateAudit(_Activity);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityRegionCreatedAsync(_ActivityRegion.ActivityRegionID, cancellationToken);
    }

    #endregion Methods

}
