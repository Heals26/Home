using Home.Domain.Entities;

namespace Home.Application.UseCases.CardSections.GetCardSections;

public interface IGetCardSectionsOutputPort
{

    #region Methods

    Task PresentCardSectionsAsync(IEnumerable<CardSection> cardSections, CancellationToken cancellationToken);

    #endregion Methods

}