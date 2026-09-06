using Microsoft.JSInterop;

namespace Home.WebUI.Infrastructure.Circuits;

/// <summary>
/// Answers the one question the browser cannot answer for itself: is this page's circuit still
/// there?
/// <para>
/// A phone that has been locked comes back with a page that looks perfectly alive. Its socket died
/// while the tab was frozen, and nothing on the page knows yet, so every tap does nothing and no
/// overlay appears to explain why. Blazor works it out eventually, from a keepalive that was not
/// running either, which can take longer than anyone is willing to stand there for.
/// </para>
/// <para>
/// Calling this from JavaScript settles it immediately. The call only completes if the circuit is
/// alive to run it, so a rejection or a timeout is the answer, not an error. There is deliberately
/// nothing to do here: the round trip is the whole point.
/// </para>
/// </summary>
public static class CircuitPing
{

    #region Methods

    [JSInvokable("HomeCircuitPing")]
    public static bool Ping()
        => true;

    #endregion Methods

}
