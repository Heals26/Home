namespace Home.Domain.Entities;

public class RecipeStep
{

    #region Properties

    public long RecipeStepID { get; set; }

    public string Content { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Sequence { get; set; }

    #endregion Properties

}
