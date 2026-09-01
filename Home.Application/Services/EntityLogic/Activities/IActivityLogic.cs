using Home.Application.UseCases.ActivityContents.CreateActivityContent;
using Home.Application.UseCases.ActivityContents.UpdateActivityContent;
using Home.Application.UseCases.ActivityRegions.CreateActivityRegion;
using Home.Application.UseCases.ActivityRegions.UpdateActivityRegion;
using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.Activities;

public interface IActivityLogic
{

    #region Methods

    /// <summary>
    /// Null when the section does not belong to the card's household.
    /// </summary>
    ActivityRegion? AddRegion(CreateActivityRegionInputPort inputPort);
    ActivityContent AddContent(CreateActivityContentInputPort inputPort);

    /// <summary>
    /// Moves an activity to a column and keeps <see cref="Activity.CompletedDateUTC"/> in step:
    /// landing in a completed column stamps it, leaving one clears it.
    /// </summary>
    void ApplyStateChange(Activity activity, ActivityState? state);

    bool DoesActivityRegionExist(long activityRegionID);
    bool DoesActivityContentExist(long activityContentID);
    void UpdateRegion(UpdateActivityRegionInputPort inputPort);
    void UpdateContent(UpdateActivityContentInputPort inputPort);

    #endregion Methods

}
