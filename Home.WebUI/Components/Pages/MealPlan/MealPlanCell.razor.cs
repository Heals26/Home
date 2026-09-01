using Home.WebUI.DataAccess.MealPlanEntries.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.MealPlan;

public partial class MealPlanCell
{

    #region Properties

    [Parameter] public string AddLabel { get; set; } = "Plan a meal";
    [Parameter] public string? Class { get; set; }
    [Parameter] public IEnumerable<MealPlanEntryDto> Entries { get; set; } = [];

    /// <summary>
    /// Whether a move is already in flight, so the chevrons cannot be tapped into a queue.
    /// </summary>
    [Parameter] public bool Moving { get; set; }

    [Parameter] public EventCallback<MealPlanEntryDto> OnDragStart { get; set; }
    [Parameter] public EventCallback OnDrop { get; set; }
    [Parameter] public EventCallback OnPlan { get; set; }
    [Parameter] public EventCallback<MealPlanEntryDto> OnRemove { get; set; }
    [Parameter] public EventCallback<MealPlanShift> OnShift { get; set; }

    #endregion Properties

}
