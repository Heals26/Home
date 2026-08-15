using Home.WebApi.UseCases.MealSlots.Models;

namespace Home.WebApi.UseCases.MealSlots.GetMealSlots;

public class GetMealSlotsApiResponse
{

    #region Properties

    public ICollection<MealSlotDto> MealSlots { get; set; } = [];

    #endregion Properties

}
