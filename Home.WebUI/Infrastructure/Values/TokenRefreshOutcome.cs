namespace Home.WebUI.Infrastructure.Values;

/// <summary>
/// Why a token refresh finished. The distinction matters because only the API explicitly
/// refusing the refresh token may sign the family out — an unreachable or broken API must
/// leave the stored session intact so the tablet recovers by itself.
/// </summary>
public enum TokenRefreshOutcome
{
    /// <summary>A new access token was issued and stored.</summary>
    Refreshed,

    /// <summary>The token endpoint itself refused the refresh token. The session is dead.</summary>
    Rejected,

    /// <summary>The API could not be asked, or answered unusably. The session is unchanged.</summary>
    Unavailable,
}
