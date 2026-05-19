namespace Home.Application.UseCases.Notes.UpdateNote;

public interface IUpdateNoteOutputPort
{

    #region Methods

    Task PresentNoteNoContentAsync(CancellationToken cancellationToken);
    Task PresentNoteNotFoundAsync(long noteID, CancellationToken cancellationToken);

    #endregion Methods

}
