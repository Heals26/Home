using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.Components.Shared.Inputs;
using Home.WebUI.DataAccess.ActivityStates.CreateActivityState;
using Home.WebUI.DataAccess.ActivityStates.GetActivityStates;
using Home.WebUI.DataAccess.ActivityStates.UpdateActivityState;
using Home.WebUI.DataAccess.Tags.CreateTag;
using Home.WebUI.DataAccess.Tags.GetTags;
using Home.WebUI.DataAccess.Tags.Models;
using Home.WebUI.DataAccess.Tags.UpdateTag;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.ChangeTrackers;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Activities;

public partial class BoardSettingsModal
{

    #region Fields

    private static readonly List<HomeSegmentedControl<string>.SegmentOption> Tabs =
    [
        new("Columns", "columns"),
        new("Labels", "labels"),
    ];

    private const string DefaultTagColour = "#7dd3fc";

    private bool m_WasVisible;
    private string m_Tab = "columns";
    private bool m_Saving;

    private List<ActivityStateDto> m_States = [];
    private List<TagDto> m_Tags = [];

    // Columns
    private string m_NewColumnName = string.Empty;
    private long? m_DeletingStateID;
    private long m_MoveCardsToStateID;

    // Labels
    private long? m_ColouringTagID;
    private string m_NewTagName = string.Empty;
    private string m_NewTagColour = DefaultTagColour;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }
    [Parameter] public ErrorHandler? ErrorHandler { get; set; }
    /// <summary>
    /// Raised whenever the board's vocabulary changed, so the page behind reloads and the rest of
    /// the household is told.
    /// </summary>
    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnParametersSetAsync()
    {
        if (this.Visible == this.m_WasVisible)
            return;

        this.m_WasVisible = this.Visible;

        if (this.Visible)
            await this.LoadAsync();
    }

    #endregion Lifecycle Methods

    #region Methods

    private async Task OnVisibleChangedAsync(bool visible)
        => await this.VisibleChanged.InvokeAsync(visible);

    private void SelectTab(string tab)
    {
        this.m_Tab = tab;
        this.m_DeletingStateID = null;
        this.m_ColouringTagID = null;
    }

    private async Task LoadAsync()
    {
        this.m_DeletingStateID = null;
        this.m_ColouringTagID = null;
        this.m_NewColumnName = string.Empty;
        this.m_NewTagName = string.Empty;
        this.m_NewTagColour = DefaultTagColour;

        await Task.WhenAll(this.LoadStatesAsync(), this.LoadTagsAsync());
    }

    private async Task LoadStatesAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetActivityStatesWebAppResponse>(
            null!, ApiProvider.GetActivityStates(),
            e => this.ErrorHandler?.AddError(e),
            this.CancellationToken);

        if (_Result != null)
            this.m_States = [.. _Result.States.OrderBy(s => s.Sequence)];
    }

    private async Task LoadTagsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetTagsWebAppResponse>(
            null!, ApiProvider.GetTags(),
            e => this.ErrorHandler?.AddError(e),
            this.CancellationToken);

        if (_Result != null)
            this.m_Tags = [.. _Result.Tags.OrderBy(t => t.Name)];
    }

    private async Task ReloadAndNotifyAsync()
    {
        await this.LoadStatesAsync();
        await this.LoadTagsAsync();
        await this.OnChanged.InvokeAsync();
    }

    // Columns

    private async Task RenameColumnAsync(ActivityStateDto state, string? name)
    {
        var _Name = (name ?? string.Empty).Trim();

        if (this.m_Saving || _Name.Length == 0 || _Name == state.Name)
            return;

        this.m_Saving = true;

        var _Request = new UpdateActivityStateWebAppRequest()
        {
            Name = new PropertyChangeTracker<string>(_Name)
        };

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateActivityStateWebAppRequest, bool>(
            _Request, ApiProvider.UpdateActivityState(state.ActivityStateID),
            e => this.ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Saving = false;

        if (_Result == true)
            await this.ReloadAndNotifyAsync();
    }

    /// <summary>
    /// Reordering swaps the two sequences rather than renumbering the board, so two devices
    /// shuffling at once can only ever disagree about one pair.
    /// </summary>
    private async Task MoveColumnAsync(ActivityStateDto state, int direction)
    {
        if (this.m_Saving)
            return;

        var _Index = this.m_States.IndexOf(state);
        var _TargetIndex = _Index + direction;

        if (_Index < 0 || _TargetIndex < 0 || _TargetIndex >= this.m_States.Count)
            return;

        var _Target = this.m_States[_TargetIndex];
        this.m_Saving = true;

        var _Moved = await this.SetColumnSequenceAsync(state, _Target.Sequence);

        if (_Moved)
            _ = await this.SetColumnSequenceAsync(_Target, state.Sequence);

        this.m_Saving = false;

        if (_Moved)
            await this.ReloadAndNotifyAsync();
    }

    private async Task<bool> SetColumnSequenceAsync(ActivityStateDto state, int sequence)
    {
        var _Request = new UpdateActivityStateWebAppRequest()
        {
            Sequence = new PropertyChangeTracker<int>(sequence)
        };

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateActivityStateWebAppRequest, bool>(
            _Request, ApiProvider.UpdateActivityState(state.ActivityStateID),
            e => this.ErrorHandler?.AddError(e),
            this.CancellationToken);

        return _Result == true;
    }

    private async Task SetColumnCompleteAsync(ActivityStateDto state, bool isComplete)
    {
        if (this.m_Saving)
            return;

        this.m_Saving = true;

        var _Request = new UpdateActivityStateWebAppRequest()
        {
            IsComplete = new PropertyChangeTracker<bool>(isComplete)
        };

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateActivityStateWebAppRequest, bool>(
            _Request, ApiProvider.UpdateActivityState(state.ActivityStateID),
            e => this.ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Saving = false;

        if (_Result == true)
            await this.ReloadAndNotifyAsync();
    }

    private void StartDeletingColumn(ActivityStateDto state)
    {
        var _Target = this.m_States.FirstOrDefault(s => s.ActivityStateID != state.ActivityStateID);

        if (_Target == null)
        {
            this.ErrorHandler?.AddError("A board needs at least one column, so this one cannot go.");
            return;
        }

        this.m_DeletingStateID = state.ActivityStateID;
        this.m_MoveCardsToStateID = _Target.ActivityStateID;
    }

    private void CancelDeletingColumn()
        => this.m_DeletingStateID = null;

    private async Task DeleteColumnAsync(ActivityStateDto state)
    {
        if (this.m_Saving || this.m_MoveCardsToStateID == 0)
            return;

        this.m_Saving = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteActivityState(state.ActivityStateID, this.m_MoveCardsToStateID),
            e => this.ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Saving = false;

        if (_Result != true)
            return;

        this.m_DeletingStateID = null;
        await this.ReloadAndNotifyAsync();
    }

    /// <summary>
    /// The modal shows one tab's add field at a time, so Enter means whichever one is on screen.
    /// </summary>
    private Task SubmitActiveTabAsync()
        => this.m_Tab == "columns" ? this.AddColumnAsync() : this.AddTagAsync();

    private async Task AddColumnAsync()
    {
        if (this.m_Saving || string.IsNullOrWhiteSpace(this.m_NewColumnName))
            return;

        this.m_Saving = true;

        var _Request = new CreateActivityStateWebAppRequest()
        {
            IsComplete = false,
            Name = this.m_NewColumnName.Trim()
        };

        var _Response = await this.ApiAccess.SendRequestAsync<CreateActivityStateWebAppRequest, CreateActivityStateWebAppResponse>(
            _Request, ApiProvider.CreateActivityState(),
            e => this.ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Saving = false;

        if (_Response == null)
            return;

        this.m_NewColumnName = string.Empty;
        await this.ReloadAndNotifyAsync();
    }

    // Labels

    private void ToggleTagColour(TagDto tag)
        => this.m_ColouringTagID = this.m_ColouringTagID == tag.TagID ? null : tag.TagID;

    private async Task RenameTagAsync(TagDto tag, string? name)
    {
        var _Name = (name ?? string.Empty).Trim();

        if (this.m_Saving || _Name.Length == 0 || _Name == tag.Name)
            return;

        this.m_Saving = true;

        var _Request = new UpdateTagWebAppRequest()
        {
            Name = new PropertyChangeTracker<string>(_Name)
        };

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateTagWebAppRequest, bool>(
            _Request, ApiProvider.UpdateTag(tag.TagID),
            e => this.ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Saving = false;

        if (_Result == true)
            await this.ReloadAndNotifyAsync();
    }

    private async Task RecolourTagAsync(TagDto tag, string colour)
    {
        if (this.m_Saving || !HomeColourPicker.IsValidColour(colour))
            return;

        this.m_Saving = true;

        var _Request = new UpdateTagWebAppRequest()
        {
            Colour = new PropertyChangeTracker<string>(colour)
        };

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateTagWebAppRequest, bool>(
            _Request, ApiProvider.UpdateTag(tag.TagID),
            e => this.ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Saving = false;

        if (_Result == true)
            await this.ReloadAndNotifyAsync();
    }

    private async Task DeleteTagAsync(TagDto tag)
    {
        if (this.m_Saving)
            return;

        this.m_Saving = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteTag(tag.TagID),
            e => this.ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Saving = false;

        if (_Result != true)
            return;

        this.m_ColouringTagID = null;
        await this.ReloadAndNotifyAsync();
    }

    private async Task AddTagAsync()
    {
        if (this.m_Saving || string.IsNullOrWhiteSpace(this.m_NewTagName))
            return;

        if (!HomeColourPicker.IsValidColour(this.m_NewTagColour))
        {
            this.ErrorHandler?.AddError("Pick a colour for the label first.");
            return;
        }

        this.m_Saving = true;

        var _Request = new CreateTagWebAppRequest()
        {
            Colour = this.m_NewTagColour,
            Name = this.m_NewTagName.Trim()
        };

        var _Response = await this.ApiAccess.SendRequestAsync<CreateTagWebAppRequest, CreateTagWebAppResponse>(
            _Request, ApiProvider.CreateTag(),
            e => this.ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Saving = false;

        if (_Response == null)
            return;

        this.m_NewTagName = string.Empty;
        this.m_NewTagColour = DefaultTagColour;
        await this.ReloadAndNotifyAsync();
    }

    #endregion Methods

}
