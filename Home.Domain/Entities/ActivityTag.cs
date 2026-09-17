using Home.Domain.Deletions;

namespace Home.Domain.Entities;

public class ActivityTag : ISoftDeletable
{

    #region Properties

    public long ActivityID { get; set; }
    public long TagID { get; set; }

    public Activity Activity { get; set; } = null!;
    public DateTime? DeletedOnUTC { get; set; }
    public Tag Tag { get; set; } = null!;

    #endregion Properties

}
