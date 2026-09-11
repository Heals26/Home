using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.ShoppingCategories.CreateShoppingCategory;
using Home.WebUI.DataAccess.ShoppingCategories.GetShoppingCategories;
using Home.WebUI.DataAccess.ShoppingCategories.Models;
using Home.WebUI.DataAccess.ShoppingCategories.UpdateShoppingCategory;
using Home.WebUI.Infrastructure.ApiProviders;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.ShoppingList;

public partial class ShoppingAislesModal
{

    #region Fields

    private ErrorHandler? m_ErrorHandler;
    private List<ShoppingCategoryDto> m_Aisles = [];
    private bool m_Loading;
    private bool m_WasVisible;

    private string m_NewAisleName = string.Empty;
    private string? m_NewAisleError;
    private string? m_RenameError;
    private long? m_RenameErrorAisleID;
    private bool m_Saving;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Raised after every change, so whatever shows a list by aisle can read it again.
    /// </summary>
    [Parameter] public EventCallback OnAislesChanged { get; set; }

    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    /// <summary>
    /// Read afresh each time it opens, so an aisle added on another phone is there to move.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        var _Opening = this.Visible && !this.m_WasVisible;

        this.m_WasVisible = this.Visible;

        if (!_Opening)
            return;

        this.m_NewAisleName = string.Empty;
        this.m_NewAisleError = null;
        this.m_RenameError = null;
        this.m_RenameErrorAisleID = null;

        await this.LoadAislesAsync();
    }

    #endregion Lifecycle Methods

    #region Methods

    private async Task LoadAislesAsync()
    {
        this.m_Loading = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, GetShoppingCategoriesWebAppResponse>(
            null!, ApiProvider.GetShoppingCategories(),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Loading = false;

        if (_Result != null)
            this.m_Aisles = [.. _Result.ShoppingCategories];
    }

    private async Task CreateAisleAsync()
    {
        var _Name = this.m_NewAisleName.Trim();

        if (this.m_Saving || _Name.Length == 0)
            return;

        this.m_NewAisleError = this.AisleLogic.FindNameClash(this.m_Aisles, _Name, null) is { } _Clash
            ? $"There is already an aisle called {_Clash.Name}."
            : null;

        if (this.m_NewAisleError != null)
            return;

        this.m_Saving = true;

        var _Result = await this.ApiAccess.SendRequestAsync<CreateShoppingCategoryWebAppRequest, CreateShoppingCategoryWebAppResponse>(
            new CreateShoppingCategoryWebAppRequest() { Name = _Name },
            ApiProvider.CreateShoppingCategory(),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Saving = false;

        if (_Result == null)
            return;

        this.m_NewAisleName = string.Empty;

        await this.ReloadAsync();
    }

    private async Task RenameAisleAsync(ShoppingCategoryDto aisle, string name)
    {
        var _Name = name.Trim();

        this.m_RenameErrorAisleID = aisle.ShoppingCategoryID;
        this.m_RenameError = _Name.Length == 0
            ? "Give the aisle a name."
            : this.AisleLogic.FindNameClash(this.m_Aisles, _Name, aisle.ShoppingCategoryID) is { } _Clash
                ? $"There is already an aisle called {_Clash.Name}."
                : null;

        if (this.m_RenameError != null || _Name == aisle.Name)
            return;

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateShoppingCategoryWebAppRequest, bool>(
            new UpdateShoppingCategoryWebAppRequest() { Name = new(_Name) },
            ApiProvider.UpdateShoppingCategory(aisle.ShoppingCategoryID),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        if (_Result == true)
            await this.ReloadAsync();
    }

    /// <summary>
    /// Renumbers the whole walk rather than swapping a pair, so a run of moves can never leave two
    /// aisles sharing a place.
    /// </summary>
    private async Task MoveAisleAsync(ShoppingCategoryDto aisle, int direction)
    {
        var _Index = this.m_Aisles.IndexOf(aisle);
        var _Target = _Index + direction;

        if (this.m_Saving || _Index < 0 || _Target < 0 || _Target >= this.m_Aisles.Count)
            return;

        this.m_Saving = true;

        this.m_Aisles.RemoveAt(_Index);
        this.m_Aisles.Insert(_Target, aisle);

        for (var _Sequence = 0; _Sequence < this.m_Aisles.Count; _Sequence++)
        {
            var _Aisle = this.m_Aisles[_Sequence];

            if (_Aisle.Sequence == _Sequence)
                continue;

            _Aisle.Sequence = _Sequence;

            _ = await this.ApiAccess.SendRequestAsync<UpdateShoppingCategoryWebAppRequest, bool>(
                new UpdateShoppingCategoryWebAppRequest() { Sequence = new(_Sequence) },
                ApiProvider.UpdateShoppingCategory(_Aisle.ShoppingCategoryID),
                e => this.m_ErrorHandler?.AddError(e),
                this.CancellationToken);
        }

        this.m_Saving = false;

        await this.OnAislesChanged.InvokeAsync();
    }

    private async Task RemoveAisleAsync(ShoppingCategoryDto aisle)
    {
        if (this.m_Saving)
            return;

        this.m_Saving = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteShoppingCategory(aisle.ShoppingCategoryID),
            e => this.m_ErrorHandler?.AddError(e),
            this.CancellationToken);

        this.m_Saving = false;

        if (_Result == true)
            await this.ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        await this.LoadAislesAsync();
        await this.OnAislesChanged.InvokeAsync();
    }

    #endregion Methods

}
