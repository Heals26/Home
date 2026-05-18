using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeNotes.AddRecipeNote;

public record AddRecipeNoteInputPort(string Content, long RecipeID) : IInputPort<IAddRecipeNoteOutputPort>;
