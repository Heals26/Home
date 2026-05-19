using Home.Domain.Enumerations;

namespace Home.Application.Infrastructure.Values;

public class OAuthValues : BaseEnumeration
{

    #region Fields

    public static readonly OAuthValues InvalidRequest = new("invalid_request", 1);
    public static readonly OAuthValues InvalidClient = new("invalid_client", 2);
    public static readonly OAuthValues InvalidGrant = new("invalid_grant", 3);
    public static readonly OAuthValues InvalidScope = new("invalid_scope", 4);
    public static readonly OAuthValues UnauthorizedClient = new("unauthorized_client", 5);
    public static readonly OAuthValues UnsupportedGrantType = new("unsupported_grant_type", 6);
    public static readonly OAuthValues InvalidUsernameOrPassword = new("Invalid Username or Password", 7);

    public static readonly OAuthValues GrantTypePassword = new("password", 7);
    public static readonly OAuthValues GrantTypeRefresh = new("refresh_token", 8);

    public static readonly OAuthValues WebAppScope = new("WebApp", 9);

    #endregion Fields

    #region Constructors

    public OAuthValues(string name, long value) : base(name, value) { }

    #endregion Constructors

    #region Methods

    public static implicit operator OAuthValues(string name)
        => FromName<OAuthValues>(name);

    public static implicit operator OAuthValues(long value)
        => FromValue<OAuthValues>(value);

    #endregion Methods

}
