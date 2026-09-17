using Home.Domain.Deletions;

namespace Home.Domain.Entities;

public class RecipeStep : ISoftDeletable
{

    #region Properties

    public long RecipeStepID { get; set; }

    public string Content { get; set; } = string.Empty;
    public DateTime? DeletedOnUTC { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Sequence { get; set; }

    #endregion Properties

}
