namespace Home.Application.UseCases.ApiAuditing;

public class CreateApiAuditEntry
{

    #region Properties

    public ApiAuditEntryActionData ActionData { get; } = new ApiAuditEntryActionData();

    public ApiAuditEntryAuthenticationData AuthenticationData { get; } = new ApiAuditEntryAuthenticationData();

    public ApiAuditEntryRequestData RequestData { get; } = new ApiAuditEntryRequestData();

    public ApiAuditEntryResponseData ResponseData { get; } = new ApiAuditEntryResponseData();

    #endregion Properties

    #region Nested Classes

    public class ApiAuditEntryActionData
    {

        #region Properties

        public string ActionName { get; set; }

        public long? CreatedResourceID { get; set; }

        public string Details { get; set; }

        #endregion Properties

    }

    public class ApiAuditEntryAuthenticationData
    {

        #region Properties

        public long? AccountID { get; set; }

        public long? ClientApplicationID { get; set; }

        public string ClientName { get; set; }

        public long? CompanyID { get; set; }

        #endregion Properties

    }

    public class ApiAuditEntryRequestData
    {

        #region Properties

        public string RemoteIPAddress { get; set; }

        public string RequestBody { get; set; }

        public DateTime RequestReceivedOnUTC { get; set; }

        public string RequestUri { get; set; }

        public string UserAgent { get; set; }

        public string Version { get; set; }

        #endregion Properties

    }

    public class ApiAuditEntryResponseData
    {

        #region Properties

        public short HttpResponseStatusCode { get; set; }

        public DateTime ResponseSentOnUTC { get; set; }

        #endregion Properties

    }

    #endregion Nested Classes

}

