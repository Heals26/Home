namespace Home.WebApi.UseCases.RecipeNotes.AddRecipeNote;

public record AddRecipeNoteApiRequest(string Content, long RecipeID);
