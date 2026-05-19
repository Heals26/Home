using AutoMapper;
using Home.Application.UseCases.Recipes.GetRecipe;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Recipes.GetRecipe;
using Home.WebApi.UseCases.Recipes.Models;

namespace Home.WebApi.Presenters.Recipes.GetRecipe;

public class GetRecipePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetRecipeOutputPort
{

    #region Methods

    Task IGetRecipeOutputPort.PresentRecipeAsync(Recipe recipe, CancellationToken cancellationToken)
        => this.OkAsync(new GetRecipeApiResponse()
        {
            RecipeID = recipe.RecipeID,
            Name = recipe.Name,
            Url = recipe.Url,
            Ingredients = [.. recipe.Ingredients.Select(ri => new RecipeIngredientDto()
            {
                IngredientID = ri.IngredientID,
                Name = ri.Ingredient.Name,
                Quantity = ri.Ingredient.Quantity,
                Volume = ri.Ingredient.Volume,
                Weight = ri.Ingredient.Weight
            })],
            Notes = [.. recipe.Notes.Select(rn => new RecipeNoteDto()
            {
                NoteID = rn.NoteID,
                Content = rn.Note.Content,
                CreatedOnUTC = rn.Note.CreatedOnUTC
            })]
        }, cancellationToken);

    Task IGetRecipeOutputPort.PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe {recipeID} Not Found", cancellationToken);

    #endregion Methods

}
