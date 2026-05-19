namespace Home.WebApi.UseCases.Ingredients.Models;

public class IngredientNoteDto
{

    #region Properties

    public string Content { get; set; }
    public DateTime CreatedOnUTC { get; set; }
    public long NoteID { get; set; }

    #endregion Properties

}
