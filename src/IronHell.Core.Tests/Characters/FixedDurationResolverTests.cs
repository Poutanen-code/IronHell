using IronHell.Core.Characters;
using IronHell.Core.Definitions;
using Xunit;

namespace IronHell.Core.Tests.Characters;

public sealed class FixedDurationResolverTests
{
    [Fact]
    public void ResolveDuration_ReturnsDefinitionFixedDuration()
    {
        var definition = new StatusDefinition(
            "blessed",
            StatusApplicationPolicy.ReplaceExisting,
            new StatusDurationDefinition(12, DiceCount: 1, DiceSides: 12, LevelMultiplier: null));

        var duration = new FixedDurationResolver().ResolveDuration(definition, new DurationResolutionContext(1));

        Assert.Equal(12, duration);
    }

    [Fact]
    public void ResolveDuration_IdenticalDefinitions_ReturnIdenticalDurations()
    {
        var definition = new StatusDefinition(
            "blessed",
            StatusApplicationPolicy.ReplaceExisting,
            new StatusDurationDefinition(12, DiceCount: 1, DiceSides: 12, LevelMultiplier: null));
        var resolver = new FixedDurationResolver();

        Assert.Equal(
            resolver.ResolveDuration(definition, new DurationResolutionContext(1)),
            resolver.ResolveDuration(definition, new DurationResolutionContext(1)));
    }
}