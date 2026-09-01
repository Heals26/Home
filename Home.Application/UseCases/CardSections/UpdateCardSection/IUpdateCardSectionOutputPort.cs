namespace Home.Application.UseCases.CardSections.UpdateCardSection;

public interface IUpdateCardSectionOutputPort
{

    #region Methods

    Task PresentCardSectionNoContentAsync(CancellationToken cancellationToken);
    Task PresentCardSectionNotFoundAsync(long cardSectionID, CancellationToken cancellationToken);

    #endregion Methods

}