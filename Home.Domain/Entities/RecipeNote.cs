namespace Home.Domain.Entities;


public class RecipeNote
{

    #region Properties

    public long NoteID { get; set; }
    public long RecipeID { get; set; }

    public Recipe Recipe { get; set; }
    public Note Note { get; set; }

    #endregion Properties

}
