namespace Home.Domain.Entities;

/// <summary>
/// A photo somebody took of the dish, stored as bytes on the recipe's own row family. Kept out of
/// <see cref="Recipe"/> itself so listing the book never drags image bytes through a query.
/// </summary>
public class RecipeImage
{

    #region Properties

    public long RecipeImageID { get; set; }

    /// <summary>
    /// The image bytes, exactly as uploaded.
    /// </summary>
    public byte[] Content { get; set; } = [];

    /// <summary>
    /// The MIME type the browser declared, served back with the bytes.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    public Recipe Recipe { get; set; } = null!;

    #endregion Properties

}
