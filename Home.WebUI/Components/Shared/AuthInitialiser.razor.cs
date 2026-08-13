namespace Home.WebUI.Components.Shared;

public partial class AuthInitialiser
{

    #region Lifecycle Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await this.AuthorisationService.InitialiseAsync();
    }

    #endregion Lifecycle Methods

}
