namespace Home.Application.UseCases.RecipeImages.DeleteRecipeImage;

public interface IDeleteRecipeImageOutputPort
{

    #region Methods

    Task PresentRecipeImageDeletedNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
