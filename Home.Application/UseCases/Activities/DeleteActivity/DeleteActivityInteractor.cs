using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.Activities.DeleteActivity;

internal class DeleteActivityInteractor : IInteractor<DeleteActivityInputPort, IDeleteActivityOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteActivityInputPort input,
        IDeleteActivityOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<Activity>>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Activity = _PersistenceContext.GetEntities<Activity>()
            .SingleOrDefault(a => a.ActivityID == input.ActivityID && a.Household.HouseholdID == _Household.HouseholdID);

        if (_Activity != null)
        {
            _AuditLogic.DeleteAudit(_Activity);
            _PersistenceContext.Remove(_Activity);
        }

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await output.PresentActivityDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
