using AutoMapper;
using Home.Application.UseCases.RecipeIngredients.GetRecipeIngredient;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Ingredients.Models;
using Home.WebApi.UseCases.RecipeIngredients.GetRecipeIngredient;

namespace Home.WebApi.Presenters.RecipeIngredients.GetRecipeIngredient;

public class GetRecipeIngredientPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetRecipeIngredientOutputPort
{

    #region Methods

    Task IGetRecipeIngredientOutputPort.PresentRecipeIngredientAsync(Ingredient ingredient, CancellationToken cancellationToken)
        => this.OkAsync(new GetRecipeIngredientApiResponse()
        {
            IngredientID = ingredient.IngredientID,
            Name = ingredient.Name,
            Quantity = ingredient.Quantity,
            Volume = ingredient.Volume,
            Weight = ingredient.Weight,
            Notes = [.. ingredient.Notes.Select(n => new IngredientNoteDto()
            {
                NoteID = n.NoteID,
                Content = n.Note.Content,
                CreatedOnUTC = n.Note.CreatedOnUTC
            })]
        }, cancellationToken);

    Task IGetRecipeIngredientOutputPort.PresentRecipeIngredientNotFoundAsync(long ingredientID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Ingredient {ingredientID} Not Found", cancellationToken);

    #endregion Methods

}
