using AutoMapper;
using Home.Application.UseCases.IngredientNotes.AddIngredientNote;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.IngredientNotes.AddIngredientNote;

namespace Home.WebApi.Presenters.IngredientNotes.AddIngredientNote;

public class AddIngredientNotePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IAddIngredientNoteOutputPort
{

    #region Methods

    Task IAddIngredientNoteOutputPort.PresentIngredientNoteAddedAsync(long noteID, CancellationToken cancellationToken)
        => this.CreatedAsync(noteID, new AddIngredientNoteApiResponse() { NoteID = noteID }, cancellationToken);

    Task IAddIngredientNoteOutputPort.PresentIngredientNotFoundAsync(long ingredientID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Ingredient {ingredientID} Not Found", cancellationToken);

    #endregion Methods

}
