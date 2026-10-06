using CleanArchitecture.Mediator;
using FluentAssertions;
using Home.Application.Services.Persistence;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Households.RegisterHousehold;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.Households.RegisterHousehold;

/// <summary>
/// Signing in finds a member by email across every household, so a new household cannot be made with
/// an email somebody already signs in with.
/// </summary>
public class RegisterHouseholdBusinessRuleEvaluatorTests
{

    #region Fields

    private readonly Mock<IRegisterHouseholdOutputPort> m_OutputPort = new();
    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();

    #endregion Fields

    #region Methods

    private Task<ContinuationBehaviour> EvaluateAsync(string email, params User[] existingUsers)
    {
        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<User>())
            .Returns(existingUsers.AsQueryable());

        _ = this.m_OutputPort
            .Setup(o => o.PresentUserConflictAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ContinuationBehaviour.ReturnAsync);

        return ((IBusinessRuleEvaluator<RegisterHouseholdInputPort, IRegisterHouseholdOutputPort>)new RegisterHouseholdBusinessRuleEvaluator()).EvaluateAsync(
            new RegisterHouseholdInputPort(email, "Claude", "Claude", "Tester", "a-long-password"),
            this.m_OutputPort.Object,
            new TestServiceFactory().With(this.m_PersistenceContext.Object).Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task EvaluateAsync_LetsANewEmailThrough()
    {
        var _Result = await this.EvaluateAsync("claude@home.test", new User() { Email = "mitch@example.test", UserID = 1 });

        _ = _Result.Should().Be(ContinuationBehaviour.Continue);
        this.m_OutputPort.Verify(o => o.PresentUserConflictAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_RefusesAnEmailSomebodyAlreadySignsInWith()
    {
        var _Result = await this.EvaluateAsync(" Mitch@Example.test ", new User() { Email = "mitch@example.test", UserID = 1 });

        _ = _Result.Should().Be(ContinuationBehaviour.Return, "an email differing only in case or spacing is the same email to sign in with");
        this.m_OutputPort.Verify(o => o.PresentUserConflictAsync(" Mitch@Example.test ", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_IsNotStoppedByAMemberWithoutALogin()
    {
        var _Result = await this.EvaluateAsync("claude@home.test", new User() { Email = null, UserID = 1 });

        _ = _Result.Should().Be(ContinuationBehaviour.Continue);
    }

    #endregion Methods

}
