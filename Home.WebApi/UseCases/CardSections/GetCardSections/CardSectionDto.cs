namespace Home.WebApi.UseCases.CardSections.GetCardSections;

public class CardSectionDto
{

    #region Properties

    /// <summary>
    /// The ID of the section
    /// </summary>
    public long CardSectionID { get; set; }

    /// <summary>
    /// How many cards currently use it
    /// </summary>
    public int CardCount { get; set; }

    /// <summary>
    /// The heading as the household wrote it
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Where it reads down a card
    /// </summary>
    public int Sequence { get; set; }

    #endregion Properties

}