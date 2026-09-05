namespace Home.WebUI.DataAccess.IngredientNotes.AddIngredientNote;

public class AddIngredientNoteWebAppRequest
{

    #region Properties

    /// <summary>
    /// The text content of the note.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the ingredient this note belongs to.
    /// </summary>
    public long IngredientID { get; set; }

    #endregion Properties

}
