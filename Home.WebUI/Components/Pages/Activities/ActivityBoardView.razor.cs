using Home.WebUI.DataAccess.Activities.Models;
using Home.WebUI.DataAccess.ActivityStates.GetActivityStates;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Activities;

public partial class ActivityBoardView
{

    #region Records

    private record ColumnVm(string Title, long? StateID);

    #endregion Records

    #region Fields

    private List<ColumnVm> m_Columns = [];
    private ActivitySummaryDto? m_DraggedActivity;

    #endregion Fields

    #region Properties

    [Parameter] public List<ActivitySummaryDto> Activities { get; set; } = [];
    [Parameter] public EventCallback<ActivitySummaryDto> OnEdit { get; set; }
    [Parameter] public EventCallback<ActivityMove> OnMove { get; set; }
    [Parameter] public EventCallback<ActivitySummaryDto> OnOpen { get; set; }
    [Parameter] public bool Saving { get; set; }
    [Parameter] public List<ActivityStateDto> States { get; set; } = [];

    #endregion Properties

    #region Lifecycle Methods

    protected override void OnParametersSet()
    {
        // Always rendered, so there is somewhere to move a card to un-sort it.
        this.m_Columns = [new ColumnVm("Not sorted", null)];

        foreach (var _State in this.States.OrderBy(s => s.Sequence))
            this.m_Columns.Add(new ColumnVm(_State.Name, _State.ActivityStateID));
    }

    #endregion Lifecycle Methods

    #region Methods

    private List<ActivitySummaryDto> CardsFor(long? stateID)
    {
        if (stateID == null)
            return [.. this.Activities.Where(a => a.StateID == null || !this.IsKnownState(a.StateID.Value))];

        return [.. this.Activities.Where(a => a.StateID == stateID)];
    }

    private bool IsKnownState(long stateID)
        => this.States.Any(s => s.ActivityStateID == stateID);

    private void OnDragStart(ActivitySummaryDto activity)
        => this.m_DraggedActivity = activity;

    private async Task OnDropAsync(long? stateID)
    {
        var _Activity = this.m_DraggedActivity;
        this.m_DraggedActivity = null;

        if (_Activity == null || _Activity.StateID == stateID)
            return;

        await this.OnMove.InvokeAsync(new ActivityMove(_Activity, stateID));
    }

    private async Task MoveToColumnAsync(ActivitySummaryDto activity, ColumnVm? column)
    {
        if (column == null)
            return;

        await this.OnMove.InvokeAsync(new ActivityMove(activity, column.StateID));
    }

    private static string MoveLabel(ColumnVm? column, string whenUnavailable)
        => column == null ? whenUnavailable : $"Move to {column.Title}";

    #endregion Methods

}
