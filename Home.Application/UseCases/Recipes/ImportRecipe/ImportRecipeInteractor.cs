using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.RecipeImports;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.ImportRecipe;

internal class ImportRecipeInteractor : IInteractor<ImportRecipeInputPort, IImportRecipeOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        ImportRecipeInputPort inputPort,
        IImportRecipeOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _RecipeImportService = serviceFactory.GetService<IRecipeImportService>();

        var _Imported = await _RecipeImportService.FetchRecipeAsync(inputPort.Url, cancellationToken);

        if (_Imported == null)
        {
            await outputPort.PresentRecipeImportFailedAsync(inputPort.Url, cancellationToken);
            return;
        }

        var _Recipe = new Recipe()
        {
            CookMinutes = _Imported.CookMinutes,
            Household = _AuthorisationService.GetHousehold(),
            ImageUrl = _Imported.ImageUrl,
            Ingredients = [],
            MealSlots = [],
            Name = _Imported.Name,
            Notes = [],
            PrepMinutes = _Imported.PrepMinutes,
            Servings = _Imported.Servings,
            Steps = [],
            Url = inputPort.Url
        };

        foreach (var _Ingredient in _Imported.Ingredients)
            _Recipe.Ingredients.Add(new RecipeIngredient()
            {
                Ingredient = new Ingredient() { Name = _Ingredient },
                Recipe = _Recipe
            });

        var _Sequence = 0;

        foreach (var _Step in _Imported.Steps)
            _Recipe.Steps.Add(new RecipeStep()
            {
                Content = _Step.Content,
                Sequence = ++_Sequence,
                Title = _Step.Title
            });

        _PersistenceContext.Add(_Recipe);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeImportedAsync(_Recipe.RecipeID, cancellationToken);
    }

    #endregion Methods

}
