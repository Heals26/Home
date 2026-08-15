using Home.WebUI.DataAccess.MealSlots.Models;

namespace Home.WebUI.DataAccess.MealSlots.GetMealSlots;

public class GetMealSlotsWebAppResponse
{

    #region Properties

    /// <summary>
    /// The household's meals, ordered through the day.
    /// </summary>
    public ICollection<MealSlotDto> MealSlots { get; set; } = [];

    #endregion Properties

}
