using Home.WebApi.Infrastructure.Values;
using Microsoft.AspNetCore.Authentication;

namespace Home.WebApi.Infrastructure.OAuth;

public static class AuthenticationExtensions
{

    #region Methods

    public static AuthenticationBuilder AddBasicAuthentication(this AuthenticationBuilder builder)
        => builder.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(FrameworkValues.Basic, configureOptions: null);

    #endregion Methods

}
