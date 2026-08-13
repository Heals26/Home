using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Feedback;

public partial class HomeEmptyState
{

    #region Properties

    [Parameter] public RenderFragment? ActionContent { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? IconContent { get; set; }
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public string Title { get; set; } = "Nothing here yet";

    #endregion Properties

}
