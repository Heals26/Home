using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Users.DeleteUser;

public record DeleteUserInputPort(long UserID) : IInputPort<IDeleteUserOutputPort>;
