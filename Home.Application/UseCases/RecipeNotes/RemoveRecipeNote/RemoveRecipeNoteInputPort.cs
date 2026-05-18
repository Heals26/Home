using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeNotes.RemoveRecipeNote;

public record RemoveRecipeNoteInputPort(long NoteID, long RecipeID) : IInputPort<IRemoveRecipeNoteOutputPort>;
