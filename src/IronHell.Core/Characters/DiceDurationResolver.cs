using IronHell.Core.Definitions;
using IronHell.Core.Randomness;

namespace IronHell.Core.Characters;

public sealed class DiceDurationResolver : IDurationResolver
{
    private readonly IRandomSource _randomSource;

    public DiceDurationResolver(IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(randomSource);
        _randomSource = randomSource;
    }

    public int ResolveDuration(StatusDefinition definition, DurationResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(context);

        var duration = definition.Duration;
        if (duration.DiceCount is null || duration.DiceSides is null)
        {
            return duration.FixedDuration;
        }

        return duration.FixedDuration + _randomSource.RollDice(duration.DiceCount.Value, duration.DiceSides.Value);
    }
}