using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.IngredientNotes.AddIngredientNote;

public record AddIngredientNoteInputPort(string Content, long IngredientID)
    : IInputPort<IAddIngredientNoteOutputPort>;
