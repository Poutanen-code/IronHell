using IronHell.Core.Definitions;

namespace IronHell.Core.Characters;

public sealed class FixedDurationResolver : IDurationResolver
{
    public int ResolveDuration(StatusDefinition definition, DurationResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(context);
        return definition.Duration.FixedDuration;
    }
}