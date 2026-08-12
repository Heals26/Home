using CleanArchitecture.Mediator;
using FluentAssertions;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.LightGroups.AssignLightToGroup;
using Home.Application.UseCases.LightGroups.CreateLightGroup;
using Home.Application.UseCases.LightGroups.DeleteLightGroup;
using Home.Application.UseCases.LightGroups.UpdateLightGroup;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.LightGroups;

/// <summary>
/// Create / rename / reorder / delete / reassign. Grouped in one fixture because they share the
/// same small object graph and none of them touch the provider.
/// </summary>
public class LightGroupManagementTests
{

    #region Fields

    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<IAuthorisationService> m_AuthorisationService = new();
    private readonly Household m_Household = new() { HouseholdID = 42 };
    private readonly LightLocation m_Location;

    #endregion Fields

    #region Constructors

    public LightGroupManagementTests()
    {
        this.m_Location = new LightLocation()
        {
            ID = "loc-1",
            Name = "Home",
            Household = this.m_Household,
            Groups = []
        };

        _ = this.m_AuthorisationService.Setup(a => a.GetHousehold()).Returns(this.m_Household);
    }

    #endregion Constructors

    #region Methods

    private LightGroup BuildGroup(long id, string name, int sequence, params Light[] lights)
    {
        var _Group = new LightGroup()
        {
            LightGroupID = id,
            Name = name,
            Sequence = sequence,
            Lights = [.. lights],
            Location = this.m_Location
        };

        this.m_Location.Groups.Add(_Group);

        foreach (var _Light in lights)
            _Light.Group = _Group;

        return _Group;
    }

    private ServiceFactory Factory()
        => new TestServiceFactory()
            .With(this.m_PersistenceContext.Object)
            .With(this.m_AuthorisationService.Object)
            .Build();

    /* ---------- create ---------- */

    [Fact]
    public async Task CreateLightGroup_AppendsAfterTheHighestExistingSequence()
    {
        _ = this.BuildGroup(1, "Bedroom", 0);
        _ = this.BuildGroup(2, "Kitchen", 5);

        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<LightLocation>())
            .Returns(new[] { this.m_Location }.AsQueryable());

        LightGroup? _Added = null;
        this.m_PersistenceContext.Setup(c => c.Add(It.IsAny<LightGroup>())).Callback<LightGroup>(g => _Added = g);

        var _OutputPort = new Mock<ICreateLightGroupOutputPort>();

        await new CreateLightGroupInteractor().HandleAsync(
            new CreateLightGroupInputPort("  Reading Nook  "),
            _OutputPort.Object, this.Factory(), CancellationToken.None);

        _Added.Should().NotBeNull();
        _Added!.Name.Should().Be("Reading Nook", "the name should be trimmed");
        _Added.Sequence.Should().Be(6);
        _Added.ID.Should().BeNull("a group created in Home has no provider ID");
    }

    [Fact]
    public async Task CreateLightGroup_WithNoLocationYet_TellsTheCallerToSyncFirst()
    {
        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<LightLocation>())
            .Returns(Array.Empty<LightLocation>().AsQueryable());

        var _OutputPort = new Mock<ICreateLightGroupOutputPort>();

        await new CreateLightGroupInteractor().HandleAsync(
            new CreateLightGroupInputPort("Reading Nook"),
            _OutputPort.Object, this.Factory(), CancellationToken.None);

        _OutputPort.Verify(o => o.PresentNoLocationAsync(It.IsAny<CancellationToken>()), Times.Once);
        this.m_PersistenceContext.Verify(c => c.Add(It.IsAny<LightGroup>()), Times.Never);
    }

    /* ---------- update ---------- */

    [Fact]
    public async Task UpdateLightGroup_RenamesWithoutDisturbingSequence()
    {
        var _Group = this.BuildGroup(1, "Bedroom", 3);

        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<LightGroup>())
            .Returns(new[] { _Group }.AsQueryable());

        var _OutputPort = new Mock<IUpdateLightGroupOutputPort>();

        await new UpdateLightGroupInteractor().HandleAsync(
            new UpdateLightGroupInputPort(1, new("Main Bedroom"), new()),
            _OutputPort.Object, this.Factory(), CancellationToken.None);

        _Group.Name.Should().Be("Main Bedroom");
        _Group.Sequence.Should().Be(3);
    }

    [Fact]
    public async Task UpdateLightGroup_ForAnUnknownGroup_PresentsNotFound()
    {
        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<LightGroup>())
            .Returns(Array.Empty<LightGroup>().AsQueryable());

        var _OutputPort = new Mock<IUpdateLightGroupOutputPort>();

        await new UpdateLightGroupInteractor().HandleAsync(
            new UpdateLightGroupInputPort(99, new("Nope"), new()),
            _OutputPort.Object, this.Factory(), CancellationToken.None);

        _OutputPort.Verify(o => o.PresentLightGroupNotFoundAsync(99, It.IsAny<CancellationToken>()), Times.Once);
    }

    /* ---------- delete ---------- */

    [Fact]
    public async Task DeleteLightGroup_RefusesWhileItStillHasLights()
    {
        // The FK cascades, so deleting a populated group would take the bulbs with it.
        var _Group = this.BuildGroup(1, "Bedroom", 0, new Light { ID = "a", Name = "Bedside" });

        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<LightGroup>())
            .Returns(new[] { _Group }.AsQueryable());

        var _OutputPort = new Mock<IDeleteLightGroupOutputPort>();

        await new DeleteLightGroupInteractor().HandleAsync(
            new DeleteLightGroupInputPort(1),
            _OutputPort.Object, this.Factory(), CancellationToken.None);

        _OutputPort.Verify(o => o.PresentLightGroupNotEmptyAsync(1, 1, It.IsAny<CancellationToken>()), Times.Once);
        this.m_PersistenceContext.Verify(c => c.Remove(It.IsAny<LightGroup>()), Times.Never);
    }

    [Fact]
    public async Task DeleteLightGroup_RemovesAnEmptyGroup()
    {
        var _Group = this.BuildGroup(1, "Spare Room", 0);

        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<LightGroup>())
            .Returns(new[] { _Group }.AsQueryable());

        var _OutputPort = new Mock<IDeleteLightGroupOutputPort>();

        await new DeleteLightGroupInteractor().HandleAsync(
            new DeleteLightGroupInputPort(1),
            _OutputPort.Object, this.Factory(), CancellationToken.None);

        this.m_PersistenceContext.Verify(c => c.Remove(_Group), Times.Once);
        _OutputPort.Verify(o => o.PresentLightGroupDeletedAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /* ---------- assign ---------- */

    [Fact]
    public async Task AssignLightToGroup_MovesTheBulbBetweenGroups()
    {
        var _Light = new Light() { ID = "bulb-1", Name = "Bedside" };
        _ = this.BuildGroup(1, "Bedroom", 0, _Light);
        var _Target = this.BuildGroup(2, "Reading Nook", 1);

        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<LightGroup>())
            .Returns(this.m_Location.Groups.AsQueryable());
        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<Light>())
            .Returns(new[] { _Light }.AsQueryable());

        var _OutputPort = new Mock<IAssignLightToGroupOutputPort>();

        await new AssignLightToGroupInteractor().HandleAsync(
            new AssignLightToGroupInputPort("bulb-1", 2),
            _OutputPort.Object, this.Factory(), CancellationToken.None);

        _Light.Group.Should().BeSameAs(_Target);
        this.m_PersistenceContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignLightToGroup_ForAnUnknownGroup_LeavesTheBulbWhereItWas()
    {
        var _Light = new Light() { ID = "bulb-1", Name = "Bedside" };
        var _Original = this.BuildGroup(1, "Bedroom", 0, _Light);

        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<LightGroup>())
            .Returns(this.m_Location.Groups.AsQueryable());
        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<Light>())
            .Returns(new[] { _Light }.AsQueryable());

        var _OutputPort = new Mock<IAssignLightToGroupOutputPort>();

        await new AssignLightToGroupInteractor().HandleAsync(
            new AssignLightToGroupInputPort("bulb-1", 99),
            _OutputPort.Object, this.Factory(), CancellationToken.None);

        _Light.Group.Should().BeSameAs(_Original);
        _OutputPort.Verify(o => o.PresentLightGroupNotFoundAsync(99, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion Methods

}
