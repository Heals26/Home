using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.OAuth.CreatePasswordGrant;

public record CreatePasswordGrantInputPort(
    long ClientID,
    string ClientSecret,
    string GrantType,
    string Password,
    string Scope,
    string Username)
    : IInputPort<ICreatePasswordGrantOutputPort>;
