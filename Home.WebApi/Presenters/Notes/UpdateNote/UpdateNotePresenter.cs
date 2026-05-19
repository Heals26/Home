using AutoMapper;
using Home.Application.UseCases.Notes.UpdateNote;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Notes.UpdateNote;

public class UpdateNotePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateNoteOutputPort
{

    #region Methods

    Task IUpdateNoteOutputPort.PresentNoteNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateNoteOutputPort.PresentNoteNotFoundAsync(long noteID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Note {noteID} Not Found", cancellationToken);

    #endregion Methods

}
