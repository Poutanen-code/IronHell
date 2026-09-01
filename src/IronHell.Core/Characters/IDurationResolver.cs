using IronHell.Core.Definitions;

namespace IronHell.Core.Characters;

public interface IDurationResolver
{
    int ResolveDuration(StatusDefinition definition, DurationResolutionContext context);
}