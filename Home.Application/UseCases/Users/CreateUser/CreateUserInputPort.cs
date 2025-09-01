using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Users.CreateUser;

public class CreateUserInputPort : IInputPort<ICreateUserOutputPort>
{

    #region Properties

    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleNames { get; set; }
    public string Password { get; set; }

    #endregion Properties

}

