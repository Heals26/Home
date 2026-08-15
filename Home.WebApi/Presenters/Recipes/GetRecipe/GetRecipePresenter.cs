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
            Complexity = recipe.Complexity,
            CookMinutes = recipe.CookMinutes,
            ImageUrl = recipe.ImageUrl,
            Name = recipe.Name,
            PrepMinutes = recipe.PrepMinutes,
            Servings = recipe.Servings,
            Url = recipe.Url,
            Ingredients = [.. recipe.Ingredients.Select(ri => new RecipeIngredientDto()
            {
                IngredientID = ri.IngredientID,
                Amount = ri.Ingredient.Amount,
                Name = ri.Ingredient.Name,
                Quantity = ri.Ingredient.Quantity,
                Unit = ri.Ingredient.Unit,
                Volume = ri.Ingredient.Volume,
                Weight = ri.Ingredient.Weight
            })],
            MealSlots = [.. recipe.MealSlots.Select(rms => rms.MealSlot).OrderBy(ms => ms.Sequence).Select(ms => new RecipeMealSlotDto()
            {
                MealSlotID = ms.MealSlotID,
                Name = ms.Name,
                Sequence = ms.Sequence
            })],
            Notes = [.. recipe.Notes.Select(rn => new RecipeNoteDto()
            {
                NoteID = rn.NoteID,
                Content = rn.Note.Content,
                CreatedOnUTC = rn.Note.CreatedOnUTC
            })],
            Steps = [.. recipe.Steps.OrderBy(s => s.Sequence).Select(s => new RecipeStepDto()
            {
                RecipeStepID = s.RecipeStepID,
                Sequence = s.Sequence,
                Title = s.Title,
                Content = s.Content
            })]
        }, cancellationToken);

    Task IGetRecipeOutputPort.PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe {recipeID} Not Found", cancellationToken);

    #endregion Methods

}
