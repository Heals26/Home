namespace Home.Application.UseCases.Households.GetSetupStatus;

public interface IGetSetupStatusOutputPort
{

    #region Methods

    Task PresentSetupStatusAsync(bool requiresSetup, CancellationToken cancellationToken);

    #endregion Methods

}
