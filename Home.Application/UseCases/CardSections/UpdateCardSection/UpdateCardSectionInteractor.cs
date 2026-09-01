using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.CardSections.UpdateCardSection;

internal class UpdateCardSectionInteractor : IInteractor<UpdateCardSectionInputPort, IUpdateCardSectionOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateCardSectionInputPort inputPort,
        IUpdateCardSectionOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _CardSection = _PersistenceContext.GetEntities<CardSection>()
            .Where(s => s.CardSectionID == inputPort.CardSectionID
                && s.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_CardSection == null)
        {
            await outputPort.PresentCardSectionNotFoundAsync(inputPort.CardSectionID, cancellationToken);
        }
        else
        {
            if (inputPort.Name.HasBeenSet)
                _CardSection.Name = inputPort.Name.Value;

            if (inputPort.Sequence.HasBeenSet)
                _CardSection.Sequence = inputPort.Sequence.Value;

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentCardSectionNoContentAsync(cancellationToken);
        }
    }

    #endregion Methods

}