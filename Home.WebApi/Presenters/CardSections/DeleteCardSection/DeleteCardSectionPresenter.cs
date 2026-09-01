using AutoMapper;
using Home.Application.UseCases.CardSections.DeleteCardSection;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.CardSections.DeleteCardSection;

public class DeleteCardSectionPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteCardSectionOutputPort
{

    #region Methods

    Task IDeleteCardSectionOutputPort.PresentCardSectionDeletedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    /// <summary>
    /// A 409 rather than a 404 or a silent no-op: the section exists, the household simply cannot
    /// have it while cards are still written under it.
    /// </summary>
    Task IDeleteCardSectionOutputPort.PresentCardSectionInUseAsync(long cardSectionID, int cardCount, CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task IDeleteCardSectionOutputPort.PresentCardSectionNotFoundAsync(long cardSectionID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Card Section {cardSectionID} Not Found", cancellationToken);

    #endregion Methods

}