using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Users.GetUser;

public class GetUserInputPort : IInputPort<IGetUserOutputPort>
{

    #region Properties

    public long UserID { get; set; }

    #endregion Properties

}
