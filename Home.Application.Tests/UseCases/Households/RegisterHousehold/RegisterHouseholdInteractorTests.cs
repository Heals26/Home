using FluentAssertions;
using Home.Application.Infrastructure.Households;
using Home.Application.Services.EntityLogic.Households;
using Home.Application.Services.Persistence;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Households.RegisterHousehold;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.Domain.Services.Users;
using Moq;

namespace Home.Application.Tests.UseCases.Households.RegisterHousehold;

public class RegisterHouseholdInteractorTests
{

    #region Fields

    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<IPasswordService> m_PasswordService = new();
    private readonly Mock<IAuditLogic<User>> m_AuditLogic = new();
    private readonly Mock<IRegisterHouseholdOutputPort> m_OutputPort = new();

    #endregion Fields

    #region Methods

    private Task HandleAsync(User[]? existingUsers = null)
    {
        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<User>())
            .Returns((existingUsers ?? []).AsQueryable());

        // The real setup logic, not a mock: a household that arrives without board columns or
        // meal slots is unusable on the first screen it shows, so that is worth covering here.
        var _ServiceFactory = new TestServiceFactory()
            .With(this.m_PersistenceContext.Object)
            .With(this.m_PasswordService.Object)
            .With(this.m_AuditLogic.Object)
            .With<IHouseholdSetupLogic>(new HouseholdSetupLogic(this.m_PersistenceContext.Object))
            .Build();

        return new RegisterHouseholdInteractor().HandleAsync(
            new RegisterHouseholdInputPort("mitch@example.test", " Mitch ", " The Healys ", " Healy ", "hunter2"),
            this.m_OutputPort.Object,
            _ServiceFactory,
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_SeedsTheHouseholdsOwnBoardColumnsAndMealSlots()
    {
        Household? _Added = null;
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<Household>()))
            .Callback<Household>(h => _Added = h);

        await this.HandleAsync();

        _Added.Should().NotBeNull();
        _Added!.ActivityStates.Select(s => s.Name).Should().Equal("To do", "Doing", "Waiting on", "Done");
        _Added.ActivityStates.Should().ContainSingle(s => s.IsComplete).Which.Name.Should().Be("Done");
        _Added.ActivityStates.Select(s => s.Sequence).Should().Equal(0, 1, 2, 3);
        _Added.MealSlots.Select(s => s.Name).Should().Equal("Breakfast", "Lunch", "Dinner", "Snack");
    }

    [Fact]
    public async Task HandleAsync_ClosesRegistrationOnceAnyUserExists()
    {
        await this.HandleAsync([new User() { UserID = 1 }]);

        this.m_OutputPort.Verify(
            o => o.PresentRegistrationClosedAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        this.m_PersistenceContext.Verify(c => c.Add(It.IsAny<Household>()), Times.Never);
        this.m_PersistenceContext.Verify(c => c.Add(It.IsAny<User>()), Times.Never);
        this.m_PersistenceContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_CreatesTheHouseholdAndItsFirstMember()
    {
        Household? _Household = null;
        User? _User = null;
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<Household>()))
            .Callback<Household>(h => _Household = h);
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<User>()))
            .Callback<User>(u => _User = u);

        await this.HandleAsync();

        _Household.Should().NotBeNull();
        _Household!.Name.Should().Be("The Healys");
        _User.Should().NotBeNull();
        _User!.Email.Should().Be("mitch@example.test");
        _User.FirstName.Should().Be("Mitch");
        _User.LastName.Should().Be("Healy");
        _User.MiddleNames.Should().BeEmpty();
        _User.Household.Should().BeSameAs(_Household);
    }

    [Fact]
    public async Task HandleAsync_SetsThePasswordThroughThePasswordService()
    {
        await this.HandleAsync();

        this.m_PasswordService.Verify(
            p => p.SetPassword(It.IsAny<User>(), "hunter2"),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AuditsTheFirstMember()
    {
        await this.HandleAsync();

        this.m_AuditLogic.Verify(a => a.AddAudit(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_PresentsTheIDAssignedDuringSave()
    {
        Household? _Household = null;
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<Household>()))
            .Callback<Household>(h => _Household = h);
        this.m_PersistenceContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _Household!.HouseholdID = 7)
            .ReturnsAsync(1);

        await this.HandleAsync();

        this.m_OutputPort.Verify(
            o => o.PresentHouseholdRegisteredAsync(7, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion Methods

}
