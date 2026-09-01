using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.CardSections.DeleteCardSection;

internal class DeleteCardSectionInteractor : IInteractor<DeleteCardSectionInputPort, IDeleteCardSectionOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteCardSectionInputPort inputPort,
        IDeleteCardSectionOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _CardSection = _PersistenceContext.GetEntities<CardSection>()
            .Where(s => s.CardSectionID == inputPort.CardSectionID
                && s.Household.HouseholdID == _Household.HouseholdID)
            .Select(s => new
            {
                CardSection = s,
                s.Regions
            })
            .SingleOrDefault()
            ?.CardSection;

        if (_CardSection == null)
        {
            await outputPort.PresentCardSectionNotFoundAsync(inputPort.CardSectionID, cancellationToken);
        }
        else if (_CardSection.Regions.Count > 0)
        {
            // Refused rather than cascaded: a section still holding writing on real cards would
            // take that writing with it, and there is no undo. The caller is told how many cards
            // are involved so it can say so.
            await outputPort.PresentCardSectionInUseAsync(inputPort.CardSectionID, _CardSection.Regions.Count, cancellationToken);
        }
        else
        {
            _PersistenceContext.Remove(_CardSection);
            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentCardSectionDeletedAsync(cancellationToken);
        }
    }

    #endregion Methods

}