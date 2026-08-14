using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Users.GetUsers;

public record GetUsersInputPort() : IInputPort<IGetUsersOutputPort>;
