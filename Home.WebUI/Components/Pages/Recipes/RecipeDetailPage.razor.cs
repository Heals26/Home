using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.RecipeIngredients.AddRecipeIngredient;
using Home.WebUI.DataAccess.RecipeIngredients.UpdateRecipeIngredient;
using Home.WebUI.DataAccess.RecipeNotes.AddRecipeNote;
using Home.WebUI.DataAccess.Recipes.GetRecipe;
using Home.WebUI.DataAccess.Recipes.Models;
using Home.WebUI.DataAccess.Recipes.UpdateRecipe;
using Home.WebUI.DataAccess.RecipeSteps.AddRecipeStep;
using Home.WebUI.DataAccess.RecipeSteps.UpdateRecipeStep;
using Home.WebUI.DataAccess.ShoppingLists.CreateShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.GetShoppingLists;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.ChangeTrackers;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Recipes;

public partial class RecipeDetailPage : IDisposable
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;
    private GetRecipeWebAppResponse? m_Recipe;

    private bool m_Saving;

    // Edit recipe
    private bool m_ShowEditRecipe;
    private string m_EditName = string.Empty;
    private string m_EditUrl = string.Empty;

    // Ingredient
    private bool m_ShowIngredient;
    private long? m_EditingIngredientID;
    private string m_IngName = string.Empty;
    private string m_IngQuantity = string.Empty;
    private string m_IngVolume = string.Empty;
    private string m_IngWeight = string.Empty;

    // Step
    private bool m_ShowStep;
    private long? m_EditingStepID;
    private string m_StepTitle = string.Empty;
    private string m_StepContent = string.Empty;

    // Note
    private bool m_ShowNote;
    private string m_NoteContent = string.Empty;

    // Add to list
    private bool m_ShowAddToList;
    private GetShoppingListsWebAppResponse? m_ShoppingLists;
    private string m_NewListName = string.Empty;
    private bool m_AddingToList;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }
    [Parameter] public long RecipeID { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
        => this.m_ChangeSubscription = await this.ChangeBroadcaster.SubscribeAsync(
            this.OnHouseholdChangedAsync, this.m_CancellationTokenHandler.Token);

    protected override async Task OnParametersSetAsync()
        => await this.LoadRecipeAsync();

    public void Dispose()
    {
        this.m_ChangeSubscription?.Dispose();
        this.m_CancellationTokenHandler.Dispose();
    }

    #endregion Lifecycle Methods

    #region Methods

    // Loading

    private async Task LoadRecipeAsync()
    {
        this.m_Recipe = await this.ApiAccess.SendRequestAsync<object, GetRecipeWebAppResponse>(
            null!, ApiProvider.GetRecipe(this.RecipeID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);
    }

    private async Task OnHouseholdChangedAsync(ChangeArea area)
    {
        if (area != ChangeArea.Recipes)
            return;

        await this.InvokeAsync(async () =>
        {
            await this.LoadRecipeAsync();
            this.StateHasChanged();
        });
    }

    private async Task ReloadAndPublishAsync()
    {
        await this.LoadRecipeAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Recipes, this.m_CancellationTokenHandler.Token);
    }

    // Recipe

    private void OpenEditRecipeModal()
    {
        this.m_EditName = this.m_Recipe!.Name;
        this.m_EditUrl = this.m_Recipe!.Url ?? string.Empty;
        this.m_ShowEditRecipe = true;
    }

    private async Task SaveRecipeAsync()
    {
        if (this.m_Saving) return;
        this.m_Saving = true;

        var _Request = new UpdateRecipeWebAppRequest()
        {
            Name = new PropertyChangeTracker<string>(this.m_EditName),
            Url = new PropertyChangeTracker<string>(this.m_EditUrl)
        };

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateRecipeWebAppRequest, bool>(
            _Request, ApiProvider.UpdateRecipe(this.RecipeID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Saving = false;

        if (_Result != true) return;

        this.m_ShowEditRecipe = false;
        await this.ReloadAndPublishAsync();
    }

    // Ingredients

    private void OpenAddIngredientModal()
    {
        this.m_EditingIngredientID = null;
        this.m_IngName = string.Empty;
        this.m_IngQuantity = string.Empty;
        this.m_IngVolume = string.Empty;
        this.m_IngWeight = string.Empty;
        this.m_ShowIngredient = true;
    }

    private void OpenEditIngredientModal(RecipeIngredientDto ingredient)
    {
        this.m_EditingIngredientID = ingredient.IngredientID;
        this.m_IngName = ingredient.Name;
        this.m_IngQuantity = ingredient.Quantity?.ToString() ?? string.Empty;
        this.m_IngVolume = ingredient.Volume?.ToString() ?? string.Empty;
        this.m_IngWeight = ingredient.Weight?.ToString() ?? string.Empty;
        this.m_ShowIngredient = true;
    }

    private async Task SaveIngredientAsync()
    {
        if (this.m_Saving) return;
        this.m_Saving = true;

        bool _Result;

        if (this.m_EditingIngredientID.HasValue)
        {
            var _Request = new UpdateRecipeIngredientWebAppRequest()
            {
                Name = new PropertyChangeTracker<string>(this.m_IngName),
                Quantity = new PropertyChangeTracker<decimal?>(ParseDecimal(this.m_IngQuantity)),
                Volume = new PropertyChangeTracker<decimal?>(ParseDecimal(this.m_IngVolume)),
                Weight = new PropertyChangeTracker<decimal?>(ParseDecimal(this.m_IngWeight))
            };

            _Result = await this.ApiAccess.SendRequestAsync<UpdateRecipeIngredientWebAppRequest, bool>(
                _Request, ApiProvider.UpdateRecipeIngredient(this.m_EditingIngredientID.Value),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token);
        }
        else
        {
            var _Request = new AddRecipeIngredientWebAppRequest()
            {
                Name = this.m_IngName,
                Quantity = ParseDecimal(this.m_IngQuantity),
                Volume = ParseDecimal(this.m_IngVolume),
                Weight = ParseDecimal(this.m_IngWeight),
                RecipeID = this.RecipeID
            };

            var _Response = await this.ApiAccess.SendRequestAsync<AddRecipeIngredientWebAppRequest, AddRecipeIngredientWebAppResponse>(
                _Request, ApiProvider.AddRecipeIngredient(),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token);

            _Result = _Response != null;
        }

        this.m_Saving = false;

        if (!_Result) return;

        this.m_ShowIngredient = false;
        await this.ReloadAndPublishAsync();
    }

    private async Task RemoveIngredientAsync(long ingredientID)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.RemoveRecipeIngredient(this.RecipeID, ingredientID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result == true)
            await this.ReloadAndPublishAsync();
    }

    // Steps

    private void OpenAddStepModal()
    {
        this.m_EditingStepID = null;
        this.m_StepTitle = string.Empty;
        this.m_StepContent = string.Empty;
        this.m_ShowStep = true;
    }

    private void OpenEditStepModal(RecipeStepDto step)
    {
        this.m_EditingStepID = step.RecipeStepID;
        this.m_StepTitle = step.Title;
        this.m_StepContent = step.Content;
        this.m_ShowStep = true;
    }

    private async Task SaveStepAsync()
    {
        if (this.m_Saving) return;
        this.m_Saving = true;

        bool _Result;

        if (this.m_EditingStepID.HasValue)
        {
            var _Existing = this.m_Recipe!.Steps.First(s => s.RecipeStepID == this.m_EditingStepID.Value);
            var _Request = new UpdateRecipeStepWebAppRequest()
            {
                Content = new PropertyChangeTracker<string>(this.m_StepContent),
                Title = new PropertyChangeTracker<string>(this.m_StepTitle),
                Sequence = new PropertyChangeTracker<int>(_Existing.Sequence)
            };

            _Result = await this.ApiAccess.SendRequestAsync<UpdateRecipeStepWebAppRequest, bool>(
                _Request, ApiProvider.UpdateRecipeStep(this.m_EditingStepID.Value),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token);
        }
        else
        {
            var _NextSequence = this.m_Recipe!.Steps.Any()
                ? this.m_Recipe.Steps.Max(s => s.Sequence) + 1
                : 1;

            var _Request = new AddRecipeStepWebAppRequest()
            {
                Content = this.m_StepContent,
                Title = this.m_StepTitle,
                Sequence = _NextSequence,
                RecipeID = this.RecipeID
            };

            var _Response = await this.ApiAccess.SendRequestAsync<AddRecipeStepWebAppRequest, AddRecipeStepWebAppResponse>(
                _Request, ApiProvider.AddRecipeStep(),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token);

            _Result = _Response != null;
        }

        this.m_Saving = false;

        if (!_Result) return;

        this.m_ShowStep = false;
        await this.ReloadAndPublishAsync();
    }

    private async Task RemoveStepAsync(long recipeStepID)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.RemoveRecipeStep(recipeStepID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result == true)
            await this.ReloadAndPublishAsync();
    }

    // Notes

    private void OpenAddNoteModal()
    {
        this.m_NoteContent = string.Empty;
        this.m_ShowNote = true;
    }

    private async Task SaveNoteAsync()
    {
        if (this.m_Saving) return;
        this.m_Saving = true;

        var _Request = new AddRecipeNoteWebAppRequest()
        {
            Content = this.m_NoteContent,
            RecipeID = this.RecipeID
        };

        var _Response = await this.ApiAccess.SendRequestAsync<AddRecipeNoteWebAppRequest, AddRecipeNoteWebAppResponse>(
            _Request, ApiProvider.AddRecipeNote(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Saving = false;

        if (_Response == null) return;

        this.m_ShowNote = false;
        await this.ReloadAndPublishAsync();
    }

    private async Task RemoveNoteAsync(long noteID)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.RemoveRecipeNote(this.RecipeID, noteID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result == true)
            await this.ReloadAndPublishAsync();
    }

    // Add to shopping list

    private async Task OpenAddToListModal()
    {
        this.m_ShoppingLists = null;
        this.m_NewListName = string.Empty;
        this.m_ShowAddToList = true;

        this.m_ShoppingLists = await this.ApiAccess.SendRequestAsync<object, GetShoppingListsWebAppResponse>(
            null!, ApiProvider.GetShoppingLists(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);
    }

    private async Task AddToExistingListAsync(long shoppingListID)
    {
        if (this.m_AddingToList) return;
        this.m_AddingToList = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.AddRecipeToShoppingList(shoppingListID, this.RecipeID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_AddingToList = false;

        if (_Result != true)
            return;

        this.m_ShowAddToList = false;
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.m_CancellationTokenHandler.Token);
    }

    private async Task CreateListAndAddAsync()
    {
        if (this.m_AddingToList) return;
        if (string.IsNullOrWhiteSpace(this.m_NewListName)) return;
        this.m_AddingToList = true;

        var _Created = await this.ApiAccess.SendRequestAsync<CreateShoppingListWebAppRequest, CreateShoppingListWebAppResponse>(
            new CreateShoppingListWebAppRequest() { Name = this.m_NewListName },
            ApiProvider.CreateShoppingList(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Created == null)
        {
            this.m_AddingToList = false;
            return;
        }

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.AddRecipeToShoppingList(_Created.ShoppingListID, this.RecipeID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_AddingToList = false;

        if (_Result != true)
            return;

        this.m_ShowAddToList = false;
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.m_CancellationTokenHandler.Token);
    }

    // Helpers

    private static decimal? ParseDecimal(string value)
        => decimal.TryParse(value, out var _Parsed) ? _Parsed : null;

    private bool HasMeasurements(RecipeIngredientDto ingredient)
        => ingredient.Quantity.HasValue || ingredient.Volume.HasValue || ingredient.Weight.HasValue;

    private string DescribeMeasurements(RecipeIngredientDto ingredient)
    {
        var _Parts = new List<string>();

        if (ingredient.Quantity.HasValue)
            _Parts.Add($"Qty {ingredient.Quantity.Value}");

        if (ingredient.Volume.HasValue)
            _Parts.Add($"Vol {ingredient.Volume.Value}");

        if (ingredient.Weight.HasValue)
            _Parts.Add($"Wt {ingredient.Weight.Value}");

        return string.Join(" · ", _Parts);
    }

    #endregion Methods

}
