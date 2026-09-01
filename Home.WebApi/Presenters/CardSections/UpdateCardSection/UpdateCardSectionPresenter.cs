using AutoMapper;
using Home.Application.UseCases.CardSections.UpdateCardSection;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.CardSections.UpdateCardSection;

public class UpdateCardSectionPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateCardSectionOutputPort
{

    #region Methods

    Task IUpdateCardSectionOutputPort.PresentCardSectionNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateCardSectionOutputPort.PresentCardSectionNotFoundAsync(long cardSectionID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Card Section {cardSectionID} Not Found", cancellationToken);

    #endregion Methods

}