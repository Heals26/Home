namespace Home.WebApi.UseCases.Ingredients.Models;

public class IngredientNoteDto
{

    #region Properties

    /// <summary>
    /// The text content of the note.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// The UTC date and time the note was created.
    /// </summary>
    public DateTime CreatedOnUTC { get; set; }

    /// <summary>
    /// The ID of the note.
    /// </summary>
    public long NoteID { get; set; }

    #endregion Properties

}
