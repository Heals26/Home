using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Navigation;

public partial class HomePageTitle
{

    #region Properties

    /// <summary>
    /// What this screen is. Left empty while a page is still loading its subject, so the tab
    /// reads "Home" rather than flashing a stray separator.
    /// </summary>
    [Parameter] public string? Title { get; set; }

    #endregion Properties

    #region Methods

    private string FullTitle()
        => string.IsNullOrWhiteSpace(this.Title) ? "Home" : $"{this.Title} · Home";

    #endregion Methods

}
