using Home.WebUI.Infrastructure.Enumerations;

namespace Home.WebUI.Infrastructure.ApiProviders.Helpers;

public class RouteType : BaseEnumeration
{

    #region Fields

    public static readonly RouteType Route = new("Route", 1);
    public static readonly RouteType Body = new("Body", 2);
    public static readonly RouteType Form = new("Form", 3);

    #endregion Fields

    #region Constructors

    public RouteType(string name, long value) : base(name, value) { }

    #endregion Constructors

    #region Methods

    public static implicit operator RouteType(string name)
        => FromName<RouteType>(name);

    public static implicit operator RouteType(long value)
        => FromValue<RouteType>(value);

    #endregion Methods

}
