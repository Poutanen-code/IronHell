using IronHell.Core.Randomness;

namespace IronHell.Core.Monsters;

public static class MonsterAllocationLevelResolver
{
    public static int ResolveEffectiveLevel(int requestedLevel, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(randomSource);

        if (requestedLevel <= 0)
        {
            return requestedLevel;
        }

        var effectiveLevel = requestedLevel;
        for (var roll = 0; roll < 2; roll++)
        {
            if (randomSource.Next(0, 50) == 0)
            {
                effectiveLevel += Math.Min(effectiveLevel / 4 + 2, 5);
            }
        }

        return effectiveLevel;
    }
}
