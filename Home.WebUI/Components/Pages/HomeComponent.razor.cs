namespace Home.WebUI.Components.Pages;

public partial class HomeComponent
{

    #region Lifecycle Methods

    protected override void OnInitialized()
        => this.NavigationManager.NavigateTo("/");

    #endregion Lifecycle Methods

}
