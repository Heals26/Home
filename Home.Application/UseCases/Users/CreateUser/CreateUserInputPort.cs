using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Users.CreateUser;

public class CreateUserInputPort : IInputPort<ICreateUserOutputPort>
{

    #region Properties

    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string MiddleNames { get; set; }
    public required string Password { get; set; }

    #endregion Properties

}

