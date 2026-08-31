namespace Home.WebUI.DataAccess.RecipeIngredients.SetRecipeIngredientSequence;

public class SetRecipeIngredientSequenceWebAppRequest
{

    #region Properties

    /// <summary>
    /// Where the ingredient should sit in this recipe's list.
    /// </summary>
    public long Sequence { get; set; }

    #endregion Properties

}
