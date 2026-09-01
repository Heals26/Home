using Home.WebUI.DataAccess.CardSections.Models;

namespace Home.WebUI.DataAccess.CardSections.GetCardSections;

public class GetCardSectionsWebAppResponse
{

    #region Properties

    /// <summary>
    /// The sections this household puts on its activity cards, in the order they read.
    /// </summary>
    public List<CardSectionDto> CardSections { get; set; } = [];

    #endregion Properties

}