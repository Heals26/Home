namespace Home.Application.UseCases.LightGroups.AssignLightToGroup;

public interface IAssignLightToGroupOutputPort
{

    #region Methods

    Task PresentLightAssignedAsync(CancellationToken cancellationToken);
    Task PresentLightGroupNotFoundAsync(long lightGroupID, CancellationToken cancellationToken);
    Task PresentLightNotFoundAsync(string lightID, CancellationToken cancellationToken);

    #endregion Methods

}
