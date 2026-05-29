using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityContents.DeleteActivityContent;

internal class DeleteActivityContentInteractor : IInteractor<DeleteActivityContentInputPort, IDeleteActivityContentOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteActivityContentInputPort inputPort,
        IDeleteActivityContentOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _ActivityContent = _PersistenceContext.Find<ActivityContent>(inputPort.ActivityContentID);

        if (_ActivityContent == null)
        {
            await outputPort.PresentActivityContentNotFoundAsync(inputPort.ActivityContentID, cancellationToken);
            return;
        }

        _PersistenceContext.Remove(_ActivityContent);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityContentDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
