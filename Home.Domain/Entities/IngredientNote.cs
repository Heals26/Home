namespace Home.Domain.Entities;

public class IngredientNote
{

    #region Properties

    public long NoteID { get; set; }
    public long IngredientID { get; set; }

    public Ingredient Ingredient { get; set; } = null!;
    public Note Note { get; set; } = null!;

    #endregion Properties

}
