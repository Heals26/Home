using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Home.WebUI.Infrastructure.Services.HttpClients;
using Home.WebUI.Infrastructure.Services.Undo;
using Home.WebUI.Infrastructure.Values;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Home.WebUI.Infrastructure.Undo;

/// <summary>
/// Scoped to the circuit, so the bar belongs to the tab that did the thing and stays with it from page
/// to page.
/// </summary>
public class UndoLogic(
    IChangeBroadcaster changeBroadcaster,
    IHomeHttpClient homeHttpClient,
    TimeProvider timeProvider)
    : IUndoLogic, IDisposable
{

    #region Fields

    private readonly CancellationTokenSource m_Ending = new();
    private readonly object m_Lock = new();

    private UndoBarState? m_Bar;
    private Action? m_Changed;
    private int m_Docks;
    private (UndoOffer Offer, Guid Token)? m_Offered;

    /// <summary>
    /// Moves on whenever the bar shows something new or an undo sets off, so a timer or a reply that
    /// belongs to something the bar has since moved past leaves it alone.
    /// </summary>
    private int m_Version;

    #endregion Fields

    #region Events

    event Action? IUndoLogic.Changed
    {
        add => this.m_Changed += value;
        remove => this.m_Changed -= value;
    }

    #endregion Events

    #region Properties

    UndoBarState? IUndoLogic.Bar
        => this.m_Bar;

    bool IUndoLogic.IsDocked
        => this.m_Docks > 0;

    #endregion Properties

    #region Methods

    public void Dispose()
        => this.m_Ending.Cancel();

    IDisposable IUndoLogic.Dock()
    {
        lock (this.m_Lock)
            this.m_Docks++;

        this.m_Changed?.Invoke();

        return new Docking(this.Undock);
    }

    private async Task HideAfterAsync(int version, TimeSpan delay)
    {
        try
        {
            await Task.Delay(delay, timeProvider, this.m_Ending.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        this.Show(version, null, null, TimeSpan.Zero);
    }

    async Task<TResponse?> IUndoLogic.SendRequestAsync<TRequest, TResponse>(
        TRequest request,
        ApiProviderHelper apiProvider,
        UndoOffer offer,
        Action<ValidationProblemDetails> errors,
        CancellationToken cancellationToken)
        where TResponse : default
    {
        var _Token = Guid.NewGuid();

        var _Response = await homeHttpClient.SendRequestAsync<TRequest, TResponse>(
            request, apiProvider with { UndoToken = _Token }, errors, cancellationToken);

        // A request that did not go through answers false when it answers a bool, and null otherwise.
        if (!EqualityComparer<TResponse?>.Default.Equals(_Response, default))
            this.Show(null, new(true, false, offer.Summary), (offer, _Token), UndoValues.ShownFor);

        return _Response;
    }

    /// <summary>
    /// Puts <paramref name="bar"/> up, or takes the bar down when it is null. A caller passing the
    /// <paramref name="version"/> it last saw changes nothing if the bar has moved on since, because
    /// whatever took the bar keeps it.
    /// </summary>
    private void Show(int? version, UndoBarState? bar, (UndoOffer Offer, Guid Token)? offered, TimeSpan shownFor)
    {
        int _Version;

        lock (this.m_Lock)
        {
            if (version != null && version != this.m_Version)
                return;

            this.m_Bar = bar;
            this.m_Offered = offered;
            _Version = ++this.m_Version;
        }

        this.m_Changed?.Invoke();

        if (bar != null)
            _ = this.HideAfterAsync(_Version, shownFor);
    }

    async Task IUndoLogic.UndoAsync(CancellationToken cancellationToken)
    {
        (UndoOffer Offer, Guid Token) _Offered;
        int _Version;

        lock (this.m_Lock)
        {
            if (this.m_Offered is not { } _Current || this.m_Bar is not { CanUndo: true, IsUndoing: false } _Bar)
                return;

            _Offered = _Current;
            this.m_Bar = _Bar with { IsUndoing = true };

            // The bar stays up for as long as the API takes to answer, however long that is.
            _Version = ++this.m_Version;
        }

        this.m_Changed?.Invoke();

        int? _Status = null;

        var _Undone = await homeHttpClient.SendRequestAsync<object, bool>(
            null!, ApiProvider.UndoAction(_Offered.Token), e => _Status = e.Status, cancellationToken);

        if (_Undone)
        {
            if (_Offered.Offer.OnUndone != null)
                await _Offered.Offer.OnUndone(cancellationToken);

            await changeBroadcaster.PublishAsync(_Offered.Offer.Area, cancellationToken);

            this.Show(_Version, null, null, TimeSpan.Zero);
        }
        else if (_Status is (int)HttpStatusCode.NotFound or (int)HttpStatusCode.Conflict)
            this.Show(_Version, new(false, false, "This can no longer be undone."), null, UndoValues.RefusalShownFor);
        else
            this.Show(_Version, new(true, false, "Undo did not go through. Try again."), _Offered, UndoValues.ShownFor);
    }

    private void Undock()
    {
        lock (this.m_Lock)
            this.m_Docks--;

        this.m_Changed?.Invoke();
    }

    #endregion Methods

    #region Nested Types

    private sealed class Docking(Action undock) : IDisposable
    {
        private int m_Disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.m_Disposed, 1) == 0)
                undock();
        }
    }

    #endregion Nested Types

}
