using Home.Application.Services.Security;
using System.Text;

namespace Home.Application.Infrastructure.Security;

public class TokenFactory : ITokenFactory
{
    #region Methods

    string ITokenFactory.GetOAuthToken()
    {
        var _Token = new StringBuilder();
        for (var _Index = 0; _Index < 3; _Index++)
            _ = _Token.Append(Guid.NewGuid().ToString().Replace("-", string.Empty));
        return _Token.ToString();
    }

    #endregion Methods

}
