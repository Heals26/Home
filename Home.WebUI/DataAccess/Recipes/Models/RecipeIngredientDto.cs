namespace Home.WebUI.DataAccess.Recipes.Models;

public class RecipeIngredientDto
{

    #region Properties

    /// <summary>
    /// How much of the ingredient, in <see cref="Unit"/>.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// The ID of the ingredient.
    /// </summary>
    public long IngredientID { get; set; }

    /// <summary>
    /// The name of the ingredient.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// What the household knows about buying this ingredient, such as a brand or which shop. Empty
    /// when there is no note.
    /// </summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the note in <see cref="Note"/>, needed to change or clear it. Null when there is
    /// no note yet, which is what tells an edit to add one instead.
    /// </summary>
    public long? NoteID { get; set; }

    /// <summary>
    /// Where it sits in this recipe's list, the order it is reached for while cooking.
    /// </summary>
    public long Sequence { get; set; }

    /// <summary>
    /// The measurement the amount is in.
    /// </summary>
    public long? Unit { get; set; }

    /// <summary>
    /// How the unit reads beside the amount, as the API resolved it.
    /// </summary>
    public string UnitAbbreviation { get; set; } = string.Empty;


    #endregion Properties

}
