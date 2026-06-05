namespace Home.WebUI.DataAccess.Recipes.Models;

public class RecipeNoteDto
{

    #region Properties

    /// <summary>
    /// The text content of the note.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When the note was created (UTC).
    /// </summary>
    public DateTime CreatedOnUTC { get; set; }

    /// <summary>
    /// The ID of the note.
    /// </summary>
    public long NoteID { get; set; }

    #endregion Properties

}
