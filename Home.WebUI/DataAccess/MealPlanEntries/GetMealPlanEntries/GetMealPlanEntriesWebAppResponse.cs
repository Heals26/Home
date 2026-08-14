using Home.WebUI.DataAccess.MealPlanEntries.Models;

namespace Home.WebUI.DataAccess.MealPlanEntries.GetMealPlanEntries;

public class GetMealPlanEntriesWebAppResponse
{

    #region Properties

    /// <summary>
    /// The planned meals inside the requested date window, ordered by date.
    /// </summary>
    public ICollection<MealPlanEntryDto> Entries { get; set; } = [];

    #endregion Properties

}
