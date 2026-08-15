namespace Home.WebUI.DataAccess.Recipes.SetRecipeMealSlots;

public class SetRecipeMealSlotsWebAppRequest
{

    #region Properties

    /// <summary>
    /// The complete set of meals the recipe suits — whatever is sent replaces what was there.
    /// </summary>
    public List<long> MealSlotIDs { get; set; } = [];

    #endregion Properties

}
