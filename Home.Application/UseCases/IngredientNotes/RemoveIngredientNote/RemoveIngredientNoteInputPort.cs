using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.IngredientNotes.RemoveIngredientNote;

public record RemoveIngredientNoteInputPort(long IngredientID, long NoteID)
    : IInputPort<IRemoveIngredientNoteOutputPort>;
