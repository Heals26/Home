using Home.Domain.Enumerations;

namespace Home.Application.Infrastructure.Recipes;

public static class RecipeComplexityLogic
{

    #region Methods

    /// <summary>
    /// Null is defined — nobody has judged the recipe yet.
    /// </summary>
    public static bool IsDefined(long? complexity)
        => complexity == null || BaseEnumeration.FromValue<RecipeComplexitySE>(complexity.Value) != null;

    #endregion Methods

}
