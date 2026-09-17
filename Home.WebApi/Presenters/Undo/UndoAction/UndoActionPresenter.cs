using AutoMapper;
using Home.Application.UseCases.Undo.UndoAction;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Undo.UndoAction;

public class UndoActionPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUndoActionOutputPort
{

    #region Methods

    Task IUndoActionOutputPort.PresentActionUndoneNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUndoActionOutputPort.PresentUndoExpiredAsync(CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task IUndoActionOutputPort.PresentUndoNotFoundAsync(Guid token, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Undo {token} Not Found", cancellationToken);

    Task IUndoActionOutputPort.PresentUndoRefusedAsync(CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    #endregion Methods

}
