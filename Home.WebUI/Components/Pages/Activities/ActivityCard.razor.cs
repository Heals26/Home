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
    /// Ticking the card off in place. Left unset, the tick is not shown at all.
    /// </summary>
    [Parameter] public EventCallback<bool> OnToggleComplete { get; set; }
    /// <summary>
    /// The calendar views already say which day a card sits under, so they turn the date off.
    /// </summary>
    [Parameter] public bool ShowDate { get; set; } = true;

    private bool IsComplete
        => this.Activity.CompletedDateUTC != null;

    #endregion Properties

    #region Methods

    private async Task ToggleCompleteAsync()
        => await this.OnToggleComplete.InvokeAsync(!this.IsComplete);

    #endregion Methods

}
