namespace Home.WebApi.UseCases.ActivityRegions.GetActivityRegions;

public class GetActivityRegionsApiResponse
{

    #region Properties

    public ICollection<GetActivityRegionDto> Regions { get; set; }

    #endregion Properties

}

public class GetActivityRegionDto
{

    #region Properties

    public long ActivityRegionID { get; set; }
    public string Region { get; set; }
    public int Sequence { get; set; }

    #endregion Properties

}
