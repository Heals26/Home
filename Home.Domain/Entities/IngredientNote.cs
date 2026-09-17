using Home.Domain.Deletions;

namespace Home.Domain.Entities;

public class IngredientNote : ISoftDeletable
{

    #region Properties

    public long NoteID { get; set; }
    public long IngredientID { get; set; }

    public DateTime? DeletedOnUTC { get; set; }

    public Ingredient Ingredient { get; set; } = null!;
    public Note Note { get; set; } = null!;

    #endregion Properties

}
