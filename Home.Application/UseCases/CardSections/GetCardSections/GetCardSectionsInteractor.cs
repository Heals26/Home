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

        var _CardSections = _PersistenceContext.GetEntities<CardSection>()
            .Where(s => s.Household.HouseholdID == _Household.HouseholdID)
            .OrderBy(s => s.Sequence)
            .ThenBy(s => s.CardSectionID)
            .ToList();

        await outputPort.PresentCardSectionsAsync(_CardSections, cancellationToken);
    }

    #endregion Methods

}