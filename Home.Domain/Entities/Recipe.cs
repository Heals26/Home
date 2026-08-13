namespace Home.Domain.Entities;

public class Recipe
{

    #region Properties

    public long RecipeID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    public ICollection<Audit> Audits { get; set; } = [];
    public Household Household { get; set; } = null!;
    public ICollection<RecipeIngredient> Ingredients { get; set; } = [];
    public ICollection<RecipeNote> Notes { get; set; } = [];
    public ICollection<RecipeStep> Steps { get; set; } = [];

    #endregion Properties

}
