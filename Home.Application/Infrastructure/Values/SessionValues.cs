namespace Home.Application.Infrastructure.Values;

public static class SessionValues
{

    #region Fields

    /// <summary>
    /// How long a rotated refresh token keeps working. Restarting the server reconnects every
    /// open circuit at once, and they all present the same stored token within moments of each
    /// other; without this window the first one wins and the rest are signed out.
    /// </summary>
    public static readonly TimeSpan RefreshGraceWindow = TimeSpan.FromSeconds(60);

    /// <summary>
    /// How long a household stays signed in without touching a password. Slides forward on every
    /// refresh, so a tablet in daily use never asks again.
    /// </summary>
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(90);

    #endregion Fields

}
