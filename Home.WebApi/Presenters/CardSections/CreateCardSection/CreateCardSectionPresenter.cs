using AutoMapper;
using Home.Application.UseCases.CardSections.CreateCardSection;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.CardSections.CreateCardSection;

namespace Home.WebApi.Presenters.CardSections.CreateCardSection;

public class CreateCardSectionPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateCardSectionOutputPort
{

    #region Methods

    Task ICreateCardSectionOutputPort.PresentCardSectionCreatedAsync(long cardSectionID, CancellationToken cancellationToken)
        => this.CreatedAsync(cardSectionID, new CreateCardSectionApiResponse() { CardSectionID = cardSectionID }, cancellationToken);

    #endregion Methods

}