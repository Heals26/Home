using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.CreateRecipe;

internal class CreateRecipeInteractor : IInteractor<CreateRecipeInputPort, ICreateRecipeOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateRecipeInputPort inputPort,
        ICreateRecipeOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Recipe = new Recipe()
        {
            Complexity = inputPort.Complexity,
            CookMinutes = inputPort.CookMinutes,
            Household = _AuthorisationService.GetHousehold(),
            ImageUrl = string.IsNullOrWhiteSpace(inputPort.ImageUrl) ? null : inputPort.ImageUrl.Trim(),
            Ingredients = [],
            MealSlots = [],
            Name = inputPort.Name,
            Notes = [],
            PrepMinutes = inputPort.PrepMinutes,
            Servings = inputPort.Servings,
            Steps = [],
            Url = inputPort.Url
        };

        _PersistenceContext.Add(_Recipe);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeCreatedAsync(_Recipe.RecipeID, cancellationToken);
    }

    #endregion Methods

}
