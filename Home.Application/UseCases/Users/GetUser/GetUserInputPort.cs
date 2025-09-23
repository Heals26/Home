using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Users.GetUser;

public record GetUserInputPort(long UserID) : IInputPort<IGetUserOutputPort>;
