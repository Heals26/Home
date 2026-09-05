using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.OAuth.CreatePasswordGrant;

/// <summary>
/// <paramref name="DeviceLabel"/> is what the signed-in devices screen calls this session. It comes
/// from the User-Agent, which the caller controls, so it is a label and never evidence of anything.
/// </summary>
public record CreatePasswordGrantInputPort(
    long ClientID,
    string ClientSecret,
    string DeviceLabel,
    string GrantType,
    string Password,
    string Scope,
    string Username)
    : IInputPort<ICreatePasswordGrantOutputPort>;
