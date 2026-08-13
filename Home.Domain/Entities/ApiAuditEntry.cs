namespace Home.Domain.Entities;

public class ApiAuditEntry
{

    #region Properties

    public long ApiAuditEntryID { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public long? ClientApplicationID { get; set; }
    public long? CreatedResourceID { get; set; }
    public string Details { get; set; } = string.Empty;
    public short HttpResponseStatusCode { get; set; }
    public string RemoteIPAddress { get; set; } = string.Empty;
    public string RequestBody { get; set; } = string.Empty;
    public DateTime RequestReceivedOnUTC { get; set; }
    public string RequestUri { get; set; } = string.Empty;
    public DateTime ResponseSentOnUTC { get; set; }
    public string UserAgent { get; set; } = string.Empty;
    public long? UserID { get; set; }
    public string Version { get; set; } = string.Empty;

    public ClientApplication ClientApplication { get; set; } = null!;
    public User User { get; set; } = null!;

    #endregion Properties

}
