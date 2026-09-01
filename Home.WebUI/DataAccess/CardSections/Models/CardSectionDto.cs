namespace Home.WebUI.DataAccess.CardSections.Models;

public class CardSectionDto
{

    #region Properties

    /// <summary>
    /// How many cards currently use this section.
    /// </summary>
    public int CardCount { get; set; }

    /// <summary>
    /// The ID of the section.
    /// </summary>
    public long CardSectionID { get; set; }

    /// <summary>
    /// The heading as the household wrote it.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Where it reads down a card.
    /// </summary>
    public int Sequence { get; set; }

    #endregion Properties

}