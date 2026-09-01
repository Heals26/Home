using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.CardSections.CreateCardSection;

internal class CreateCardSectionInteractor : IInteractor<CreateCardSectionInputPort, ICreateCardSectionOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateCardSectionInputPort inputPort,
        ICreateCardSectionOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        // A new section goes on the end, computed here so two people adding at once cannot land on
        // the same position.
        var _NextSequence = _PersistenceContext.GetEntities<CardSection>()
            .Where(s => s.Household.HouseholdID == _Household.HouseholdID)
            .Select(s => (int?)s.Sequence)
            .Max() ?? -1;

        var _CardSection = new CardSection()
        {
            Household = _Household,
            Name = inputPort.Name,
            Regions = [],
            Sequence = _NextSequence + 1
        };

        _PersistenceContext.Add(_CardSection);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentCardSectionCreatedAsync(_CardSection.CardSectionID, cancellationToken);
    }

    #endregion Methods

}