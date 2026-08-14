using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using System.Text.Json;

namespace Home.Application.Tests.Infrastructure.ChangeTrackers;

public class PropertyChangeTrackerJsonConverterTests
{

    #region Methods

    [Fact]
    public void Deserialise_HonoursCamelCasePropertyNamesFromTheWebUISerialiser()
    {
        var _Tracker = JsonSerializer.Deserialize<PropertyChangeTracker<string>>(
            """{"hasBeenSet":true,"value":"abc"}""");

        _Tracker.HasBeenSet.Should().BeTrue();
        _Tracker.Value.Should().Be("abc");
    }

    [Fact]
    public void Deserialise_KeepsAnUnsetPropertyUnset()
    {
        var _Tracker = JsonSerializer.Deserialize<PropertyChangeTracker<string>>(
            """{"HasBeenSet":false,"Value":null}""");

        _Tracker.HasBeenSet.Should().BeFalse();
    }

    [Fact]
    public void Deserialise_TreatsASetNullValueAsSet()
    {
        var _Tracker = JsonSerializer.Deserialize<PropertyChangeTracker<double?>>(
            """{"HasBeenSet":true,"Value":null}""");

        _Tracker.HasBeenSet.Should().BeTrue();
        _Tracker.Value.Should().BeNull();
    }

    [Fact]
    public void RoundTrip_OnlyTheAssignedPropertySurvivesAsSet()
    {
        var _InputPort = JsonSerializer.Deserialize<UpdateSettingsStandIn>(
            JsonSerializer.Serialize(new UpdateSettingsStandIn { LifxApiToken = new("token-123") }));

        _InputPort!.LifxApiToken.HasBeenSet.Should().BeTrue();
        _InputPort.LifxApiToken.Value.Should().Be("token-123");
        _InputPort.Name.HasBeenSet.Should().BeFalse();
    }

    #endregion Methods

    #region Nested Types

    private class UpdateSettingsStandIn
    {
        public PropertyChangeTracker<string> LifxApiToken { get; set; }
        public PropertyChangeTracker<string> Name { get; set; }
    }

    #endregion Nested Types

}
