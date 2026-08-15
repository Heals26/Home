using Home.WebUI.DataAccess.Activities.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Activities;

public partial class ActivityCard
{

    #region Properties

    [Parameter] public ActivitySummaryDto Activity { get; set; } = new();
    [Parameter] public bool Draggable { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }
    [Parameter] public EventCallback OnDragStart { get; set; }
    [Parameter] public EventCallback OnOpen { get; set; }
    /// <summary>
    /// The calendar views already say which day a card sits under, so they turn the date off.
    /// </summary>
    [Parameter] public bool ShowDate { get; set; } = true;

    #endregion Properties

}
