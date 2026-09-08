using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;

namespace Home.Application.UseCases.History.GetEntityHistory;

internal class GetEntityHistoryInteractor : IInteractor<GetEntityHistoryInputPort, IGetEntityHistoryOutputPort>
{

    #region Constants

    /// <summary>
    /// A chore's page does not need its whole life; the feed has that.
    /// </summary>
    private const int MaximumRows = 50;

    #endregion Constants

    #region Methods

    public async Task HandleAsync(
        GetEntityHistoryInputPort inputPort,
        IGetEntityHistoryOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();
        var _ResourceType = ResourceTypeSE.FromValue<ResourceTypeSE>(inputPort.ResourceTypeValue);

        // An unknown resource type is simply a thing with no history, and another household's thing
        // is too: the household link on the row is what keeps a guessed ID from reading anything.
        var _Audits = _ResourceType == null
            ? []
            : _PersistenceContext.GetEntities<Audit>()
                .Where(a => a.Household != null && a.Household.HouseholdID == _Household.HouseholdID
                    && a.Entity == _ResourceType
                    && a.EntityID == inputPort.EntityID)
                .OrderByDescending(a => a.AuditID)
                .Take(MaximumRows)
                .ToList();

        await outputPort.PresentEntityHistoryAsync(_Audits, cancellationToken);
    }

    #endregion Methods

}
