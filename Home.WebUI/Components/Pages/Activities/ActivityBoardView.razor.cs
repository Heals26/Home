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

    /// <summary>
    /// Whether the board is currently narrowed, so an empty column can tell the difference between
    /// "nothing here" and "nothing here that you asked for".
    /// </summary>
    [Parameter] public bool IsFiltered { get; set; }

    [Parameter] public EventCallback OnAdd { get; set; }
    [Parameter] public EventCallback OnClearFilters { get; set; }
    [Parameter] public EventCallback<ActivitySummaryDto> OnEdit { get; set; }
    [Parameter] public EventCallback<ActivityMove> OnMove { get; set; }
    [Parameter] public EventCallback<ActivityReorder> OnReorder { get; set; }
    [Parameter] public EventCallback<ActivitySummaryDto> OnOpen { get; set; }
    [Parameter] public EventCallback<ActivityCompletion> OnToggleComplete { get; set; }
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

    /// <summary>
    /// A column's cards in the order the family put them. Title breaks a tie so two cards that
    /// have never been moved still come back in the same order every render.
    /// </summary>
    private List<ActivitySummaryDto> CardsFor(long? stateID)
    {
        var _Cards = stateID == null
            ? this.Activities.Where(a => a.StateID == null || !this.IsKnownState(a.StateID.Value))
            : this.Activities.Where(a => a.StateID == stateID);

        return [.. _Cards.OrderBy(a => a.Sequence).ThenBy(a => a.Title)];
    }

    /// <summary>
    /// Swaps a card with the one above or below it in the same column, the same two-call sequence
    /// swap used everywhere else that has an order.
    /// </summary>
    private async Task MoveWithinColumnAsync(ActivitySummaryDto activity, long? stateID, int direction)
    {
        if (this.Saving)
            return;

        var _Cards = this.CardsFor(stateID);
        var _Index = _Cards.FindIndex(a => a.ActivityID == activity.ActivityID);
        var _TargetIndex = _Index + direction;

        if (_Index < 0 || _TargetIndex < 0 || _TargetIndex >= _Cards.Count)
            return;

        await this.OnReorder.InvokeAsync(new ActivityReorder(activity, _Cards[_TargetIndex]));
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
