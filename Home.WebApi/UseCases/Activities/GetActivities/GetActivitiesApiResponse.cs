using Home.WebApi.UseCases.Activities.Models;

namespace Home.WebApi.UseCases.Activities.GetActivities;

public class GetActivitiesApiResponse
{

    #region Properties

    public ICollection<ActivitySummaryDto> Activities { get; set; }

    #endregion Properties

}
