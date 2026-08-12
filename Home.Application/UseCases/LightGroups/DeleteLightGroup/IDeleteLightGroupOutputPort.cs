namespace Home.Application.UseCases.LightGroups.DeleteLightGroup;

public interface IDeleteLightGroupOutputPort
{

    #region Methods

    Task PresentLightGroupDeletedAsync(CancellationToken cancellationToken);
    Task PresentLightGroupNotEmptyAsync(long lightGroupID, int lightCount, CancellationToken cancellationToken);
    Task PresentLightGroupNotFoundAsync(long lightGroupID, CancellationToken cancellationToken);

    #endregion Methods

}
