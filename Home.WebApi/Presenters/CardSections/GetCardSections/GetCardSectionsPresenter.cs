using AutoMapper;
using Home.Application.UseCases.CardSections.GetCardSections;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.CardSections.GetCardSections;

namespace Home.WebApi.Presenters.CardSections.GetCardSections;

public class GetCardSectionsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetCardSectionsOutputPort
{

    #region Methods

    Task IGetCardSectionsOutputPort.PresentCardSectionsAsync(IEnumerable<CardSection> cardSections, CancellationToken cancellationToken)
        => this.OkAsync(new GetCardSectionsApiResponse()
        {
            CardSections = [.. cardSections.Select(s => new CardSectionDto()
            {
                CardCount = s.Regions.Count,
                CardSectionID = s.CardSectionID,
                Name = s.Name,
                Sequence = s.Sequence
            })]
        }, cancellationToken);

    #endregion Methods

}