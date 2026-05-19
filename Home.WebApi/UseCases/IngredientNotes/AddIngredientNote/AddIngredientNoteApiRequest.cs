namespace Home.WebApi.UseCases.IngredientNotes.AddIngredientNote;

public class AddIngredientNoteApiRequest
{

    #region Properties

    /// <summary>
    /// The text content of the note to add.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// The ID of the ingredient to add the note to.
    /// </summary>
    public long IngredientID { get; set; }

    #endregion Properties

}
