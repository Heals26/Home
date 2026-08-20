namespace Home.WebUI.DataAccess.RecipeImages.SetRecipeImage;

/// <summary>
/// Sent as multipart form data — the property name becomes the form field the API binds its
/// IFormFile parameter from, so the two must stay in step.
/// </summary>
public class SetRecipeImageWebAppRequest
{

    #region Properties

    /// <summary>
    /// The photo's bytes, streamed straight from the browser's file input.
    /// </summary>
    public Stream? Image { get; set; }

    #endregion Properties

}
