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

    /// <summary>
    /// Ticks of the household's own photo, null when there isn't one. The photo beats
    /// <see cref="ImageUrl"/> — a picture somebody took of their actual dinner is the better one.
    /// </summary>
    [Parameter] public long? ImageVersion { get; set; }

    [Parameter] public string? ImageUrl { get; set; }
    [Parameter] public long RecipeID { get; set; }

    #endregion Properties

    #region Methods

    private string? Source()
    {
        // The household's own photo streams through the web app itself, authenticated by the
        // sign-in cookie the img tag already sends.
        if (this.ImageVersion != null)
            return $"/recipe-images/{this.RecipeID}?v={this.ImageVersion}";

        return RecipeDisplayLogic.IsAWebImage(this.ImageUrl) ? this.ImageUrl : null;
    }

    private bool ShowImage()
        => this.Source() is { } _Source && _Source != this.m_FailedUrl;

    private void OnLoadFailed()
        => this.m_FailedUrl = this.Source() ?? string.Empty;

    #endregion Methods

}
