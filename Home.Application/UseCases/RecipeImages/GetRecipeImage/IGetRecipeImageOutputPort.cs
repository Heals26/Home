namespace Home.Application.UseCases.RecipeImages.GetRecipeImage;

public interface IGetRecipeImageOutputPort
{

    #region Methods

    Task PresentRecipeImageAsync(byte[] content, string contentType, CancellationToken cancellationToken);
    Task PresentRecipeImageNotFoundAsync(long recipeID, CancellationToken cancellationToken);

    #endregion Methods

}
