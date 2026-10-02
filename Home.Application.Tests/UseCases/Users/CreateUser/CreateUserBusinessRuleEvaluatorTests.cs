using CleanArchitecture.Mediator;
using FluentAssertions;
using Home.Application.Services.Persistence;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Users.CreateUser;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.Users.CreateUser;

public class CreateUserBusinessRuleEvaluatorTests
{

    #region Fields

    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<ICreateUserOutputPort> m_OutputPort = new();

    #endregion Fields

    #region Methods

    private Task<ContinuationBehaviour> EvaluateAsync(string email, User[]? existingUsers = null)
    {
        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<User>())
            .Returns((existingUsers ?? []).AsQueryable());

        _ = this.m_OutputPort
            .Setup(o => o.PresentUserConflictAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ContinuationBehaviour.Return);

        var _ServiceFactory = new TestServiceFactory()
            .With(this.m_PersistenceContext.Object)
            .Build();

        IBusinessRuleEvaluator<CreateUserInputPort, ICreateUserOutputPort> _Evaluator = new CreateUserBusinessRuleEvaluator();

        return _Evaluator.EvaluateAsync(
            new CreateUserInputPort(email, "Mitch", "Healy", string.Empty, "hunter2"),
            this.m_OutputPort.Object,
            _ServiceFactory,
            CancellationToken.None);
    }

    [Fact]
    public async Task EvaluateAsync_ContinuesWhenTheEmailIsUnused()
    {
        var _Continuation = await this.EvaluateAsync(
            "mitch@example.test",
            [new User() { Email = "someone.else@example.test" }]);

        _Continuation.Should().Be(ContinuationBehaviour.Continue);
        this.m_OutputPort.Verify(
            o => o.PresentUserConflictAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_PresentsAConflictWhenTheEmailIsTakenInAnyCasing()
    {
        var _Continuation = await this.EvaluateAsync(
            "MITCH@example.test",
            [new User() { Email = "mitch@EXAMPLE.test" }]);

        _Continuation.Should().Be(ContinuationBehaviour.Return);
        this.m_OutputPort.Verify(
            o => o.PresentUserConflictAsync("MITCH@example.test", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion Methods

}
