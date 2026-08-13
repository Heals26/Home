using Home.Domain.Enumerations;

namespace Home.Domain.Entities;

public class ActivityRegion
{

    #region Properties

    public long ActivityRegionID { get; set; }
    public RegionSE Region { get; set; } = null!;
    public int Sequence { get; set; }

    public Activity Activity { get; set; } = null!;
    public ICollection<ActivityContent> Fields { get; set; } = [];

    #endregion Properties

}
