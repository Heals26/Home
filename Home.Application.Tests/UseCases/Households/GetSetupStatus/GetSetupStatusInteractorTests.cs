using FluentAssertions;
using Home.Application.Services.Persistence;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Households.GetSetupStatus;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.Households.GetSetupStatus;

public class GetSetupStatusInteractorTests
{

    #region Fields

    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<IGetSetupStatusOutputPort> m_OutputPort = new();

    #endregion Fields

    #region Methods

    private Task HandleAsync(params User[] users)
    {
        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<User>())
            .Returns(users.AsQueryable());

        var _ServiceFactory = new TestServiceFactory()
            .With(this.m_PersistenceContext.Object)
            .Build();

        return new GetSetupStatusInteractor().HandleAsync(
            new GetSetupStatusInputPort(),
            this.m_OutputPort.Object,
            _ServiceFactory,
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_RequiresSetupWhileNoUsersExist()
    {
        await this.HandleAsync();

        this.m_OutputPort.Verify(
            o => o.PresentSetupStatusAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DoesNotRequireSetupOnceAUserExists()
    {
        await this.HandleAsync(new User() { UserID = 1 });

        this.m_OutputPort.Verify(
            o => o.PresentSetupStatusAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion Methods

}
