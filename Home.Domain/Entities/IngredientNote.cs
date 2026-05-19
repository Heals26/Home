namespace Home.Domain.Entities;

public class IngredientNote
{

    #region Properties

    public long NoteID { get; set; }
    public long IngredientID { get; set; }

    public Ingredient Ingredient { get; set; }
    public Note Note { get; set; }

    #endregion Properties

}
