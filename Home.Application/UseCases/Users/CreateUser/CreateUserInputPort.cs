using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Users.CreateUser;

/// <summary>
/// Adds a member. <paramref name="Email"/> and <paramref name="Password"/> come together or not at
/// all: with both the member can sign in, with neither they are someone to assign things to and
/// name on events, which is what a young child is.
/// </summary>
public record CreateUserInputPort(
    string? Email,
    string FirstName,
    string LastName,
    string MiddleNames,
    string? Password)
    : IInputPort<ICreateUserOutputPort>;
