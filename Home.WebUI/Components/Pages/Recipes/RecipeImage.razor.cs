using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Recipes;

public partial class RecipeImage
{

    #region Fields

    private string m_FailedUrl = string.Empty;

    #endregion Fields

    #region Properties

    [Parameter] public string Alt { get; set; } = string.Empty;
    [Parameter] public string? Class { get; set; }
    [Parameter] public string IconClass { get; set; } = "h-8 w-8";
    [Parameter] public string? ImageUrl { get; set; }

    #endregion Properties

    #region Methods

    private bool ShowImage()
        => RecipeDisplayLogic.IsAWebImage(this.ImageUrl) && this.ImageUrl != this.m_FailedUrl;

    private void OnLoadFailed()
        => this.m_FailedUrl = this.ImageUrl ?? string.Empty;

    #endregion Methods

}
