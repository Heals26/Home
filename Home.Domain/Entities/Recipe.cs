namespace Home.Domain.Entities;

public class Recipe
{

    #region Properties

    public long RecipeID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    public ICollection<Audit> Audits { get; set; } = [];

    /// <summary>
    /// Minutes of cooking, once the prep is done. Null when unknown.
    /// </summary>
    public int? CookMinutes { get; set; }

    /// <summary>
    /// <see cref="Enumerations.RecipeComplexitySE"/> value, or null when nobody has judged it.
    /// </summary>
    public long? Complexity { get; set; }

    public Household Household { get; set; } = null!;

    /// <summary>
    /// When the household's own photo of the dish was last uploaded, or null when there isn't
    /// one. Lives here rather than on <see cref="RecipeImage"/> so listing the book can say
    /// "has a photo" without a query ever touching the image bytes; the two are only ever
    /// written together by the image use cases.
    /// </summary>
    public DateTime? ImageUpdatedOnUTC { get; set; }

    /// <summary>
    /// A link to a picture of the finished dish — imported from the source page, or pasted in.
    /// The household's own photo beats this when both exist.
    /// </summary>
    public string? ImageUrl { get; set; }

    public ICollection<RecipeIngredient> Ingredients { get; set; } = [];

    public ICollection<RecipeMealSlot> MealSlots { get; set; } = [];

    public ICollection<RecipeNote> Notes { get; set; } = [];

    /// <summary>
    /// Minutes of hands-on preparation before cooking starts. Null when unknown.
    /// </summary>
    public int? PrepMinutes { get; set; }

    public int? Servings { get; set; }

    public ICollection<RecipeStep> Steps { get; set; } = [];

    #endregion Properties

}
