namespace Home.Application.UseCases.CardSections.DeleteCardSection;

public interface IDeleteCardSectionOutputPort
{

    #region Methods

    Task PresentCardSectionDeletedAsync(CancellationToken cancellationToken);
    Task PresentCardSectionInUseAsync(long cardSectionID, int cardCount, CancellationToken cancellationToken);
    Task PresentCardSectionNotFoundAsync(long cardSectionID, CancellationToken cancellationToken);

    #endregion Methods

}