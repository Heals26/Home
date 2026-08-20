using Home.WebUI.Infrastructure.Values;

namespace Home.WebUI.Infrastructure.Services.Security;

/// <summary>
/// What came back from the token endpoint, and whether the API actually refused us — an
/// unreachable API is not a refusal, and only a refusal may end anyone's session.
/// </summary>
public readonly record struct TokenGrantResult<TResponse>(TokenRefreshOutcome Outcome, TResponse? Token)
    where TResponse : class;
