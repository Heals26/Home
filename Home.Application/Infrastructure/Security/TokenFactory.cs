using Home.Application.Services.Security;
using System.Security.Cryptography;

namespace Home.Application.Infrastructure.Security;

public class TokenFactory : ITokenFactory
{
    #region Methods

    string ITokenFactory.GetOAuthToken()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    #endregion Methods

}
