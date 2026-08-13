using Microsoft.AspNetCore.Components.Routing;

namespace Home.WebUI.Components.Shared.Navigation;

public partial class HomeNavRail
{

    #region Records

    private sealed record NavItem(string Href, string Label, string Icon, string ActiveText, NavLinkMatch Match);

    #endregion Records

    #region Fields

    private static readonly NavItem[] m_Items =
    [
        new("/", "Home", "home-icon-home", "text-ink-50", NavLinkMatch.All),
        new("/recipes", "Recipes", "home-icon-book", "text-recipes", NavLinkMatch.Prefix),
        new("/shopping-lists", "Shopping", "home-icon-shopping-list", "text-shopping", NavLinkMatch.Prefix),
        new("/activities", "Week", "home-icon-board", "text-week", NavLinkMatch.Prefix),
        new("/lights", "Lights", "home-icon-lightbulb", "text-lights", NavLinkMatch.Prefix),
    ];

    #endregion Fields

}
