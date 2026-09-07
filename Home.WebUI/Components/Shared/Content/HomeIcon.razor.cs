using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Content;

public partial class HomeIcon
{

    #region Properties

    [Parameter] public string? Class { get; set; }
    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public string Size { get; set; } = "md";

    #endregion Properties

    #region Methods

    /// <summary>
    /// The name is only half-written here, so `home-icon-` is safelisted in tailwind.config.js. An
    /// icon whose rule got purged renders as a bare grey square rather than failing.
    /// </summary>
    private string GetClasses()
    {
        var _Size = this.Size switch
        {
            "xs" => "h-3 w-3",
            "sm" => "h-3.5 w-3.5",
            "lg" => "h-5 w-5",
            "xl" => "h-6 w-6",
            "2xl" => "h-7 w-7",
            "3xl" => "h-12 w-12",
            _    => "h-4 w-4"
        };

        return $"home-icon home-icon-{this.Name} {_Size} inline-block {this.Class}".TrimEnd();
    }

    #endregion Methods

}
