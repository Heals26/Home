namespace Home.Application.Services.Security;

public interface ITokenFactory
{

    #region Methods

    string GetOAuthToken();

    #endregion Methods

}
