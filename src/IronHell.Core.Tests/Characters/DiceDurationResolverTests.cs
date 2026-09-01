using IronHell.Core.Characters;
using IronHell.Core.Definitions;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Characters;

public sealed class DiceDurationResolverTests
{
    [Fact]
    public void ResolveDuration_ReturnsBasePlusDeterministicDice()
    {
        var definition = new StatusDefinition(
            "blessed",
            StatusApplicationPolicy.ReplaceExisting,
            new StatusDurationDefinition(10, DiceCount: 2, DiceSides: 6, LevelMultiplier: null));
        var expectedRandom = new SeededRandomSource(12345);
        var resolver = new DiceDurationResolver(new SeededRandomSource(12345));

        var duration = resolver.ResolveDuration(definition, new DurationResolutionContext(1));

        Assert.Equal(10 + expectedRandom.RollDice(2, 6), duration);
    }

    [Fact]
    public void ResolveDuration_WithoutDice_ReturnsFixedDuration()
    {
        var definition = new StatusDefinition(
            "blessed",
            StatusApplicationPolicy.ReplaceExisting,
            new StatusDurationDefinition(10, DiceCount: null, DiceSides: null, LevelMultiplier: null));

        Assert.Equal(10, new DiceDurationResolver(new SeededRandomSource(12345)).ResolveDuration(definition, new DurationResolutionContext(1)));
    }
}