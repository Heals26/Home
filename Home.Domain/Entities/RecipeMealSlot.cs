using Home.Domain.Deletions;

namespace Home.Domain.Entities;

/// <summary>
/// Which meals a recipe suits. Many-to-many deliberately, because pancakes are breakfast and dessert.
/// </summary>
public class RecipeMealSlot : ISoftDeletable
{

    #region Properties

    public long MealSlotID { get; set; }
    public long RecipeID { get; set; }

    public DateTime? DeletedOnUTC { get; set; }

    public MealSlot MealSlot { get; set; } = null!;
    public Recipe Recipe { get; set; } = null!;

    #endregion Properties

}
