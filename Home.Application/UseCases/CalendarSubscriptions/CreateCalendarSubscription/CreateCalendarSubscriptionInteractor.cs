using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Calendar;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.CalendarSubscriptions.CreateCalendarSubscription;

internal class CreateCalendarSubscriptionInteractor
    : IInteractor<CreateCalendarSubscriptionInputPort, ICreateCalendarSubscriptionOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateCalendarSubscriptionInputPort inputPort,
        ICreateCalendarSubscriptionOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _SubscriptionLogic = serviceFactory.GetService<ICalendarSubscriptionLogic>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<CalendarSubscription>>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Subscription = new CalendarSubscription()
        {
            Household = _Household,
            Name = inputPort.Name.Trim(),
            Url = NormaliseUrl(inputPort.Url)
        };

        _PersistenceContext.Add(_Subscription);

        // Read it straight away so the calendar fills in while the family is still looking at the
        // Settings page. An unreachable feed is recorded on the row, not thrown.
        var _WasFetched = await _SubscriptionLogic.RefreshAsync(_Subscription, cancellationToken);

        // After the first read, so the summary carries the name the feed gave itself.
        _AuditLogic.AddAudit(_Subscription);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentCalendarSubscriptionCreatedAsync(_Subscription.CalendarSubscriptionID, _WasFetched, cancellationToken);
    }

    /// <summary>
    /// Apple publishes feeds as webcal://, which is https:// wearing a hat.
    /// </summary>
    private static string NormaliseUrl(string url)
    {
        var _Trimmed = url.Trim();

        return _Trimmed.StartsWith("webcal://", StringComparison.OrdinalIgnoreCase)
            ? string.Concat("https://", _Trimmed.AsSpan("webcal://".Length))
            : _Trimmed;
    }

    #endregion Methods

}
