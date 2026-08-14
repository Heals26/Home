using Home.WebApi.UseCases.MealPlanEntries.Models;

namespace Home.WebApi.UseCases.MealPlanEntries.GetMealPlanEntries;

public class GetMealPlanEntriesApiResponse
{

    #region Properties

    public ICollection<MealPlanEntryDto> Entries { get; set; } = [];

    #endregion Properties

}
