using Home.WebUI.DataAccess.MealPlanEntries.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.MealPlan;

public partial class MealPlanCell
{

    #region Properties

    [Parameter] public string AddLabel { get; set; } = "Plan a meal";
    [Parameter] public string? Class { get; set; }
    [Parameter] public IEnumerable<MealPlanEntryDto> Entries { get; set; } = [];
    [Parameter] public EventCallback OnPlan { get; set; }
    [Parameter] public EventCallback<MealPlanEntryDto> OnRemove { get; set; }

    #endregion Properties

}
