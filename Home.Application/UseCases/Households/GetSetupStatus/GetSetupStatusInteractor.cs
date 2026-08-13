using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Households.GetSetupStatus;

/// <summary>
/// Setup is required only while the database has no users at all — the state a fresh
/// install starts in. The login page uses this to offer first-run registration.
/// </summary>
internal class GetSetupStatusInteractor : IInteractor<GetSetupStatusInputPort, IGetSetupStatusOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetSetupStatusInputPort inputPort,
        IGetSetupStatusOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        await outputPort.PresentSetupStatusAsync(!_PersistenceContext.GetEntities<User>().Any(), cancellationToken);
    }

    #endregion Methods

}
