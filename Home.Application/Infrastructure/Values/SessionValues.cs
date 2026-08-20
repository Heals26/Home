namespace Home.Application.Infrastructure.Values;

public static class SessionValues
{

    #region Fields

    /// <summary>
    /// How long an access token is accepted for, measured from the row's DateSetUTC. Owned here
    /// because both the bearer handler and the refresh grant read it, and the two drifting apart
    /// would mean tokens the handler refuses but the grant declines to replace.
    /// </summary>
    public static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromHours(1);

    /// <summary>
    /// A refresh only mints a new access token when the current one has less life than this left.
    /// Every tab on a device shares one session row, so an always-minting refresh would have two
    /// tabs invalidating each other's token every hour; below the floor they converge on the same
    /// one instead.
    /// </summary>
    public static readonly TimeSpan AccessTokenReissueFloor = TimeSpan.FromMinutes(5);

    /// <summary>
    /// How long a household stays signed in without touching a password. Slides forward on every
    /// refresh, so a tablet in daily use never asks again.
    /// </summary>
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(90);

    #endregion Fields

}
