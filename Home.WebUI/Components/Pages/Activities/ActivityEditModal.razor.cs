using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.Activities.Models;
using Home.WebUI.DataAccess.Activities.SetActivityTags;
using Home.WebUI.DataAccess.Activities.UpdateActivity;
using Home.WebUI.DataAccess.ActivityStates.GetActivityStates;
using Home.WebUI.DataAccess.Tags.Models;
using Home.WebUI.DataAccess.Users.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.ChangeTrackers;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Activities;

public partial class ActivityEditModal
{

    #region Fields

    private const string DefaultDueTime = "18:00";

    private bool m_WasVisible;
    private bool m_Saving;

    private string m_Title = string.Empty;
    private DateTime? m_DueDate;
    private bool m_HasTime;
    private string m_DueTime = DefaultDueTime;
    private long? m_StateID;
    private long? m_UserID;
    private HashSet<long> m_TagIDs = [];

    #endregion Fields

    #region Properties

    [Parameter] public ActivitySummaryDto? Activity { get; set; }
    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }
    [Parameter] public ErrorHandler? ErrorHandler { get; set; }
    [Parameter] public EventCallback OnDeleted { get; set; }
    /// <summary>
    /// Supplied only where there is somewhere else to go — the card's own page does not link to
    /// itself.
    /// </summary>
    [Parameter] public EventCallback OnOpenCard { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }
    [Parameter] public List<ActivityStateDto> States { get; set; } = [];
    [Parameter] public List<TagDto> Tags { get; set; } = [];
    [Parameter] public List<UserSummaryDto> Users { get; set; } = [];
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override void OnParametersSet()
    {
        if (this.Visible == this.m_WasVisible)
            return;

        this.m_WasVisible = this.Visible;

        if (this.Visible)
            this.Seed();
    }

    #endregion Lifecycle Methods

    #region Methods

    private void Seed()
    {
        var _Activity = this.Activity;

        if (_Activity == null)
            return;

        this.m_Title = _Activity.Title;

        // Deliberately not truncated to the date — the time of day lives in its own column, and
        // rounding the date here made a saved time look like a change and get wiped.
        this.m_DueDate = _Activity.DueDateUTC;
        this.m_HasTime = _Activity.DueTime.HasValue;
        this.m_DueTime = _Activity.DueTime.HasValue
            ? ActivityBoardLogic.FormatTime(_Activity.DueTime)
            : DefaultDueTime;
        this.m_StateID = _Activity.StateID;
        this.m_UserID = _Activity.AssignedToUserID;
        this.m_TagIDs = [.. _Activity.Tags.Select(t => t.TagID)];
    }

    private async Task OnVisibleChangedAsync(bool visible)
        => await this.VisibleChanged.InvokeAsync(visible);

    private void ToggleTag(long tagID)
    {
        if (!this.m_TagIDs.Add(tagID))
            _ = this.m_TagIDs.Remove(tagID);
    }

    private async Task SaveAsync()
    {
        if (this.m_Saving || this.Activity == null) return;

        // Only what this form actually altered, so an untouched field cannot revert a change
        // made elsewhere in the household.
        var _Request = new UpdateActivityWebAppRequest();
        var _HasChanges = false;

        if (this.m_Title != this.Activity.Title)
        {
            _Request.Title = new PropertyChangeTracker<string>(this.m_Title);
            _HasChanges = true;
        }

        if (this.m_DueDate != this.Activity.DueDateUTC)
        {
            _Request.DueDateUTC = new PropertyChangeTracker<DateTime?>(this.m_DueDate);
            _HasChanges = true;
        }

        // A time with no day would be saved and never shown anywhere, so the date gates it.
        var _DueTime = this.m_DueDate.HasValue && this.m_HasTime
            ? ActivityBoardLogic.ParseTime(this.m_DueTime)
            : null;

        if (_DueTime != this.Activity.DueTime)
        {
            _Request.DueTime = new PropertyChangeTracker<TimeSpan?>(_DueTime);
            _HasChanges = true;
        }

        if (this.m_StateID != this.Activity.StateID)
        {
            _Request.StateID = new PropertyChangeTracker<long?>(this.m_StateID);
            _HasChanges = true;
        }

        if (this.m_UserID != this.Activity.AssignedToUserID)
        {
            _Request.UserID = new PropertyChangeTracker<long?>(this.m_UserID);
            _HasChanges = true;
        }

        var _TagsChanged = !this.m_TagIDs.SetEquals(this.Activity.Tags.Select(t => t.TagID));

        if (!_HasChanges && !_TagsChanged)
        {
            await this.OnVisibleChangedAsync(false);
            return;
        }

        this.m_Saving = true;
        var _Saved = true;

        if (_HasChanges)
        {
            var _Result = await this.ApiAccess.SendRequestAsync<UpdateActivityWebAppRequest, bool>(
                _Request, ApiProvider.UpdateActivity(this.Activity.ActivityID),
                e => this.ErrorHandler?.AddError(e),
                this.CancellationToken);

            _Saved = _Result == true;
        }

        if (_Saved && _TagsChanged)
            _Saved = await this.SaveTagsAsync(this.Activity.ActivityID);

        this.m_Saving = false;

        if (!_Saved) return;

        await this.OnVisibleChangedAsync(false);
        await this.OnSaved.InvokeAsync();
    }

    private async Task<bool> SaveTagsAsync(long activityID)
    {
        var _Request = new SetActivityTagsWebAppRequest()
        {
            TagIDs = [.. this.m_TagIDs]
        };

        var _Result = await this.ApiAccess.SendRequestAsync<SetActivityTagsWebAppRequest, bool>(
            _Request, ApiProvider.SetActivityTags(activityID),
            e => this.ErrorHandler?.AddError(e),
            this.CancellationToken);

        return _Result == true;
    }

    private async Task DeleteAsync()
    {
        if (this.m_Saving || this.Activity == null) return;
        this.m_Saving = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteActivity(this.Activity.ActivityID),
            e => this.ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Saving = false;

        if (_Result != true) return;

        await this.OnVisibleChangedAsync(false);
        await this.OnDeleted.InvokeAsync();
    }

    #endregion Methods

}
