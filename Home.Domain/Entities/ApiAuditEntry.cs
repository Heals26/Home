namespace Home.Domain.Entities;

public class ApiAuditEntry
{

    #region Properties

    public long ApiAuditEntryID { get; set; }
    public string ActionName { get; set; }
    public long? CreatedResourceID { get; set; }
    public string Details { get; set; }
    public short HttpResponseStatusCode { get; set; }
    public string RemoteIPAddress { get; set; }
    public string RequestBody { get; set; }
    public DateTime RequestReceivedOnUTC { get; set; }
    public string RequestUri { get; set; }
    public DateTime ResponseSentOnUTC { get; set; }
    public string UserAgent { get; set; }
    public long? UserID { get; set; }
    public string Version { get; set; }

    public User User { get; set; }

    #endregion Properties

}
