using AutoMapper;
using Home.Application.UseCases.IngredientNotes.RemoveIngredientNote;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.IngredientNotes.RemoveIngredientNote;

public class RemoveIngredientNotePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IRemoveIngredientNoteOutputPort
{

    #region Methods

    Task IRemoveIngredientNoteOutputPort.PresentIngredientNoteNotFoundAsync(long noteID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Ingredient Note {noteID} Not Found", cancellationToken);

    Task IRemoveIngredientNoteOutputPort.PresentIngredientNoteRemovedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
