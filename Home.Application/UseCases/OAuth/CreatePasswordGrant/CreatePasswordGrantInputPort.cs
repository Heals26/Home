using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.OAuth.CreatePasswordGrant;

public class CreatePasswordGrantInputPort : IInputPort<ICreatePasswordGrantOutputPort>
{

    #region Properties

    public long ClientID { get; set; }
    public string ClientSecret { get; set; }
    public string GrantType { get; set; }
    public string Password { get; set; }
    public string Scope { get; set; }
    public string Username { get; set; }

    #endregion Properties

}
