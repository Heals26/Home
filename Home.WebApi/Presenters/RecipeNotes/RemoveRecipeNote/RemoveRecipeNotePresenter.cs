using AutoMapper;
using Home.Application.UseCases.RecipeNotes.RemoveRecipeNote;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.RecipeNotes.RemoveRecipeNote;

public class RemoveRecipeNotePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IRemoveRecipeNoteOutputPort
{

    #region Methods

    Task IRemoveRecipeNoteOutputPort.PresentRecipeNoteRemovedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IRemoveRecipeNoteOutputPort.PresentRecipeNoteNotFoundAsync(long noteID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe Note {noteID} Not Found", cancellationToken);

    #endregion Methods

}
