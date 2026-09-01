using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.CardSections.UpdateCardSection;

public class UpdateCardSectionWebAppRequest
{

    #region Properties

    /// <summary>
    /// The heading as the household wants it.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    /// <summary>
    /// Where it reads down a card.
    /// </summary>
    public PropertyChangeTracker<int> Sequence { get; set; }

    #endregion Properties

}