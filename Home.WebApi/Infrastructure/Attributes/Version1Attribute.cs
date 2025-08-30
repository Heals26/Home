using Asp.Versioning;

namespace Home.WebApi.Infrastructure.Attributes;

public class Version1Attribute : ApiVersionAttribute
{

    #region Constructors

    public Version1Attribute() : base("1.0") { }

    #endregion Constructors

}
