namespace Home.WebApi.UseCases.Recipes.Models;

public class RecipeNoteDto
{

    #region Properties

    public string Content { get; set; }
    public DateTime CreatedOnUTC { get; set; }
    public long NoteID { get; set; }

    #endregion Properties

}
