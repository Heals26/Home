namespace Home.WebUI.DataAccess.RecipeNotes.AddRecipeNote;

public class AddRecipeNoteWebAppRequest
{

    #region Properties

    /// <summary>
    /// The text content of the note.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the recipe this note belongs to.
    /// </summary>
    public long RecipeID { get; set; }

    #endregion Properties

}
