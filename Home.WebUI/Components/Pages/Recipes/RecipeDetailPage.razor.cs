using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.Components.Pages.ShoppingList;
using Home.WebUI.Components.Shared.Inputs;
using Home.WebUI.DataAccess.MealSlots.GetMealSlots;
using Home.WebUI.DataAccess.MealSlots.Models;
using Home.WebUI.DataAccess.Notes.UpdateNote;
using Home.WebUI.DataAccess.RecipeIngredients.AddRecipeIngredient;
using Home.WebUI.DataAccess.RecipeIngredients.GetIngredientSuggestions;
using Home.WebUI.DataAccess.RecipeIngredients.SetRecipeIngredientSequence;
using Home.WebUI.DataAccess.RecipeIngredients.UpdateRecipeIngredient;
using Home.WebUI.DataAccess.RecipeImages.SetRecipeImage;
using Home.WebUI.DataAccess.RecipeNotes.AddRecipeNote;
using Home.WebUI.DataAccess.Recipes.GetRecipe;
using Home.WebUI.DataAccess.Recipes.Models;
using Home.WebUI.DataAccess.Recipes.SetRecipeMealSlots;
using Home.WebUI.DataAccess.Recipes.UpdateRecipe;
using Home.WebUI.DataAccess.RecipeSteps.AddRecipeStep;
using Home.WebUI.DataAccess.RecipeSteps.UpdateRecipeStep;
using Home.WebUI.DataAccess.ShoppingLists.AddRecipeToShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.CreateShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.GetShoppingLists;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.ChangeTrackers;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;

namespace Home.WebUI.Components.Pages.Recipes;

public partial class RecipeDetailPage : IDisposable
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;
    private GetRecipeWebAppResponse? m_Recipe;
    private List<MealSlotDto> m_MealSlots = [];

    private bool m_Saving;

    // Edit recipe
    private bool m_ShowEditRecipe;
    private string m_EditName = string.Empty;
    private string m_EditUrl = string.Empty;
    private string m_EditImageUrl = string.Empty;
    private string m_EditPrepMinutes = string.Empty;
    private string m_EditCookMinutes = string.Empty;
    private string m_EditServings = string.Empty;
    private string m_EditComplexity = string.Empty;
    private HashSet<long> m_EditMealSlotIDs = [];

    /// <summary>
    /// Mirrors the API's photo cap, so an oversized pick fails in the modal, not on the wire.
    /// </summary>
    private const long MaxPhotoBytes = 5 * 1024 * 1024;

    private byte[]? m_PendingPhoto;
    private string m_PendingPhotoName = string.Empty;
    private string m_PhotoError = string.Empty;
    private bool m_RemovePhoto;

    // Focus can only land once the modal has actually rendered, so opening it raises a flag that
    // OnAfterRenderAsync acts on.
    private HomeTextInput? m_IngredientNameInput;
    private bool m_FocusIngredientName;

    // Ingredient
    private bool m_ShowIngredient;
    private long? m_EditingIngredientID;
    private string m_IngName = string.Empty;
    private string m_IngAmount = string.Empty;
    private string m_IngUnit = string.Empty;

    /// <summary>
    /// How many of the household's ingredients the name box offers at once. Enough to recognise
    /// one without the list swallowing the amount field underneath it.
    /// </summary>
    private const int IngredientSuggestionsShown = 6;

    private List<GetIngredientSuggestionDto> m_IngredientSuggestions = [];
    private bool m_ShowIngredientSuggestions;

    // Step
    private bool m_ShowStep;
    private long? m_EditingStepID;
    private string m_StepTitle = string.Empty;
    private string m_StepContent = string.Empty;

    // Note
    private bool m_ShowNote;
    private long? m_EditingNoteID;
    private string m_NoteContent = string.Empty;

    // Add to list
    private bool m_ShowAddToList;
    private GetShoppingListsWebAppResponse? m_ShoppingLists;
    private HashSet<long> m_SelectedIngredientIDs = [];
    private string m_NewListName = string.Empty;
    private bool m_AddingToList;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }
    [Parameter] public long RecipeID { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await this.LoadMealSlotsAsync();
        await this.LoadIngredientSuggestionsAsync();

        this.m_ChangeSubscription = await this.ChangeBroadcaster.SubscribeAsync(
            this.OnHouseholdChangedAsync, this.m_CancellationTokenHandler.Token);
    }

    protected override async Task OnParametersSetAsync()
        => await this.LoadRecipeAsync();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!this.m_FocusIngredientName || this.m_IngredientNameInput == null)
            return;

        this.m_FocusIngredientName = false;
        await this.m_IngredientNameInput.FocusAsync();
    }

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

    private async Task LoadMealSlotsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetMealSlotsWebAppResponse>(
            null!, ApiProvider.GetMealSlots(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_MealSlots = [.. _Result.MealSlots.OrderBy(m => m.Sequence).ThenBy(m => m.Name)];
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
        this.m_EditUrl = this.m_Recipe.Url ?? string.Empty;
        this.m_EditImageUrl = this.m_Recipe.ImageUrl ?? string.Empty;
        this.m_EditPrepMinutes = this.m_Recipe.PrepMinutes?.ToString() ?? string.Empty;
        this.m_EditCookMinutes = this.m_Recipe.CookMinutes?.ToString() ?? string.Empty;
        this.m_EditServings = this.m_Recipe.Servings?.ToString() ?? string.Empty;
        this.m_EditComplexity = this.m_Recipe.Complexity?.ToString() ?? string.Empty;
        this.m_EditMealSlotIDs = [.. this.m_Recipe.MealSlots.Select(m => m.MealSlotID)];
        this.m_PendingPhoto = null;
        this.m_PendingPhotoName = string.Empty;
        this.m_PhotoError = string.Empty;
        this.m_RemovePhoto = false;
        this.m_ShowEditRecipe = true;
    }

    private void ToggleMealSlot(long mealSlotID)
    {
        if (!this.m_EditMealSlotIDs.Add(mealSlotID))
            _ = this.m_EditMealSlotIDs.Remove(mealSlotID);
    }

    /// <summary>
    /// Two calls, because the meals a recipe suits are a set the API replaces wholesale rather
    /// than a property it patches.
    /// </summary>
    private async Task SaveRecipeAsync()
    {
        if (this.m_Saving) return;
        this.m_Saving = true;

        var _Request = new UpdateRecipeWebAppRequest()
        {
            Complexity = new PropertyChangeTracker<long?>(ParseLong(this.m_EditComplexity)),
            CookMinutes = new PropertyChangeTracker<int?>(ParseInt(this.m_EditCookMinutes)),
            ImageUrl = new PropertyChangeTracker<string>(this.m_EditImageUrl),
            Name = new PropertyChangeTracker<string>(this.m_EditName),
            PrepMinutes = new PropertyChangeTracker<int?>(ParseInt(this.m_EditPrepMinutes)),
            Servings = new PropertyChangeTracker<int?>(ParseInt(this.m_EditServings)),
            Url = new PropertyChangeTracker<string>(this.m_EditUrl)
        };

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateRecipeWebAppRequest, bool>(
            _Request, ApiProvider.UpdateRecipe(this.RecipeID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result == true)
        {
            _Result = await this.ApiAccess.SendRequestAsync<SetRecipeMealSlotsWebAppRequest, bool>(
                new SetRecipeMealSlotsWebAppRequest() { MealSlotIDs = [.. this.m_EditMealSlotIDs] },
                ApiProvider.SetRecipeMealSlots(this.RecipeID),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token);
        }

        if (_Result == true)
            _Result = await this.SavePhotoAsync();

        this.m_Saving = false;

        if (_Result != true) return;

        this.m_ShowEditRecipe = false;
        await this.ReloadAndPublishAsync();
    }

    /// <summary>
    /// Reads the picked file straight away rather than at save time — the browser revokes the
    /// file handle if the user picks another one, and a failed read should be visible while the
    /// modal is still open.
    /// </summary>
    private async Task OnPhotoPickedAsync(InputFileChangeEventArgs e)
    {
        this.m_PhotoError = string.Empty;
        this.m_PendingPhoto = null;
        this.m_PendingPhotoName = string.Empty;
        this.m_RemovePhoto = false;

        if (e.File.Size > MaxPhotoBytes)
        {
            this.m_PhotoError = "That photo is over 5 MB. Most phones can export a smaller size.";
            return;
        }

        try
        {
            using var _Content = new MemoryStream();
            await e.File.OpenReadStream(MaxPhotoBytes).CopyToAsync(_Content, this.m_CancellationTokenHandler.Token);

            this.m_PendingPhoto = _Content.ToArray();
            this.m_PendingPhotoName = e.File.Name;
        }
        catch (IOException)
        {
            this.m_PhotoError = "That photo couldn't be read. Try picking it again.";
        }
    }

    private async Task<bool> SavePhotoAsync()
    {
        if (this.m_PendingPhoto != null)
            return await this.ApiAccess.SendRequestAsync<SetRecipeImageWebAppRequest, bool>(
                new SetRecipeImageWebAppRequest() { Image = new MemoryStream(this.m_PendingPhoto) },
                ApiProvider.SetRecipeImage(this.RecipeID),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token) == true;

        if (this.m_RemovePhoto)
            return await this.ApiAccess.SendRequestAsync<object, bool>(
                null!, ApiProvider.DeleteRecipeImage(this.RecipeID),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token) == true;

        return true;
    }

    private bool HasPhoto()
        => this.m_Recipe?.ImageVersion != null;

    // Ingredients

    /// <summary>
    /// Whether the list gives its amounts a column. A recipe whose ingredients are all unmeasured
    /// would otherwise indent every name past an empty gutter for no reason.
    /// </summary>
    private bool ShowsIngredientAmounts()
        => this.m_Recipe?.Ingredients.Any(i => !string.IsNullOrEmpty(RecipeDisplayLogic.DescribeAmount(i))) == true;

    /// <summary>
    /// The household's whole larder in one call when the recipe opens, filtered on the device as
    /// the name is typed — the same trade the shopping list makes, for the same reason.
    /// </summary>
    private async Task LoadIngredientSuggestionsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetIngredientSuggestionsWebAppResponse>(
            null!, ApiProvider.GetIngredientSuggestions(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_IngredientSuggestions = [.. _Result.Suggestions];
    }

    /// <summary>
    /// What the name box is offering right now. Ingredients already in this recipe are left out —
    /// they are the one thing being written that cannot be the answer.
    /// </summary>
    private IEnumerable<GetIngredientSuggestionDto> VisibleIngredientSuggestions()
    {
        if (!this.m_ShowIngredientSuggestions || this.m_EditingIngredientID.HasValue)
            return [];

        var _AlreadyIn = (this.m_Recipe?.Ingredients ?? [])
            .Select(i => i.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var _Candidates = this.m_IngredientSuggestions.Where(s => !_AlreadyIn.Contains(s.Name));
        var _Typed = this.m_IngName.Trim();

        return _Typed.Length == 0
            ? _Candidates.Take(IngredientSuggestionsShown)
            : _Candidates
                .Where(s => s.Name.Contains(_Typed, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.Name.StartsWith(_Typed, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenByDescending(s => s.TimesUsed)
                .Take(IngredientSuggestionsShown);
    }

    /// <summary>
    /// Picking one brings the amount it was last written with, because the same ingredient is
    /// usually wanted in the same quantity. Anything already typed into the amount box wins.
    /// </summary>
    private void UseIngredientSuggestion(GetIngredientSuggestionDto suggestion)
    {
        this.m_IngName = suggestion.Name;

        if (this.m_IngAmount.Length == 0 && suggestion.Amount != null)
        {
            this.m_IngAmount = suggestion.Amount.Value.ToString("0.##", CultureInfo.InvariantCulture);
            this.m_IngUnit = (suggestion.Unit ?? MeasurementUnits.All[0].Value).ToString();
        }

        this.m_ShowIngredientSuggestions = false;
    }

    private void OpenAddIngredientModal()
    {
        this.m_EditingIngredientID = null;
        this.m_IngName = string.Empty;
        this.m_IngAmount = string.Empty;
        this.m_IngUnit = MeasurementUnits.All[0].Value.ToString();
        this.m_FocusIngredientName = true;
        this.m_ShowIngredient = true;
    }

    private void OpenEditIngredientModal(RecipeIngredientDto ingredient)
    {
        this.m_EditingIngredientID = ingredient.IngredientID;
        this.m_IngName = ingredient.Name;
        this.m_IngAmount = (ingredient.Amount ?? ingredient.Quantity)?.ToString("0.##") ?? string.Empty;
        this.m_IngUnit = (ingredient.Unit ?? MeasurementUnits.All[0].Value).ToString();
        this.m_ShowIngredient = true;
    }

    private async Task SaveIngredientAsync()
    {
        if (this.m_Saving || string.IsNullOrWhiteSpace(this.m_IngName)) return;
        this.m_Saving = true;

        bool _Result;

        if (this.m_EditingIngredientID.HasValue)
        {
            var _Request = new UpdateRecipeIngredientWebAppRequest()
            {
                Amount = new PropertyChangeTracker<decimal?>(ParseDecimal(this.m_IngAmount)),
                Name = new PropertyChangeTracker<string>(this.m_IngName),
                Unit = new PropertyChangeTracker<long?>(ParseLong(this.m_IngUnit))
            };

            _Result = await this.ApiAccess.SendRequestAsync<UpdateRecipeIngredientWebAppRequest, bool>(
                _Request, ApiProvider.UpdateRecipeIngredient(this.m_EditingIngredientID.Value),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token) == true;
        }
        else
        {
            var _Request = new AddRecipeIngredientWebAppRequest()
            {
                Amount = ParseDecimal(this.m_IngAmount),
                Name = this.m_IngName,
                RecipeID = this.RecipeID,
                Unit = ParseLong(this.m_IngUnit)
            };

            var _Response = await this.ApiAccess.SendRequestAsync<AddRecipeIngredientWebAppRequest, AddRecipeIngredientWebAppResponse>(
                _Request, ApiProvider.AddRecipeIngredient(),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token);

            _Result = _Response != null;
        }

        this.m_Saving = false;

        if (!_Result) return;

        // Adding chains: the modal stays open with the cursor back in the name box, because the
        // next ingredient is nearly always right behind this one. Editing closes as before.
        if (this.m_EditingIngredientID.HasValue)
        {
            this.m_ShowIngredient = false;
        }
        else
        {
            this.m_IngName = string.Empty;
            this.m_IngAmount = string.Empty;
            this.m_IngUnit = MeasurementUnits.All[0].Value.ToString();
            this.m_FocusIngredientName = true;
        }

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

    /// <summary>
    /// Swaps an ingredient with its neighbour, the same two-call swap the board uses to move a
    /// column. The list is already in sequence order, so the neighbour is simply the next row.
    /// </summary>
    private async Task MoveIngredientAsync(RecipeIngredientDto ingredient, int direction)
    {
        if (this.m_Saving || this.m_Recipe == null)
            return;

        var _Ingredients = this.m_Recipe.Ingredients.ToList();
        var _Index = _Ingredients.IndexOf(ingredient);
        var _TargetIndex = _Index + direction;

        if (_Index < 0 || _TargetIndex < 0 || _TargetIndex >= _Ingredients.Count)
            return;

        var _Target = _Ingredients[_TargetIndex];
        this.m_Saving = true;

        var _Moved = await this.SetIngredientSequenceAsync(ingredient, _Target.Sequence);

        if (_Moved)
            _ = await this.SetIngredientSequenceAsync(_Target, ingredient.Sequence);

        this.m_Saving = false;

        if (_Moved)
            await this.ReloadAndPublishAsync();
    }

    /// <summary>
    /// Swaps a step with its neighbour, the same two-call swap the ingredients and the board use.
    /// </summary>
    private async Task MoveStepAsync(RecipeStepDto step, int direction)
    {
        if (this.m_Saving || this.m_Recipe == null)
            return;

        var _Steps = this.m_Recipe.Steps.OrderBy(s => s.Sequence).ToList();
        var _Index = _Steps.FindIndex(s => s.RecipeStepID == step.RecipeStepID);
        var _TargetIndex = _Index + direction;

        if (_Index < 0 || _TargetIndex < 0 || _TargetIndex >= _Steps.Count)
            return;

        var _Target = _Steps[_TargetIndex];
        this.m_Saving = true;

        var _Moved = await this.SetStepSequenceAsync(step, _Target.Sequence);

        if (_Moved)
            _ = await this.SetStepSequenceAsync(_Target, step.Sequence);

        this.m_Saving = false;

        if (_Moved)
            await this.ReloadAndPublishAsync();
    }

    /// <summary>
    /// Only the sequence is sent. Content and title are left unset so a reorder cannot overwrite
    /// what someone else is editing on another device.
    /// </summary>
    private async Task<bool> SetStepSequenceAsync(RecipeStepDto step, int sequence)
    {
        var _Request = new UpdateRecipeStepWebAppRequest() { Sequence = new PropertyChangeTracker<int>(sequence) };

        return await this.ApiAccess.SendRequestAsync<UpdateRecipeStepWebAppRequest, bool>(
            _Request, ApiProvider.UpdateRecipeStep(step.RecipeStepID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token) == true;
    }

    private async Task<bool> SetIngredientSequenceAsync(RecipeIngredientDto ingredient, long sequence)
    {
        var _Request = new SetRecipeIngredientSequenceWebAppRequest() { Sequence = sequence };

        return await this.ApiAccess.SendRequestAsync<SetRecipeIngredientSequenceWebAppRequest, bool>(
            _Request, ApiProvider.SetRecipeIngredientSequence(this.RecipeID, ingredient.IngredientID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token) == true;
    }

    /// <summary>
    /// Removing from inside the sheet closes it first, so the row it was opened from is gone by
    /// the time the list comes back rather than sitting under an open modal that edits nothing.
    /// </summary>
    private async Task RemoveEditingIngredientAsync()
    {
        if (!this.m_EditingIngredientID.HasValue)
            return;

        var _IngredientID = this.m_EditingIngredientID.Value;

        this.m_ShowIngredient = false;

        await this.RemoveIngredientAsync(_IngredientID);
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
        if (this.m_Saving || string.IsNullOrWhiteSpace(this.m_StepContent)) return;
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
                this.m_CancellationTokenHandler.Token) == true;
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

        // A method is a run of steps, so adding chains the same way ingredients do.
        if (this.m_EditingStepID.HasValue)
        {
            this.m_ShowStep = false;
        }
        else
        {
            this.m_StepTitle = string.Empty;
            this.m_StepContent = string.Empty;
        }

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
        this.m_EditingNoteID = null;
        this.m_NoteContent = string.Empty;
        this.m_ShowNote = true;
    }

    private void OpenEditNoteModal(RecipeNoteDto note)
    {
        this.m_EditingNoteID = note.NoteID;
        this.m_NoteContent = note.Content;
        this.m_ShowNote = true;
    }

    /// <summary>
    /// One sheet for both, because writing a note and fixing one are the same act — the only
    /// difference is which call it ends in.
    /// </summary>
    private async Task SaveNoteAsync()
    {
        if (this.m_Saving || string.IsNullOrWhiteSpace(this.m_NoteContent)) return;
        this.m_Saving = true;

        bool _Result;

        if (this.m_EditingNoteID.HasValue)
        {
            _Result = await this.ApiAccess.SendRequestAsync<UpdateNoteWebAppRequest, bool>(
                new UpdateNoteWebAppRequest() { Content = new PropertyChangeTracker<string>(this.m_NoteContent) },
                ApiProvider.UpdateNote(this.m_EditingNoteID.Value),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token) == true;
        }
        else
        {
            var _Request = new AddRecipeNoteWebAppRequest()
            {
                Content = this.m_NoteContent,
                RecipeID = this.RecipeID
            };

            _Result = await this.ApiAccess.SendRequestAsync<AddRecipeNoteWebAppRequest, AddRecipeNoteWebAppResponse>(
                _Request, ApiProvider.AddRecipeNote(),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token) != null;
        }

        this.m_Saving = false;

        if (!_Result) return;

        this.m_ShowNote = false;
        await this.ReloadAndPublishAsync();
    }

    /// <summary>
    /// Removing from inside the sheet closes it first, so the note it was opened from is gone by
    /// the time the list comes back.
    /// </summary>
    private async Task RemoveEditingNoteAsync()
    {
        if (!this.m_EditingNoteID.HasValue)
            return;

        var _NoteID = this.m_EditingNoteID.Value;

        this.m_ShowNote = false;

        await this.RemoveNoteAsync(_NoteID);
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
        this.m_SelectedIngredientIDs = [.. (this.m_Recipe?.Ingredients ?? []).Select(i => i.IngredientID)];
        this.m_ShowAddToList = true;

        this.m_ShoppingLists = await this.ApiAccess.SendRequestAsync<object, GetShoppingListsWebAppResponse>(
            null!, ApiProvider.GetShoppingLists(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);
    }

    private void ToggleIngredient(long ingredientID, ChangeEventArgs args)
    {
        if (args.Value is true)
            _ = this.m_SelectedIngredientIDs.Add(ingredientID);
        else
            _ = this.m_SelectedIngredientIDs.Remove(ingredientID);
    }

    private bool AllIngredientsTicked()
        => this.m_Recipe != null
            && this.m_Recipe.Ingredients.Count > 0
            && this.m_Recipe.Ingredients.All(i => this.m_SelectedIngredientIDs.Contains(i.IngredientID));

    private void ToggleAllIngredients()
        => this.m_SelectedIngredientIDs = this.AllIngredientsTicked()
            ? []
            : [.. (this.m_Recipe?.Ingredients ?? []).Select(i => i.IngredientID)];

    private async Task AddToExistingListAsync(long shoppingListID)
    {
        if (this.m_AddingToList || this.m_SelectedIngredientIDs.Count == 0) return;
        this.m_AddingToList = true;

        var _Result = await this.SendIngredientsAsync(shoppingListID);

        this.m_AddingToList = false;

        if (_Result != true)
            return;

        this.m_ShowAddToList = false;
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.m_CancellationTokenHandler.Token);
    }

    private async Task CreateListAndAddAsync()
    {
        if (this.m_AddingToList) return;
        if (string.IsNullOrWhiteSpace(this.m_NewListName) || this.m_SelectedIngredientIDs.Count == 0) return;
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

        var _Result = await this.SendIngredientsAsync(_Created.ShoppingListID);

        this.m_AddingToList = false;

        if (_Result != true)
            return;

        this.m_ShowAddToList = false;
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.m_CancellationTokenHandler.Token);
    }

    private async Task<bool?> SendIngredientsAsync(long shoppingListID)
        => await this.ApiAccess.SendRequestAsync<AddRecipeToShoppingListWebAppRequest, bool>(
            new AddRecipeToShoppingListWebAppRequest() { IngredientIDs = [.. this.m_SelectedIngredientIDs] },
            ApiProvider.AddRecipeIngredientsToShoppingList(shoppingListID, this.RecipeID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

    // Helpers

    private static decimal? ParseDecimal(string value)
        => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var _Parsed) ? _Parsed : null;

    private static int? ParseInt(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var _Parsed) ? _Parsed : null;

    private static long? ParseLong(string value)
        => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var _Parsed) ? _Parsed : null;

    #endregion Methods

}
