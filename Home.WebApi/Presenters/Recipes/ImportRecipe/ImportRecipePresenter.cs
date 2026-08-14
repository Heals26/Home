using AutoMapper;
using Home.Application.UseCases.Recipes.ImportRecipe;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Recipes.ImportRecipe;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Home.WebApi.Presenters.Recipes.ImportRecipe;

public class ImportRecipePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IImportRecipeOutputPort
{

    #region Methods

    Task IImportRecipeOutputPort.PresentRecipeImportedAsync(long recipeID, CancellationToken cancellationToken)
        => this.CreatedAsync(recipeID, new ImportRecipeApiResponse() { RecipeID = recipeID }, cancellationToken);

    Task IImportRecipeOutputPort.PresentRecipeImportFailedAsync(string url, CancellationToken cancellationToken)
        => this.UnprocessableContent(new ValidationProblemDetails()
        {
            Detail = "See Errors property for more details.",
            Status = (int)HttpStatusCode.UnprocessableContent,
            Title = "The page could not be read as a recipe.",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            Errors = new Dictionary<string, string[]>()
            {
                ["Url"] = ["Couldn't read a recipe from that page. It may be unreachable, or not carry recipe data — you can still add it by hand."]
            }
        }, cancellationToken);

    #endregion Methods

}
