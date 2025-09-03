using Home.Domain.Enumerations;

namespace Home.Domain.Entities;

public class Audit
{

    #region Properties

    public long AuditID { get; set; }
    public string Content { get; set; }
    public ResourceTypeSE Entity { get; set; }
    public long EntityID { get; set; }
    public DateTime ModifiedDateUTC { get; set; }
    public string UserName { get; set; }

    public User User { get; set; }

    #endregion Properties

}
