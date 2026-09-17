namespace Home.Application.UseCases.Undo.UndoAction;

public interface IUndoActionOutputPort
{

    #region Methods

    Task PresentActionUndoneNoContentAsync(CancellationToken cancellationToken);
    Task PresentUndoExpiredAsync(CancellationToken cancellationToken);
    Task PresentUndoNotFoundAsync(Guid token, CancellationToken cancellationToken);
    Task PresentUndoRefusedAsync(CancellationToken cancellationToken);

    #endregion Methods

}
