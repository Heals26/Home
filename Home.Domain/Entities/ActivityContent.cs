using Home.Domain.Deletions;

namespace Home.Domain.Entities;

public class ActivityContent : ISoftDeletable
{

    #region Properties

    public long ActivityContentID { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime? DeletedOnUTC { get; set; }
    public int Sequence { get; set; }

    public ActivityRegion Region { get; set; } = null!;

    #endregion Properties

}
