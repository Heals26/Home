using Home.WebUI.Infrastructure.Services.Security;
using System.Collections.Concurrent;

namespace Home.WebUI.Infrastructure.Security;

/// <summary>
/// Slows down password guessing once the app is reachable from outside the house. Counted per
/// account rather than per caller: every login is made by the server on the browser's behalf, so
/// the API only ever sees this machine's address and a per-address count would be meaningless.
/// In memory and per instance, which is the right size for one household — it is a speed bump in
/// front of a public URL, not an account-security system.
/// </summary>
public class LoginThrottle(TimeProvider timeProvider) : ILoginThrottle
{

    #region Fields

    private const int MaximumAttempts = 5;

    private static readonly TimeSpan s_Lockout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan s_Window = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<string, Attempts> m_Attempts = new(StringComparer.OrdinalIgnoreCase);

    #endregion Fields

    #region Methods

    TimeSpan? ILoginThrottle.GetLockout(string username)
    {
        if (string.IsNullOrWhiteSpace(username) || !this.m_Attempts.TryGetValue(username.Trim(), out var _Attempts))
            return null;

        var _Remaining = _Attempts.LockedUntilUTC - timeProvider.GetUtcNow();

        return _Remaining > TimeSpan.Zero ? _Remaining : null;
    }

    void ILoginThrottle.RecordFailure(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return;

        var _Now = timeProvider.GetUtcNow();

        _ = this.m_Attempts.AddOrUpdate(
            username.Trim(),
            _ => new Attempts() { Count = 1, WindowStartedUTC = _Now },
            (_, existing) =>
            {
                // A quiet spell clears the slate, so an occasional typo never accumulates into
                // a lockout days later.
                if (_Now - existing.WindowStartedUTC > s_Window)
                    return new Attempts() { Count = 1, WindowStartedUTC = _Now };

                existing.Count++;

                if (existing.Count >= MaximumAttempts)
                    existing.LockedUntilUTC = _Now.Add(s_Lockout);

                return existing;
            });
    }

    void ILoginThrottle.RecordSuccess(string username)
    {
        if (!string.IsNullOrWhiteSpace(username))
            _ = this.m_Attempts.TryRemove(username.Trim(), out _);
    }

    #endregion Methods

    #region Nested Types

    private sealed class Attempts
    {
        public int Count { get; set; }
        public DateTimeOffset LockedUntilUTC { get; set; }
        public DateTimeOffset WindowStartedUTC { get; set; }
    }

    #endregion Nested Types

}
