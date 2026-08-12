namespace Home.Application.UseCases.LightGroups.UpdateLightGroup;

public interface IUpdateLightGroupOutputPort
{

    #region Methods

    Task PresentLightGroupNotFoundAsync(long lightGroupID, CancellationToken cancellationToken);
    Task PresentLightGroupUpdatedAsync(CancellationToken cancellationToken);

    #endregion Methods

}
