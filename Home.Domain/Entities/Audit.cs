using Home.Domain.Enumerations;

namespace Home.Domain.Entities;


public class Audit
{

    #region Properties

    public long AuditID { get; set; }
    public string AuditContent { get; set; }
    public DateTime AuditDateUTC { get; set; }
    public User AuditUser { get; set; }
    public string AuditUserName { get; set; }
    public ResourceTypeSE Entity { get; set; }
    public long EntityID { get; set; }

    #endregion Properties

}
