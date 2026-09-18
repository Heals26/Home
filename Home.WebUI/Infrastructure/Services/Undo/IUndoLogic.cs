using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebUI.Infrastructure.Services.Undo;

/// <summary>
/// Sends a tick or a delete with a token of its own and then offers, on the bar, to take it back.
/// Only this device holds the token, so only this device can undo what it did, and a newer action
/// takes the bar from an older one.
/// </summary>
public interface IUndoLogic
{

    #region Events

    /// <summary>
    /// Raised whenever <see cref="Bar"/> or <see cref="IsDocked"/> changes, sometimes off the
    /// renderer's thread.
    /// </summary>
    event Action? Changed;

    #endregion Events

    #region Properties

    /// <summary>
    /// What the bar shows, or null when there is nothing to show.
    /// </summary>
    UndoBarState? Bar { get; }

    /// <summary>
    /// Whether a bar is showing inside something of its own, such as an open dialog, where the one at
    /// the foot of the page could be neither seen nor tapped.
    /// </summary>
    bool IsDocked { get; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Holds the bar at the foot of the page back until the returned handle is disposed.
    /// </summary>
    IDisposable Dock();

    /// <summary>
    /// Sends the request the way <see cref="HttpClients.IHomeHttpClient"/> does, and once it has gone
    /// through puts <paramref name="offer"/> on the bar.
    /// </summary>
    Task<TResponse?> SendRequestAsync<TRequest, TResponse>(
        TRequest request,
        ApiProviderHelper apiProvider,
        UndoOffer offer,
        Action<ValidationProblemDetails> errors,
        CancellationToken cancellationToken);

    Task UndoAsync(CancellationToken cancellationToken);

    #endregion Methods

}
