namespace Home.Application.Infrastructure.Recipes;

public static class RecipeValues
{

    #region Fields

    /// <summary>
    /// A week. Long enough for a cure or a prove, short enough that a mis-parsed duration is
    /// still caught.
    /// </summary>
    public const int MaximumMinutes = 10080;

    public const int MaximumServings = 100;

    #endregion Fields

}
