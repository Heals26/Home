using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityContents.GetActivityContent;

internal class GetActivityContentInteractor : IInteractor<GetActivityContentInputPort, IGetActivityContentOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetActivityContentInputPort inputPort,
        IGetActivityContentOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _ActivityContent = _PersistenceContext.Find<ActivityContent>(inputPort.ActivityContentID);

        if (_ActivityContent == null)
            await outputPort.PresentActivityContentNotFoundAsync(inputPort.ActivityContentID, cancellationToken);
        else
            await outputPort.PresentActivityContentAsync(_ActivityContent, cancellationToken);
    }

    #endregion Methods

}
