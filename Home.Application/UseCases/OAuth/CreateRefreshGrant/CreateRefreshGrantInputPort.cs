using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.OAuth.CreateRefreshGrant;

public record CreateRefreshGrantInputPort(
    long ClientID,
    string ClientSecret,
    string GrantType,
    string RefreshToken)
    : IInputPort<ICreateRefreshGrantOutputPort>;
