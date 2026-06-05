using Home.WebUI.DataAccess.Activities.Models;

namespace Home.WebUI.DataAccess.Activities.GetActivities;

public class GetActivitiesWebAppResponse
{

    #region Properties

    /// <summary>
    /// A collection of activities.
    /// </summary>
    public ICollection<ActivitySummaryDto> Activities { get; set; } = [];

    #endregion Properties

}
