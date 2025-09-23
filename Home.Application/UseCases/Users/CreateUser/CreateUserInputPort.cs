using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Users.CreateUser;

public record CreateUserInputPort(
    string Email,
    string FirstName,
    string LastName,
    string MiddleNames,
    string Password)
    : IInputPort<ICreateUserOutputPort>;
