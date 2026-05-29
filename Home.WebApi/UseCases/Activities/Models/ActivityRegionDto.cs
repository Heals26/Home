namespace Home.WebApi.UseCases.Activities.Models;

public class ActivityRegionDto
{

    #region Properties

    public long ActivityRegionID { get; set; }
    public string Region { get; set; }
    public int Sequence { get; set; }
    public List<ActivityContentDto> Fields { get; set; }

    #endregion Properties

}
