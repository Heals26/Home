using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Notes.UpdateNote;

internal class UpdateNoteInteractor : IInteractor<UpdateNoteInputPort, IUpdateNoteOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateNoteInputPort inputPort,
        IUpdateNoteOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _Note = _PersistenceContext.Find<Note>(inputPort.NoteID);

        if (_Note == null)
        {
            await outputPort.PresentNoteNotFoundAsync(inputPort.NoteID, cancellationToken);
            return;
        }

        if (inputPort.Content.HasBeenSet)
            _Note.Content = inputPort.Content.Value;

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentNoteNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
