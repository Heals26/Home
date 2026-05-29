using Home.WebApi.UseCases.Activities.Models;

namespace Home.WebApi.UseCases.ActivityRegions.GetActivityRegion;

public class GetActivityRegionApiResponse
{

    #region Properties

    public long ActivityRegionID { get; set; }
    public string Region { get; set; }
    public int Sequence { get; set; }
    public List<ActivityContentDto> Fields { get; set; }

    #endregion Properties

}
