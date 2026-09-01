using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.CardSections.GetCardSections;

internal class GetCardSectionsInteractor : IInteractor<GetCardSectionsInputPort, IGetCardSectionsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetCardSectionsInputPort inputPort,
        IGetCardSectionsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        // Regions is projected rather than left to lazy loading, because the caller counts them to
        // decide whether a section can still be deleted — and an unloaded collection counts zero,
        // which would offer to delete a section with a card's writing under it.
        var _CardSections = _PersistenceContext.GetEntities<CardSection>()
            .Where(s => s.Household.HouseholdID == _Household.HouseholdID)
            .OrderBy(s => s.Sequence)
            .ThenBy(s => s.CardSectionID)
            .Select(s => new
            {
                CardSection = s,
                s.Regions
            })
            .ToList()
            .Select(s => s.CardSection);

        await outputPort.PresentCardSectionsAsync(_CardSections, cancellationToken);
    }

    #endregion Methods

}