namespace Home.WebUI.Infrastructure.Values;

/// <summary>
/// How a call to the token endpoint finished. The distinction matters because only the API
/// explicitly refusing the credentials may sign the family out. An unreachable or broken API, or
/// one that refused this installation rather than the person, must leave the stored session intact
/// so the tablet recovers by itself.
/// </summary>
public enum TokenRefreshOutcome
{
    /// <summary>A new access token was issued and stored.</summary>
    Refreshed,

    /// <summary>The token endpoint refused the credentials it was given. The session is dead.</summary>
    Rejected,

    /// <summary>The API could not be asked, or answered unusably. The session is unchanged.</summary>
    Unavailable,

    /// <summary>
    /// The API refused this installation's own client credentials, not the person's password.
    /// Nothing anyone types will help and no session should end over it; the configuration is
    /// wrong. Callers that only care whether the session survived should treat this as
    /// <see cref="Unavailable"/>, which is what the default branch of every switch already does.
    /// </summary>
    ClientRejected,
}
