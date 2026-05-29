using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
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
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<Activity>>();

        var _Activity = _PersistenceContext.Find<Activity>(input.ActivityID);

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
