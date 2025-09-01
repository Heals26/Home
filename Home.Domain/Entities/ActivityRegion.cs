using Home.Domain.Enumerations;

namespace Home.Domain.Entities;


public class ActivityRegion
{

    #region Properties

    public long ActivityRegionID { get; set; }
    public RegionSE Region { get; set; }
    public int Sequence { get; set; }

    public Activity Activity { get; set; }
    public ICollection<ActivityContent> Fields { get; set; }

    #endregion Properties

}
