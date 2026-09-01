namespace Home.WebApi.UseCases.Activities.Models;

public class ActivityRegionDto
{

    #region Properties

    public long ActivityRegionID { get; set; }
    public long CardSectionID { get; set; }
    public string CardSectionName { get; set; }
    public int Sequence { get; set; }
    public List<ActivityContentDto> Fields { get; set; }

    #endregion Properties

}
