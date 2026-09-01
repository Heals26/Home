namespace Home.WebApi.UseCases.CardSections.GetCardSections;

public class GetCardSectionsApiResponse
{

    #region Properties

    /// <summary>
    /// The sections this household puts on its activity cards, in the order they read
    /// </summary>
    public ICollection<CardSectionDto> CardSections { get; set; }

    #endregion Properties

}