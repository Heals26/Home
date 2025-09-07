using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.OAuth.CreateRefreshGrant;

public class CreateRefreshGrantInputPort : IInputPort<ICreateRefreshGrantOutputPort>
{

    #region Properties

    public long ClientID { get; set; }
    public string ClientSecret { get; set; }
    public string GrantType { get; set; }
    public string RefreshToken { get; set; }

    #endregion Properties

}
