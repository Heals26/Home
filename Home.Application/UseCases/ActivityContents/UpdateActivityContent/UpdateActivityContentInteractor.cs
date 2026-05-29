using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Services.Persistence;

namespace Home.Application.UseCases.ActivityContents.UpdateActivityContent;

internal class UpdateActivityContentInteractor : IInteractor<UpdateActivityContentInputPort, IUpdateActivityContentOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateActivityContentInputPort inputPort,
        IUpdateActivityContentOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _ActivityLogic = serviceFactory.GetService<IActivityLogic>();

        if (!_ActivityLogic.DoesActivityContentExist(inputPort.ActivityContentID))
        {
            await outputPort.PresentActivityContentNotFoundAsync(inputPort.ActivityContentID, cancellationToken);
            return;
        }

        _ActivityLogic.UpdateContent(inputPort);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityContentNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
