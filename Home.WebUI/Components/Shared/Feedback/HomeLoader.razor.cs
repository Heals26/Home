using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Feedback;

public partial class HomeLoader
{

    #region Properties

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public string Size { get; set; } = "md";

    #endregion Properties

    #region Methods

    private string GetSizeClass()
        => this.Size switch
        {
            "sm" => "h-4 w-4",
            "lg" => "h-10 w-10",
            _    => "h-6 w-6"
        };

    #endregion Methods

}
