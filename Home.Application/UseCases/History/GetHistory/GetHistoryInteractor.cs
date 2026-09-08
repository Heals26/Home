using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.UseCases.History.Models;
using Home.Domain.Entities;

namespace Home.Application.UseCases.History.GetHistory;

/// <summary>
/// The household's feed. Rows are scoped by their own household link, not by walking to the member
/// who wrote them, so a removed member's doings stay in the family's history.
/// </summary>
internal class GetHistoryInteractor : IInteractor<GetHistoryInputPort, IGetHistoryOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetHistoryInputPort inputPort,
        IGetHistoryOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Categories = inputPort.Categories.Count == 0
            ? Enum.GetValues<HistoryCategory>()
            : inputPort.Categories;
        var _ResourceTypes = HistoryCategories.ResourceTypesFor(_Categories);

        // One more than asked for, which is how the page knows whether an older one exists.
        var _Audits = _PersistenceContext.GetEntities<Audit>()
            .Where(a => a.Household != null && a.Household.HouseholdID == _Household.HouseholdID
                && _ResourceTypes.Contains(a.Entity)
                && (inputPort.BeforeAuditID == null || a.AuditID < inputPort.BeforeAuditID))
            .OrderByDescending(a => a.AuditID)
            .Take(inputPort.Take + 1)
            .ToList();

        var _HasMore = _Audits.Count > inputPort.Take;

        await outputPort.PresentHistoryAsync([.. _Audits.Take(inputPort.Take)], _HasMore, cancellationToken);
    }

    #endregion Methods

}
